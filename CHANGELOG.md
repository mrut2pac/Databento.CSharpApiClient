# Changelog

All notable changes to this project will be documented in this file.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.2.1...HEAD
[1.2.1]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/mrut2pac/Databento.CSharpApiClient/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/mrut2pac/Databento.CSharpApiClient/releases/tag/v1.0.0
