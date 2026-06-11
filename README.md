# Databento.CSharpApiClient

Pure-managed C# HTTP client for the [Databento Historical API](https://databento.com/docs/api-reference-historical).

- Timeseries streaming: DBN binary (CBBO, Trades, MBP-1) and JSON (CBBO, OHLCV, Trades, MBP-1, Definitions, Statistics)
- Metadata: datasets, schemas, fields, publishers, unit prices, conditions, date ranges
- Symbology: symbol resolution across SType encodings
- Batch jobs: submit, list, download files
- Retry / backoff: exponential + equal jitter, honours `Retry-After`, configurable via `DatabentoOptions`

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

MIT — see [LICENSE](LICENSE).
