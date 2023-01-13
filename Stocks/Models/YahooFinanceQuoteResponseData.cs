using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceQuoteResponseData
    {
        [JsonProperty("result")]
        public YahooFinanceQuote[] Result { get; set; }

        [JsonProperty("error")]
        public YahooFinanceError Error { get; set; }
    }
}
