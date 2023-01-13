using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceChartIndicatorQuote
    {
        [JsonProperty("close")]
        public double?[] Close { get; set; }

        [JsonProperty("open")]
        public double?[] Open { get; set; }

        [JsonProperty("volume")]
        public long?[] Volume { get; set; }

        [JsonProperty("low")]
        public double?[] Low { get; set; }

        [JsonProperty("high")]
        public double?[] High { get; set; }
    }
}
