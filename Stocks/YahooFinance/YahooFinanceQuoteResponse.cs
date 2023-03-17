using Newtonsoft.Json;

namespace Stocks.YahooFinance
{
    public class YahooFinanceQuoteResponse
    {
        [JsonProperty("quoteResponse")]
        public YahooFinanceQuoteResponseData Data { get; set; }
    }
}
