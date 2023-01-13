using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceSparkResult
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("response")]
        public YahooFinanceSpark[] Response { get; set; }
    }
}
