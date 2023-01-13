using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceChartResponseData
    {
        [JsonProperty("result")]
        public YahooFinanceChart[] Result { get; set; }

        [JsonProperty("error")]
        public YahooFinanceError Error { get; set; }
    }
}
