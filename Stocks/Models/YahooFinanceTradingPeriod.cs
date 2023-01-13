using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceTradingPeriod
    {
        [JsonProperty("end")]
        public long End { get; set; }

        [JsonProperty("gmtoffset")]
        public long Gmtoffset { get; set; }

        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }
    }
}
