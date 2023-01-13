using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceError
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
