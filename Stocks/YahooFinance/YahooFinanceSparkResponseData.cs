using Newtonsoft.Json;

namespace Stocks.YahooFinance
{
    public class YahooFinanceSparkResponseData
    {
        [JsonProperty("result")]
        public YahooFinanceSparkResult[] Result { get; set; }

        [JsonProperty("error")]
        public YahooFinanceError Error { get; set; }
    }
}
