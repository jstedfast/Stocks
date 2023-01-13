using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceSparkResponse
    {
        [JsonProperty("spark")]
        public YahooFinanceSparkResponseData Data { get; set; }
    }
}
