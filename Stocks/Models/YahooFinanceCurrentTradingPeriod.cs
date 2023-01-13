using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceCurrentTradingPeriod
    {
        [JsonProperty("pre")]
        public YahooFinanceTradingPeriod Pre { get; set; }

        [JsonProperty("regular")]
        public YahooFinanceTradingPeriod Regular { get; set; }

        [JsonProperty("post")]
        public YahooFinanceTradingPeriod Post { get; set; }
    }
}
