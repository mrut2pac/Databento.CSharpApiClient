# Databento.CSharpApiClient

[![NuGet](https://img.shields.io/nuget/v/Databento.CSharpApiClient.svg)](https://www.nuget.org/packages/Databento.CSharpApiClient)
[![Databento](https://img.shields.io/badge/Databento-Historical%20API-474BD7)](https://databento.com)

Pure-managed C# HTTP client for the [Databento Historical API](https://databento.com/docs/api-reference-historical).

- Timeseries streaming: DBN binary and JSON — full schema coverage (MBO, MBP-1/10, CBBO, BBO, TBBO, TCBBO, CMBP-1, Trades, OHLCV, Status, Imbalance, Statistics, Definitions, Symbol Mapping)
- Metadata: datasets, schemas, fields, publishers, unit prices, conditions, date ranges, record count, cost
- Symbology: symbol resolution across SType encodings
- Batch jobs: submit, list, download files
- Retry / backoff: exponential + equal jitter, honours `Retry-After`, configurable via `DatabentoOptions`

See [CHANGELOG.md](CHANGELOG.md) for the full version history.

## Installation

```
dotnet add package Databento.CSharpApiClient
```

## Quick start

```csharp
using Databento.CSharpApiClient;
using Databento.CSharpApiClient.DataModel.Dbn;

var options = new DatabentoOptions { ApiKey = "YOUR_API_KEY" };
using var client = new DatabentoClient(options);

CbboRecordDbn[] quotes = await client.GetCbbo1sAsync(
    dataset: Datasets.OpraPillar,
    symbol: "SPXW 240119C04800000",
    startUtc: new DateTimeOffset(2024, 1, 18, 14, 30, 0, TimeSpan.Zero),
    endUtc:   new DateTimeOffset(2024, 1, 18, 21, 0,  0, TimeSpan.Zero));

foreach (var q in quotes)
    Console.WriteLine($"{q.TsEventUtc:O}  bid={q.BidPrice:F2}  ask={q.AskPrice:F2}");
```

## Streaming large requests

Every timeseries `Get*Async` method returns its records as one array. For a large request, stream the records instead with the matching `Stream*Async` method (or the synchronous `Stream*`). It yields each record as soon as it is decoded from the response, so only the records you keep stay in memory:

```csharp
await foreach (TcbboRecordDbn trade in client.StreamTcbboAsync(Datasets.OpraPillar, "SPXW  240119C04800000", start, end, cancellationToken))
{
    // aggregate, write to a file, ...
}
```

A stream retries only the HTTP status (429 and 5xx), before any of the body is read. A failure while reading the body is thrown rather than retried, so no record is seen twice. The response is released when the enumeration ends, including when you stop early or cancel. Each enumeration sends a new request, which is billed again, so enumerate a stream once, for example not `Any()` and then `foreach`.

`DbnReader` streams DBN data you already have, such as a downloaded batch file. The record types that carry a `Symbol` take it from the file's own metadata:

```csharp
using FileStream file = File.OpenRead("opra-pillar-20240118.tcbbo.dbn");
await foreach (TcbboRecordDbn trade in DbnReader.ReadTcbboAsync(file))
{
    // ...
}
```

Batch files are Zstandard-compressed (`.dbn.zst`) by default, and the client has no zstd decoder of its own: pass `DbnReader` a decompressing stream from the zstd library of your choice. For example, with the [ZstdSharp.Port](https://www.nuget.org/packages/ZstdSharp.Port) package:

```csharp
using ZstdSharp;

using FileStream file = File.OpenRead("opra-pillar-20240118.tcbbo.dbn.zst");
using DecompressionStream dbn = new DecompressionStream(file);
await foreach (TcbboRecordDbn trade in DbnReader.ReadTcbboAsync(dbn))
{
    // ...
}
```

Data that is still compressed fails with an `InvalidDataException` saying it does not start with `DBN`.

`JsonRecordReader` does the same for JSON data, such as a batch file submitted with `Encoding = "json"`. It reads the JSON with or without pretty prices and timestamps, and a record names its symbol when the job was submitted with `MapSymbols`:

```csharp
using FileStream file = File.OpenRead("xnas-itch-20220516.trades.json");
await foreach (TradeRecordJson trade in JsonRecordReader.ReadTradesAsync(file))
{
    // ...
}
```

## Batch jobs

`SubmitBatchJobAsync` takes a `BatchJobRequest`, which also says how the output is split into files: by period (`SplitDuration`, a day unless set), by symbol (`SplitSymbols`) and by size in bytes (`SplitSize`):

```csharp
BatchJob job = await client.SubmitBatchJobAsync(new BatchJobRequest
{
    Dataset = Datasets.XnasItch,
    Symbols = new[] { "SPY", "QQQ" },
    Schema = Schema.Trades,
    StartUtc = start,
    EndUtc = end,
    Encoding = "json",
    Compression = "none",   // zstd by default, which JsonRecordReader can only read through a zstd library
    MapSymbols = true,
    SplitDuration = "week",
    SplitSymbols = true,
});
```

Poll `GetBatchJobDetailsAsync(job.JobId)` until its `State` is `"done"`, then list its files with `ListBatchFilesAsync` and download each with `DownloadBatchFileAsync`.

## Configuration

```csharp
var options = new DatabentoOptions
{
    ApiKey        = "YOUR_API_KEY",
    Timeout       = TimeSpan.FromMinutes(10),  // default 5m
    MaxRetries    = 3,                          // default 3
    RetryBaseDelay = TimeSpan.FromSeconds(1),  // default 1s
    MaxRetryDelay  = TimeSpan.FromSeconds(30), // default 30s
};
```

## Clients

| Class | Encoding | Use when |
|---|---|---|
| `DatabentoClient` | DBN binary | Highest fidelity — nanosecond timestamps, nano-price integers, exact byte layout |
| `DatabentoJsonClient` | JSON | Simpler integration, all metadata + batch endpoints |

## Supported datasets

| Constant | Description |
|---|---|
| `Datasets.OpraPillar` | OPRA options (CBBO + definitions) |
| `Datasets.XnasItch` | Nasdaq equities (Trades, MBP-1, OHLCV) |
| `Datasets.GlbxMdp3` | CME Globex futures |
| `Datasets.XnyseTradesplus` | NYSE Trades+ |

## License

MIT — see [LICENSE](https://github.com/mrut2pac/Databento.CSharpApiClient/blob/main/LICENSE).
