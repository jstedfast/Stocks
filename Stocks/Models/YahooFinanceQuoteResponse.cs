using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceQuoteResponse
    {
        [JsonProperty("quoteResponse")]
        public YahooFinanceQuoteResponseData Data { get; set; }
    }
}
