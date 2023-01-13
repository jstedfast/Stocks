using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceChartIndicators
    {
        [JsonProperty("quote")]
        public YahooFinanceChartIndicatorQuote[] Quote { get; set; }
    }
}
