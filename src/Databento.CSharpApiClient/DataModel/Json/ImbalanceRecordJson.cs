using System;

using System.Text.Json.Serialization;

using Databento.CSharpApiClient.JsonSupport;

namespace Databento.CSharpApiClient.DataModel.Json
{
    /// <summary>
    /// An auction imbalance record from the <c>imbalance</c> schema (JSON encoding).
    /// Published by exchanges during opening, closing, and intraday auctions to indicate
    /// the current auction state and directional imbalance.
    /// Corresponds to DBN rtype <c>Imbalance</c>.
    /// </summary>
    public sealed class ImbalanceRecordJson
    {
        /// <summary>Common record header (record type, publisher, instrument, event timestamp).</summary>
        [JsonPropertyName("hd")]
        public RecordHeaderJson Header { get; set; }

        /// <summary>Timestamp when the gateway received this message, in UTC.</summary>
        [JsonPropertyName("ts_recv")]
        public DateTime TsReceivedUtc { get; set; }

        /// <summary>Reference price for the auction (display-scaled).</summary>
        [JsonPropertyName("ref_price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double RefPrice { get; set; }

        /// <summary>Continuous book clearing price (display-scaled).</summary>
        [JsonPropertyName("cont_book_clr_price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double ContinuousBookClearingPrice { get; set; }

        /// <summary>Auction interest clearing price (display-scaled).</summary>
        [JsonPropertyName("auct_interest_clr_price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double AuctionInterestClearingPrice { get; set; }

        /// <summary>SSR filling price (display-scaled).</summary>
        [JsonPropertyName("ssr_filling_price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double SsrFillingPrice { get; set; }

        /// <summary>Indicative match price (display-scaled).</summary>
        [JsonPropertyName("ind_match_price")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double IndicativeMatchPrice { get; set; }

        /// <summary>Upper price collar for the auction (display-scaled).</summary>
        [JsonPropertyName("upper_collar")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double UpperCollar { get; set; }

        /// <summary>Lower price collar for the auction (display-scaled).</summary>
        [JsonPropertyName("lower_collar")]
        [JsonConverter(typeof(NanoPriceConverter))]
        public double LowerCollar { get; set; }

        /// <summary>Quantity matched (paired) at the indicative price, in lots.</summary>
        [JsonPropertyName("paired_qty")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint PairedQty { get; set; }

        /// <summary>Total imbalance quantity at the indicative price, in lots.</summary>
        [JsonPropertyName("total_imbalance_qty")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint TotalImbalanceQty { get; set; }

        /// <summary>Market imbalance quantity, in lots.</summary>
        [JsonPropertyName("market_imbalance_qty")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint MarketImbalanceQty { get; set; }

        /// <summary>Unpaired imbalance quantity, in lots.</summary>
        [JsonPropertyName("unpaired_qty")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint UnpairedQty { get; set; }

        /// <summary>Auction type code (e.g. <c>"O"</c> = opening, <c>"C"</c> = closing).</summary>
        [JsonPropertyName("auction_type")]
        public string AuctionType { get; set; }

        /// <summary>Side of the imbalance (e.g. <c>"B"</c> = buy side, <c>"S"</c> = sell side, <c>"N"</c> = no imbalance).</summary>
        [JsonPropertyName("side")]
        public string Side { get; set; }

        /// <summary>Auction status code from the exchange.</summary>
        [JsonPropertyName("auction_status")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte AuctionStatus { get; set; }

        /// <summary>Freeze status code from the exchange.</summary>
        [JsonPropertyName("freeze_status")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte FreezeStatus { get; set; }

        /// <summary>Number of auction extensions.</summary>
        [JsonPropertyName("num_extensions")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public byte NumExtensions { get; set; }

        /// <summary>Side of the unpaired imbalance.</summary>
        [JsonPropertyName("unpaired_side")]
        public string UnpairedSide { get; set; }

        /// <summary>Significant imbalance flag from the exchange.</summary>
        [JsonPropertyName("significant_imbalance")]
        public string SignificantImbalance { get; set; }

        /// <summary>Gateway send timestamp (UTC). Present when <c>ts_out</c> was requested.</summary>
        [JsonPropertyName("ts_out")]
        public DateTime? TsOutUtc { get; set; }
    }
}
