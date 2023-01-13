using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceChart
    {
        [JsonProperty("meta")]
        public YahooFinanceChartMeta Meta { get; set; }

        [JsonProperty("timestamp")]
        public long[] Timestamp { get; set; }

        [JsonProperty("indicators")]
        public YahooFinanceChartIndicators Indicators { get; set; }
    }
}
