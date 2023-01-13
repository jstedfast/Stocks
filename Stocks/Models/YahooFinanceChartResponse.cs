using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceChartResponse
    {
        [JsonProperty("chart")]
        public YahooFinanceChartResponseData Data { get; set; }
    }
}
