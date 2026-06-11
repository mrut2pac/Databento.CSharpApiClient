using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Databento.CSharpApiClient.DataModel.Dbn
{
    /// <summary>
    /// Parsed DBN stream metadata header. Every DBN stream begins with the magic bytes
    /// <c>DBN</c> followed by a version byte and this fixed-length metadata block.
    /// </summary>
    public sealed class DbnMetadata
    {
        /// <summary>DBN format version (1 or 2).</summary>
        public byte Version { get; set; }

        /// <summary>Dataset identifier embedded in the stream (e.g. <c>"XNAS.ITCH"</c>).</summary>
        public string Dataset { get; set; }

        /// <summary>Schema of the records that follow.</summary>
        public DbnSchema Schema { get; set; }

        /// <summary>Inclusive start of the time range covered by this stream.</summary>
        public DateTimeOffset Start { get; set; }

        /// <summary>Exclusive end of the time range, or <see langword="null"/> if open-ended.</summary>
        public DateTimeOffset? End { get; set; }

        /// <summary>Maximum number of records in the stream, or <see langword="null"/> if unlimited.</summary>
        public ulong? Limit { get; set; }

        /// <summary>Input symbol type used when requesting this stream.</summary>
        public DbnSType STypeIn { get; set; }

        /// <summary>Output symbol type used in the symbol mappings.</summary>
        public DbnSType STypeOut { get; set; }

        /// <summary>Whether a <c>ts_out</c> field is appended to each record.</summary>
        public bool TsOut { get; set; }

        /// <summary>Fixed width of symbol C-strings in the mappings section (22 for v1, read from stream for v2).</summary>
        public ushort SymbolCStrLen { get; set; }

        /// <summary>Raw schema definition bytes (may be empty).</summary>
        public byte[] SchemaDefinition { get; set; }

        /// <summary>Input symbols that were requested.</summary>
        public List<string> Symbols { get; set; } = new List<string>();

        /// <summary>Input symbols that were only partially resolved.</summary>
        public List<string> Partial { get; set; } = new List<string>();

        /// <summary>Input symbols for which no data was found.</summary>
        public List<string> NotFound { get; set; } = new List<string>();

        /// <summary>Symbol mappings from stype_in to stype_out for the stream's time range.</summary>
        public List<DbnSymbolMapping> Mappings { get; set; } = new List<DbnSymbolMapping>();

        public static DbnMetadata FromBinaryReader(BinaryReader binaryReader)
        {
            // Prelude: 'D','B','N' + version(u8) + metadata_length(u32 LE)
            byte d = binaryReader.ReadByte(), b = binaryReader.ReadByte(), n = binaryReader.ReadByte();
            if(d != (byte)'D' || b != (byte)'B' || n != (byte)'N')
            {
                throw new InvalidDataException("Invalid DBN stream: missing 'DBN' magic.");
            }

            byte version = binaryReader.ReadByte();
            uint metaLen = binaryReader.ReadUInt32();
            if(metaLen == 0)
            {
                throw new InvalidDataException("Invalid DBN metadata: zero length.");
            }

            if(metaLen > int.MaxValue)
            {
                throw new InvalidDataException("Metadata length overflow.");
            }

            byte[] meta = binaryReader.ReadBytes((int)metaLen);
            if(meta.Length != metaLen) throw new EndOfStreamException("Truncated DBN metadata.");

            DbnMetadata md = new DbnMetadata();
            md.Version = version;

            using(MemoryStream ms = new MemoryStream(meta, false))
            using(BinaryReader r = new BinaryReader(ms, Encoding.UTF8, false))
            {
                // ---- Fixed header fields (top section) ----
                // dataset: char[16]
                md.Dataset = ReadFixedCString(r, 16);

                // schema: u16 (0xFFFF => null)
                md.Schema = (DbnSchema)r.ReadUInt16();

                // start: u64 ns
                md.Start = Utils.FromUnixNs(r.ReadUInt64());

                // end: u64 ns (0 => None)
                ulong end = r.ReadUInt64();
                md.End = (end == 0) ? (DateTimeOffset?)null : Utils.FromUnixNs(end);

                // limit: u64 (0 => None)
                ulong limitRaw = r.ReadUInt64();
                md.Limit = (limitRaw == 0) ? (ulong?)null : limitRaw;

                // stype_in: u8 (0xFF => Null)
                md.STypeIn = (DbnSType)r.ReadByte();

                // stype_out: u8
                md.STypeOut = (DbnSType)r.ReadByte();

                // ts_out: u8
                md.TsOut = r.ReadByte() != 0;

                // ---- Bottom section (version-dependent) ----
                // symbol_cstr_len: u16 (only in v2). In v1 it's always 22.
                ushort symbolCStrLen;
                if(version == 1)
                {
                    symbolCStrLen = 22;
                }
                else
                {
                    symbolCStrLen = r.ReadUInt16();
                }
                md.SymbolCStrLen = symbolCStrLen;

                // skip reserved padding bytes
                if(version == 1)
                {
                    SkipExact(r, 47);
                }
                else
                {
                    SkipExact(r, 53);
                }

                // schema_definition_length + bytes
                uint schemaDefLen = r.ReadUInt32();
                if(schemaDefLen > int.MaxValue) throw new InvalidDataException("schema_definition_length overflow.");
                md.SchemaDefinition = (schemaDefLen == 0) ? Array.Empty<byte>() : r.ReadBytes((int)schemaDefLen);
                if(md.SchemaDefinition.Length != schemaDefLen) throw new EndOfStreamException("Truncated schema_definition.");

                // symbols_length + [char[symbol_cstr_len]] * N
                uint symbolsLen = r.ReadUInt32();
                md.Symbols = ReadFixedCStringVector(r, symbolsLen, symbolCStrLen);

                // partial_length + [char[symbol_cstr_len]] * N
                uint partialLen = r.ReadUInt32();
                md.Partial = ReadFixedCStringVector(r, partialLen, symbolCStrLen);

                // not_found_length + [char[symbol_cstr_len]] * N
                uint notFoundLen = r.ReadUInt32();
                md.NotFound = ReadFixedCStringVector(r, notFoundLen, symbolCStrLen);

                // mappings_length + SymbolMapping[mappings_length]
                uint mappingsLen = r.ReadUInt32();
                if(mappingsLen > 1_000_000U) throw new InvalidDataException("mappings_length too large.");
                for(uint i = 0; i < mappingsLen; i++)
                {
                    md.Mappings.Add(ReadSymbolMapping(r, symbolCStrLen));
                }

                // Safety: we should be exactly at the end of metadata buffer
                if(ms.Position != ms.Length)
                {
                    // If there are bytes left, consume them to avoid misalignment, but flag it:
                    long remain = ms.Length - ms.Position;
                    r.ReadBytes((int)remain);
                    // Optionally log or throw depending on how strict you want to be.
                }
            }

            return md;
        }

        private static void SkipExact(BinaryReader r, int bytes)
        {
            if(bytes <= 0)
            {
                return;
            }

            int read = r.ReadBytes(bytes).Length;
            if(read != bytes)
            {
                throw new EndOfStreamException("Truncated while skipping reserved bytes.");
            }
        }

        // Fixed ASCII C-string with padding: read exactly width bytes, trim at first NUL, then trim trailing spaces.
        private static string ReadFixedCString(BinaryReader r, int width)
        {
            byte[] bytes = r.ReadBytes(width);
            if(bytes.Length != width) throw new EndOfStreamException("Truncated fixed C-string.");
            int end = Array.IndexOf<byte>(bytes, 0);
            if(end < 0) end = width; // no NUL found, take full width
                                     // Trim trailing spaces from the portion before NUL
            while(end > 0 && bytes[end - 1] == (byte)' ') end--;
            return end == 0 ? string.Empty : Encoding.ASCII.GetString(bytes, 0, end);
        }

        // Read N fixed-width C-strings of length symbolCStrLen.
        private static List<string> ReadFixedCStringVector(BinaryReader r, uint count, int symbolCStrLen)
        {
            if(count > 1_000_000U) throw new InvalidDataException("Vector too large.");
            List<string> list = new List<string>((int)Math.Min(count, (uint)int.MaxValue));
            for(uint i = 0; i < count; i++)
                list.Add(ReadFixedCString(r, symbolCStrLen));
            return list;
        }

        private static DbnMappingInterval ReadMappingInterval(BinaryReader r, int symbolCStrLen)
        {
            DbnMappingInterval mi = new DbnMappingInterval();
            mi.StartDate = r.ReadUInt32();
            mi.EndDate = r.ReadUInt32();
            mi.Symbol = ReadFixedCString(r, symbolCStrLen); // stype_out symbol, fixed width
            return mi;
        }

        private static DbnSymbolMapping ReadSymbolMapping(BinaryReader r, int symbolCStrLen)
        {
            DbnSymbolMapping map = new DbnSymbolMapping();
            map.RawSymbol = ReadFixedCString(r, symbolCStrLen); // stype_in raw symbol
            uint intervalLen = r.ReadUInt32();
            if(intervalLen > 1_000_000U) throw new InvalidDataException("interval_length too large.");
            for(uint k = 0; k < intervalLen; k++)
                map.Intervals.Add(ReadMappingInterval(r, symbolCStrLen));
            return map;
        }
    }
}
