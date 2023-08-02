using System.Diagnostics;

using Newtonsoft.Json;

namespace Stocks.YahooFinance
{
    [DebuggerDisplay("{Meta.Symbol,nq}")]
    public class YahooFinanceSpark
    {
        [JsonProperty("meta")]
        public YahooFinanceSparkMeta Meta { get; set; }

        [JsonProperty("timestamp")]
        public long[] Timestamp { get; set; }

        [JsonProperty("indicators")]
        public YahooFinanceChartIndicators Indicators { get; set; }
    }
}
