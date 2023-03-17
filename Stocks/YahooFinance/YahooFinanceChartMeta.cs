using Newtonsoft.Json;

namespace Stocks.YahooFinance
{
    public class YahooFinanceChartMeta
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("exchangeName")]
        public string ExchangeName { get; set; }

        [JsonProperty("instrumentType")]
        public string InstrumentType { get; set; }

        [JsonProperty("firstTradeDate")]
        public long UnixFirstTradeDate { get; set; }

        [JsonProperty("regularMarketTime")]
        public long UnixRegularMarketTime { get; set; }

        [JsonProperty("gmtoffset")]
        public long GmtOffsetSeconds { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("exchangeTimezoneName")]
        public string ExchangeTimezoneName { get; set; }

        [JsonProperty("regularMarketPrice")]
        public double RegularMarketPrice { get; set; }

        [JsonProperty("chartPreviousClose")]
        public double ChartPreviousClose { get; set; }

        [JsonProperty("previousClose")]
        public double PreviousClose { get; set; }

        [JsonProperty("scale")]
        public long Scale { get; set; }

        [JsonProperty("priceHint")]
        public long PriceHint { get; set; }

        [JsonProperty("currentTradingPeriod")]
        public YahooFinanceCurrentTradingPeriod CurrentTradingPeriod { get; set; }

        [JsonProperty("tradingPeriods")]
        public YahooFinanceTradingPeriods TradingPeriods { get; set; }

        [JsonProperty("dataGranularity")]
        public string RawDataGranularity { get; set; }

        [JsonProperty("range")]
        public string Range { get; set; }

        [JsonProperty("validRanges")]
        public string[] ValidRanges { get; set; }

        [JsonIgnore]
        public YahooFinanceTimeInterval DataGranularity => YahooFinanceClient.ParseDataGranularity(RawDataGranularity);

        [JsonIgnore]
        public TimeSpan GmtOffset => TimeSpan.FromSeconds(GmtOffsetSeconds);

        [JsonIgnore]
        public DateTimeOffset FirstTradeDate => DateTimeOffset.FromUnixTimeSeconds(UnixFirstTradeDate).ToOffset(GmtOffset);

        [JsonIgnore]
        public DateTimeOffset RegularMarketTime => DateTimeOffset.FromUnixTimeSeconds(UnixRegularMarketTime).ToOffset(GmtOffset);
    }
}
