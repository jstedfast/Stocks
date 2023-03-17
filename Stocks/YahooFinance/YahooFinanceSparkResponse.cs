using Newtonsoft.Json;

namespace Stocks.YahooFinance
{
    public class YahooFinanceSparkResponse
    {
        [JsonProperty("spark")]
        public YahooFinanceSparkResponseData Data { get; set; }
    }
}
