using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Databento.CSharpApiClient.Caching.Archive
{
    /// <summary>
    /// The list of symbol-days an archive file holds, kept in the Parquet footer's key-value metadata: per day its first row and record count.
    /// A day listed with 0 records is cached as empty and final; a day not listed is not cached. Rows sit in symbol (ordinal)
    /// then day order, and a stored first row that does not match that layout is refused on load.
    /// </summary>
    internal sealed class ArchiveIndex
    {
        /// <summary>
        /// Version of the index JSON; a reader refuses any other
        /// </summary>
        public const int FormatVersion = 1;

        /// <summary>
        /// Key of the index in the Parquet file's key-value metadata
        /// </summary>
        public const string MetadataKey = "databento.archive.index";

        private const string MonthFormat = "yyyy-MM";

        private const string BuiltUtcFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

        private const string FullKind = "full";

        private const string DeltaKind = "delta";

        private readonly ArchiveIndexEntry[] entries;

        private readonly ReadOnlyCollection<ArchiveIndexEntry> readOnlyEntries;

        private readonly Dictionary<string, Dictionary<int, ArchiveDayLocation>> locations = new Dictionary<string, Dictionary<int, ArchiveDayLocation>>(StringComparer.Ordinal);

        public ArchiveIndex(ArchiveFileIdentity identity, IEnumerable<ArchiveIndexEntry> entries)
        {
            ArgumentNullException.ThrowIfNull(identity);
            ArgumentNullException.ThrowIfNull(entries);

            this.Identity = identity;
            this.entries = entries.OrderBy(entry => entry?.Symbol, StringComparer.Ordinal).ThenBy(entry => entry?.Day).ToArray();
            this.readOnlyEntries = Array.AsReadOnly(this.entries);

            int daysInMonth = DateTime.DaysInMonth(identity.Year, identity.Month);
            long row = 0;
            foreach(ArchiveIndexEntry entry in this.entries)
            {
                ValidateEntry(identity, daysInMonth, entry, nameof(entries));

                if(!this.locations.TryGetValue(entry.Symbol, out Dictionary<int, ArchiveDayLocation> symbolDays))
                {
                    symbolDays = new Dictionary<int, ArchiveDayLocation>();
                    this.locations.Add(entry.Symbol, symbolDays);
                }

                if(!symbolDays.TryAdd(entry.Day, new ArchiveDayLocation(row, entry.RecordCount)))
                {
                    throw new ArgumentException(Invariant($"{entry.Symbol} day {entry.Day} is listed twice."), nameof(entries));
                }

                row += entry.RecordCount;
            }

            this.RowCount = row;
        }

        /// <summary>
        /// Gets which archive file version the index belongs to.
        /// </summary>
        public ArchiveFileIdentity Identity { get; }

        /// <summary>
        /// Gets every symbol-day the file holds, in the order of its rows.
        /// </summary>
        public IReadOnlyList<ArchiveIndexEntry> Entries => this.readOnlyEntries;

        /// <summary>
        /// Gets the number of rows the file holds, the sum of all record counts.
        /// </summary>
        public long RowCount { get; }

        /// <summary>
        /// Reads an index from its JSON form
        /// </summary>
        /// <param name="json">JSON written by <see cref="ToJson"/></param>
        /// <returns>The index</returns>
        /// <exception cref="InvalidDataException">The JSON is not an index of a known <see cref="FormatVersion"/></exception>
        public static ArchiveIndex FromJson(string json)
        {
            try
            {
                using(JsonDocument document = JsonDocument.Parse(json ?? string.Empty))
                {
                    JsonElement root = document.RootElement;

                    int formatVersion = root.GetProperty("format_version").GetInt32();
                    if(formatVersion != FormatVersion)
                    {
                        throw new InvalidDataException(Invariant($"Archive index format version {formatVersion} is not known; this build reads version {FormatVersion}."));
                    }

                    DateTime month = DateTime.ParseExact(root.GetProperty("month").GetString(), MonthFormat, CultureInfo.InvariantCulture);
                    DateTime builtUtc = DateTime.ParseExact(root.GetProperty("built_utc").GetString(), BuiltUtcFormat, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
                    string kind = root.GetProperty("kind").GetString();
                    int generation = root.GetProperty("generation").GetInt32();
                    if(!string.Equals(kind, generation == 0 ? FullKind : DeltaKind, StringComparison.Ordinal))
                    {
                        throw new InvalidDataException(Invariant($"An archive file of kind '{kind}' cannot be generation {generation}: a full file is 0, a delta 1 and up."));
                    }

                    ArchiveFileIdentity identity = new ArchiveFileIdentity(root.GetProperty("dataset").GetString(), root.GetProperty("schema").GetString(), root.GetProperty("group").GetString(), month.Year, month.Month, root.GetProperty("cutoff").GetString(), builtUtc, generation);

                    List<ArchiveIndexEntry> entries = new List<ArchiveIndexEntry>();
                    List<long> storedFirstRows = new List<long>();
                    foreach(JsonProperty symbol in root.GetProperty("symbols").EnumerateObject())
                    {
                        foreach(JsonElement day in symbol.Value.EnumerateArray())
                        {
                            if(day.GetArrayLength() != 3)
                            {
                                throw new InvalidDataException(Invariant($"{symbol.Name} has a day entry that is not [day, first row, record count]."));
                            }

                            entries.Add(new ArchiveIndexEntry(symbol.Name, day[0].GetInt32(), day[2].GetInt32()));
                            storedFirstRows.Add(day[1].GetInt64());
                        }
                    }

                    ArchiveIndex index = new ArchiveIndex(identity, entries);

                    // the stored first rows are what a reader jumps to, so they must be the layout the writer used
                    for(int i = 0; i < entries.Count; ++i)
                    {
                        index.TryLocate(entries[i].Symbol, entries[i].Day, out ArchiveDayLocation location);
                        if(location.FirstRow != storedFirstRows[i])
                        {
                            throw new InvalidDataException(Invariant($"{entries[i].Symbol} day {entries[i].Day} is stored at row {storedFirstRows[i]}, but the layout puts it at row {location.FirstRow}."));
                        }
                    }

                    return index;
                }
            }
            catch(Exception ex) when(ex is JsonException || ex is KeyNotFoundException || ex is InvalidOperationException || ex is FormatException || ex is ArgumentException)
            {
                throw new InvalidDataException(Invariant($"Archive index is malformed: {ex.Message}"), ex);
            }
        }

        /// <summary>
        /// Finds where a symbol-day's records sit
        /// </summary>
        /// <param name="symbol">Databento raw symbol</param>
        /// <param name="day">Day of the month</param>
        /// <param name="location">Where the day's records sit; its record count is 0 for a day cached as empty</param>
        /// <returns><c>true</c> when the file holds the day, <c>false</c> when the day is not cached here</returns>
        public bool TryLocate(string symbol, int day, out ArchiveDayLocation location)
        {
            location = default;

            return symbol != null
                && this.locations.TryGetValue(symbol, out Dictionary<int, ArchiveDayLocation> symbolDays)
                && symbolDays.TryGetValue(day, out location);
        }

        /// <summary>
        /// Gets the days of the month the file holds for a symbol, in order
        /// </summary>
        /// <param name="symbol">Databento raw symbol</param>
        /// <returns>The days, empty when the file holds none for the symbol</returns>
        public IReadOnlyList<int> GetDays(string symbol)
        {
            if(symbol == null || !this.locations.TryGetValue(symbol, out Dictionary<int, ArchiveDayLocation> symbolDays))
            {
                return Array.Empty<int>();
            }

            return symbolDays.Keys.OrderBy(day => day).ToArray();
        }

        /// <summary>
        /// Writes the index as compact JSON: the identity with the file's kind and generation, then each symbol's days as <c>[day, first row, record count]</c>
        /// </summary>
        /// <returns>The JSON</returns>
        public string ToJson()
        {
            using(MemoryStream stream = new MemoryStream())
            {
                using(Utf8JsonWriter writer = new Utf8JsonWriter(stream))
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("format_version", FormatVersion);
                    writer.WriteString("dataset", this.Identity.Dataset);
                    writer.WriteString("schema", this.Identity.Schema);
                    writer.WriteString("group", this.Identity.Group);
                    writer.WriteString("month", new DateTime(this.Identity.Year, this.Identity.Month, 1).ToString(MonthFormat, CultureInfo.InvariantCulture));
                    writer.WriteString("cutoff", this.Identity.Cutoff);
                    writer.WriteString("built_utc", this.Identity.BuiltUtc.ToString(BuiltUtcFormat, CultureInfo.InvariantCulture));
                    writer.WriteString("kind", this.Identity.Kind == ArchiveFileKind.Full ? FullKind : DeltaKind);
                    writer.WriteNumber("generation", this.Identity.Generation);

                    writer.WriteStartObject("symbols");
                    string currentSymbol = null;
                    foreach(ArchiveIndexEntry entry in this.entries)
                    {
                        if(!string.Equals(entry.Symbol, currentSymbol, StringComparison.Ordinal))
                        {
                            if(currentSymbol != null)
                            {
                                writer.WriteEndArray();
                            }

                            writer.WriteStartArray(entry.Symbol);
                            currentSymbol = entry.Symbol;
                        }

                        this.TryLocate(entry.Symbol, entry.Day, out ArchiveDayLocation location);
                        writer.WriteStartArray();
                        writer.WriteNumberValue(entry.Day);
                        writer.WriteNumberValue(location.FirstRow);
                        writer.WriteNumberValue(entry.RecordCount);
                        writer.WriteEndArray();
                    }

                    if(currentSymbol != null)
                    {
                        writer.WriteEndArray();
                    }

                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static void ValidateEntry(ArchiveFileIdentity identity, int daysInMonth, ArchiveIndexEntry entry, string paramName)
        {
            if(entry == null || string.IsNullOrEmpty(entry.Symbol))
            {
                throw new ArgumentException("An index entry needs a symbol.", paramName);
            }

            if(entry.Day < 1 || entry.Day > daysInMonth)
            {
                throw new ArgumentException(Invariant($"{entry.Symbol} day {entry.Day} is not a day of {identity.Year}-{identity.Month:00}."), paramName);
            }

            if(entry.RecordCount < 0)
            {
                throw new ArgumentException(Invariant($"{entry.Symbol} day {entry.Day} has a negative record count."), paramName);
            }
        }
    }
}
