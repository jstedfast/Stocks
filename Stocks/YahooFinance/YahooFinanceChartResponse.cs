using Newtonsoft.Json;

namespace Stocks.YahooFinance
{
    public class YahooFinanceChartResponse
    {
        [JsonProperty("chart")]
        public YahooFinanceChartResponseData Data { get; set; }
    }
}
