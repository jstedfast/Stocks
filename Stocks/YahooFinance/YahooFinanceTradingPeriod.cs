using Newtonsoft.Json;

namespace Stocks.YahooFinance
{
    public class YahooFinanceTradingPeriod
    {
        [JsonProperty("end")]
        public long UnixEndTime { get; set; }

        [JsonProperty("gmtoffset")]
        public long GmtOffsetSeconds { get; set; }

        [JsonProperty("start")]
        public long UnixStartTime { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonIgnore]
        public TimeSpan GmtOffset => TimeSpan.FromSeconds(GmtOffsetSeconds);

        [JsonIgnore]
        public DateTimeOffset Start => DateTimeOffset.FromUnixTimeSeconds(UnixStartTime).ToOffset(GmtOffset);

        [JsonIgnore]
        public DateTimeOffset End => DateTimeOffset.FromUnixTimeSeconds(UnixEndTime).ToOffset(GmtOffset);
    }
}
