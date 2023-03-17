using Newtonsoft.Json;

namespace Stocks.YahooFinance
{
    public class YahooFinanceChartIndicators
    {
        [JsonProperty("quote")]
        public YahooFinanceChartIndicatorQuote[] Quote { get; set; }
    }
}
