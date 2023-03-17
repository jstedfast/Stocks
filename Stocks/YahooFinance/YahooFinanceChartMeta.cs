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
        public long FirstTradeDate { get; set; }

        [JsonProperty("regularMarketTime")]
        public long RegularMarketTime { get; set; }

        [JsonProperty("gmtoffset")]
        public long Gmtoffset { get; set; }

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

        static bool TryParseDataGranularity(string value, out TimeSpan timespan)
        {
            if (value.Equals("1m", StringComparison.Ordinal))
                timespan = TimeSpan.FromMinutes(1);
            else if (value.Equals("2m", StringComparison.Ordinal))
                timespan = TimeSpan.FromMinutes(2);
            else if (value.Equals("5m", StringComparison.Ordinal))
                timespan = TimeSpan.FromMinutes(5);
            else if (value.Equals("15m", StringComparison.Ordinal))
                timespan = TimeSpan.FromMinutes(15);
            else if (value.Equals("30m", StringComparison.Ordinal))
                timespan = TimeSpan.FromMinutes(30);
            else if (value.Equals("60m", StringComparison.Ordinal))
                timespan = TimeSpan.FromMinutes(60);
            else if (value.Equals("90m", StringComparison.Ordinal))
                timespan = TimeSpan.FromMinutes(90);
            else if (value.Equals("1h", StringComparison.Ordinal))
                timespan = TimeSpan.FromHours(1);
            else if (value.Equals("1d", StringComparison.Ordinal))
                timespan = TimeSpan.FromDays(1);
            else if (value.Equals("1d", StringComparison.Ordinal))
                timespan = TimeSpan.FromDays(1);
            else if (value.Equals("5d", StringComparison.Ordinal))
                timespan = TimeSpan.FromDays(5);
            else if (value.Equals("1wk", StringComparison.Ordinal))
                timespan = TimeSpan.FromDays(7);
            else if (value.Equals("1mo", StringComparison.Ordinal))
                timespan = TimeSpan.FromDays(365.25 / 12);
            else if (value.Equals("3mo", StringComparison.Ordinal))
                timespan = TimeSpan.FromDays(365.25 / 4);
            else
                timespan = TimeSpan.Zero;

            return timespan.Ticks > 0;
        }

        [JsonIgnore]
        public TimeSpan DataGranularity
        {
            get
            {
                if (RawDataGranularity != null && TryParseDataGranularity(RawDataGranularity, out var timespan))
                    return timespan;

                return TimeSpan.Zero;
            }
        }
    }
}
