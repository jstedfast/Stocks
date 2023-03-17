using Newtonsoft.Json;

namespace Stocks.YahooFinance
{
    public class YahooFinanceError
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
