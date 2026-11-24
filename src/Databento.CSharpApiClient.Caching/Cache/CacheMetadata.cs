using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Databento.CSharpApiClient.Caching.Cache
{
    /// <summary>
    /// The cache root's <c>metadata.json</c>: the format version of its files and the day cutoff they are cut by. It is written when
    /// the first file is saved into an empty root, and a root whose metadata this build does not accept is refused by everything that
    /// opens it.
    /// </summary>
    internal sealed class CacheMetadata
    {
        /// <summary>
        /// Name of the file in the cache root
        /// </summary>
        public const string FileName = "metadata.json";

        /// <summary>
        /// The format version this build writes and reads
        /// </summary>
        public const int CurrentFormatVersion = 1;

        /// <summary>
        /// The day cutoff of a cache whose days are UTC calendar days, as Databento cuts its data; the only one for now
        /// </summary>
        public const string UtcCutoff = "UTC";

        private CacheMetadata(int formatVersion, string dayCutoff)
        {
            this.FormatVersion = formatVersion;
            this.DayCutoff = dayCutoff;
        }

        /// <summary>
        /// Gets the format version of the cache's files.
        /// </summary>
        public int FormatVersion { get; }

        /// <summary>
        /// Gets the tag of the day cutoff the cache's days are cut by, which every file name carries, e.g. <c>UTC</c>.
        /// </summary>
        public string DayCutoff { get; }

        /// <summary>
        /// Reads a cache root's metadata
        /// </summary>
        /// <param name="cacheRoot">Cache root</param>
        /// <returns>The metadata, or <c>null</c> when the root has none</returns>
        /// <exception cref="InvalidDataException">The file is not metadata this build accepts: malformed, or of another format version or cutoff</exception>
        public static CacheMetadata Read(string cacheRoot)
        {
            ArgumentException.ThrowIfNullOrEmpty(cacheRoot);

            string path = Path.Combine(cacheRoot, FileName);
            string json;
            try
            {
                json = File.ReadAllText(path, Encoding.UTF8);
            }
            catch(Exception ex) when(ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                return null;
            }

            return Parse(json, path);
        }

        /// <summary>
        /// Reads a cache root's metadata, writing it first when the root has none, so the first file saved into an empty root finds it
        /// there. Of two writers at once, the first rename wins and the other reads its file.
        /// </summary>
        /// <param name="cacheRoot">Cache root, created when missing</param>
        /// <returns>The metadata</returns>
        /// <exception cref="InvalidDataException">The root's file is not metadata this build accepts</exception>
        public static CacheMetadata EnsureCreated(string cacheRoot)
        {
            CacheMetadata existing = Read(cacheRoot);
            if(existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(cacheRoot);
            string path = Path.Combine(cacheRoot, FileName);
            string temporaryPath = Invariant($"{path}.{Guid.NewGuid():N}.tmp");
            try
            {
                File.WriteAllText(temporaryPath, ToJson(CurrentFormatVersion, UtcCutoff), new UTF8Encoding(false));
                File.Move(temporaryPath, path, overwrite: false);
            }
            catch(IOException) when(File.Exists(path))
            {
                // another writer's file got there first
            }
            finally
            {
                File.Delete(temporaryPath);
            }

            return Read(cacheRoot) ?? throw new InvalidDataException(Invariant($"{path} was written but cannot be found."));
        }

        private static CacheMetadata Parse(string json, string path)
        {
            int formatVersion;
            string dayCutoff;
            try
            {
                using(JsonDocument document = JsonDocument.Parse(json))
                {
                    formatVersion = document.RootElement.GetProperty("format_version").GetInt32();
                    dayCutoff = document.RootElement.GetProperty("day_cutoff").GetString();
                }
            }
            catch(Exception ex) when(ex is JsonException || ex is KeyNotFoundException || ex is InvalidOperationException || ex is FormatException)
            {
                throw new InvalidDataException(Invariant($"{path} is not the metadata of a Databento cache: {ex.Message}"), ex);
            }

            if(formatVersion != CurrentFormatVersion)
            {
                throw new InvalidDataException(Invariant($"{path} says format version {formatVersion}; this build reads version {CurrentFormatVersion} only."));
            }

            if(!string.Equals(dayCutoff, UtcCutoff, StringComparison.Ordinal))
            {
                throw new InvalidDataException(Invariant($"{path} says day cutoff '{dayCutoff}'; this build reads '{UtcCutoff}' only."));
            }

            return new CacheMetadata(formatVersion, dayCutoff);
        }

        private static string ToJson(int formatVersion, string dayCutoff)
        {
            using(MemoryStream stream = new MemoryStream())
            {
                using(Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                {
                    writer.WriteStartObject();
                    writer.WriteNumber("format_version", formatVersion);
                    writer.WriteString("day_cutoff", dayCutoff);
                    writer.WriteEndObject();
                }

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
