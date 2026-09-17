# Changelog

All notable changes to this project will be documented in this file.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.4.0] - 2026-09-17

### Added
- `DatabentoOptions.RequestCompressedResponses` — defaults to `true`, set it to `false` to work around an intermediary that mishandles a content encoding.

### Changed
- Requests now negotiate a compressed response. Both `DatabentoClient` and `DatabentoJsonClient` build their `HttpClient` on a handler with `AutomaticDecompression` enabled, so the client sends `Accept-Encoding` and the runtime transparently decodes whatever `Content-Encoding` the API answers with. Previously neither client sent the header and every response arrived uncompressed.
- Nothing in the response handling. Decompression happens in the transport, below the deserializer, so a response stream reads exactly as it did before and prices, timestamps, symbols and DBN framing are untouched.
- `DownloadBatchFileAsync` is deliberately exempt and sends no `Accept-Encoding`. A batch artifact is already compressed, so negotiating it back gains nothing — and were such a file ever served with a `Content-Encoding` header, the runtime would decode it in flight and hand back bytes matching neither the file name it is saved under nor its published hash. The exemption applies only to the built-in transport; a caller supplying its own `IHttpTransport` owns that decision for every request.

**Impact:** transparent and backward compatible. Market data is highly repetitive, so a timeseries response is substantially smaller on the wire — one session of option CBBO measured 117 KB uncompressed against 8.1 KB gzipped, and the ratio will vary by schema and symbol count. The saving is bandwidth and transfer time only: the decompressed bytes the caller deserializes are identical, so nothing downstream changes and peak memory is unchanged.

This is deliberately transport-level rather than the API's own `compression` query parameter, which frames the payload itself and would require a decoder this package does not carry — zero runtime dependencies is the point of it.

## [1.3.0] - 2026-09-17

### Added
- `Symbol` on every JSON record type returned by `timeseries.get_range` — `BboRecordJson`, `CbboRecordJson`, `Cmbp1RecordJson`, `DefinitionRecordJson`, `ImbalanceRecordJson`, `MboRecordJson`, `Mbp1RecordJson`, `Mbp10RecordJson`, `OhlcvRecordJson`, `StatisticsRecordJson`, `StatusRecordJson`, `TbboRecordJson`, `TcbboRecordJson`, `TradeRecordJson`. Populated from the API's `map_symbols` field; `null` when the request did not ask for it.

### Changed
- A timeseries request covering **more than one symbol** — several symbols, or `ALL_SYMBOLS` — now sends `map_symbols=true`. The response interleaves the requested symbols, so each record has to name its own; previously a caller had to resolve `instrument_id` through a separate `symbology.resolve` call to attribute them.
- A **single-symbol** request is unchanged and deliberately does not send it. The caller already knows the symbol, and `map_symbols` repeats it on every record rather than sending it once per response — roughly 10% payload growth for no information.

**Impact:** additive. Existing single-symbol callers see byte-identical requests and responses. Multi-symbol callers get a slightly larger response carrying the new field; deserialization of a response without it is unaffected, and `Symbol` is simply `null`. The DBN binary path is untouched — DBN carries symbol mappings in its metadata already.

## [1.2.3] - 2026-06-24

### Fixed
- README LICENSE link now uses an absolute GitHub URL so it resolves on the NuGet package page (relative links parked on the package page).

## [1.2.1] - 2026-06-13

### Changed
- README updated to reflect full v1.2.0 schema coverage
- NuGet `PackageReleaseNotes` expanded to the full v1.2.0 Added/Fixed entries

## [1.2.0] - 2026-06-13

### Added
- **`DatabentoJsonClient` — complete Historical API JSON coverage**
  - New timeseries schemas: `mbo`, `mbp-10`, `bbo-1s`, `bbo-1m`, `tbbo`, `tcbbo`, `cmbp-1`, `status`, `imbalance`, `symbol_mapping`
  - New metadata endpoints: `metadata.list_conditions`, `metadata.get_record_count`, `metadata.get_billable_size`, `metadata.get_cost`
  - All new methods have sync + async variants and single-symbol / multi-symbol overloads
- **`DatabentoClient` (DBN binary) — complete schema coverage**
  - New record types: `MboRecordDbn`, `Mbp10RecordDbn`, `TbboRecordDbn`, `TcbboRecordDbn`, `Cmbp1RecordDbn`, `BboRecordDbn`, `StatusRecordDbn`, `ImbalanceRecordDbn`, `StatisticsRecordDbn`, `DefinitionRecordDbn`, `SymbolMappingRecordDbn`
  - Adaptive MBO body layout: handles both DBN v1 (40-byte body, no `channel_id`) and v2 (48-byte body) records transparently
- Unit and integration tests for every new schema and endpoint

### Fixed
- TBBO DBN decoder now correctly accepts rtype `0x01` (`Mbp1`) as sent by the live Databento API; TBBO and MBP-1 share the same binary layout

## [1.1.0] - 2026-06-12

### Added
- `DatabentoErrorCase` typed enum on `DatabentoHttpException` — callers can now branch on `ex.ErrorCase` instead of comparing raw error-case strings
- `DatabentoHttpException.Create(statusCode, body)` factory that parses the Databento JSON error envelope and populates both `StatusCode` and `ErrorCase`

## [1.0.1] - 2026-06-12

### Fixed
- `NanoPriceConverter`: corrected a Write/Read round-trip asymmetry where prices written by the converter could not be correctly recovered on the read path

## [1.0.0] - 2026-06-12

Initial release.

### Added
- `DatabentoJsonClient` — Historical API, JSON encoding
  - Timeseries: `cbbo-1s`, `cbbo-1m`, `ohlcv-1s/1m/1h/1d/eod`, `trades`, `mbp-1`, `statistics`, `definition`
  - Metadata: `list_datasets`, `list_schemas`, `list_publishers`, `list_fields`, `list_unit_prices`, `get_dataset_condition`, `get_dataset_range`
  - Batch: `submit_job`, `list_jobs`, `get_job_details`, `list_files`, `download`
  - Symbology: `resolve`
- `DatabentoClient` — Historical API, DBN binary encoding
  - Schemas: `cbbo-1s`, `cbbo-1m`, `trades`, `mbp-1`
  - Full DBN metadata + framing decoder
- HTTP retry with exponential back-off and jitter
- Zero external dependencies — pure `System.Text.Json` on .NET 8+

[Unreleased]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.4.0...HEAD
[1.4.0]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.2.3...v1.3.0
[1.2.3]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.2.1...v1.2.3
[1.2.1]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/mrut2pac/Databento.CSharpApiClient/releases/tag/v1.0.0
