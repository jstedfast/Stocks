using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stocks
{
    public class YahooStockQuote
    {
        static readonly Dictionary<string, string> SymbolDescriptionOverrides;
        static readonly Dictionary<string, string> SymbolNameOverrides;

        static YahooStockQuote ()
        {
            SymbolNameOverrides = new Dictionary<string, string>
            {
                { "^DJI", "Dow Jones" },
                { "^IXIC", "NASDAQ" },
                { "^GSPC", "S&P 500" }
            };

            SymbolDescriptionOverrides = new Dictionary<string, string>
            {
                { "^GSPC", "Standard & Poor's 500" }
            };
        }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("quoteType")]
        public string QuoteType { get; set; }

        [JsonProperty("typeDisp")]
        public string TypeDisp { get; set; }

        [JsonProperty("quoteSourceName")]
        public string QuoteSourceName { get; set; }

        [JsonProperty("triggerable")]
        public bool Triggerable { get; set; }

        [JsonProperty("customPriceAlertConfidence")]
        public string CustomPriceAlertConfidence { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("shortName")]
        public string ShortName { get; set; }

        [JsonProperty("messageBoardId")]
        public string MessageBoardId { get; set; }

        [JsonProperty("exchangeTimezoneShortName")]
        public string ExchangeTimezoneShortName { get; set; }

        [JsonProperty("gmtOffSetMilliseconds")]
        public int GmtOffSetMilliseconds { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("regularMarketChangePercent")]
        public float RegularMarketChangePercent { get; set; }

        [JsonProperty("regularMarketPrice")]
        public float RegularMarketPrice { get; set; }

        [JsonProperty("marketState")]
        public string MarketState { get; set; }

        [JsonProperty("esgPopulated")]
        public bool EsgPopulated { get; set; }

        [JsonProperty("exchangeTimezoneName")]
        public string ExchangeTimezoneName { get; set; }

        [JsonProperty("regularMarketChange")]
        public float RegularMarketChange { get; set; }

        [JsonProperty("regularMarketTime")]
        public int RegularMarketTime { get; set; }

        [JsonProperty("regularMarketDayHigh")]
        public float RegularMarketDayHigh { get; set; }

        [JsonProperty("regularMarketDayRange")]
        public string RegularMarketDayRange { get; set; }

        [JsonProperty("regularMarketDayLow")]
        public float RegularMarketDayLow { get; set; }

        [JsonProperty("regularMarketVolume")]
        public long RegularMarketVolume { get; set; }

        [JsonProperty("regularMarketPreviousClose")]
        public float RegularMarketPreviousClose { get; set; }

        [JsonProperty("bid")]
        public float Bid { get; set; }

        [JsonProperty("ask")]
        public float Ask { get; set; }

        [JsonProperty("bidSize")]
        public int BidSize { get; set; }

        [JsonProperty("askSize")]
        public int AskSize { get; set; }

        [JsonProperty("fullExchangeName")]
        public string FullExchangeName { get; set; }

        [JsonProperty("regularMarketOpen")]
        public float RegularMarketOpen { get; set; }

        [JsonProperty("averageDailyVolume3Month")]
        public long AverageDailyVolume3Month { get; set; }

        [JsonProperty("averageDailyVolume10Day")]
        public long AverageDailyVolume10Day { get; set; }

        [JsonProperty("fiftyTwoWeekLowChange")]
        public float FiftyTwoWeekLowChange { get; set; }

        [JsonProperty("fiftyTwoWeekLowChangePercent")]
        public float FiftyTwoWeekLowChangePercent { get; set; }

        [JsonProperty("fiftyTwoWeekRange")]
        public string FiftyTwoWeekRange { get; set; }

        [JsonProperty("fiftyTwoWeekHighChange")]
        public float FiftyTwoWeekHighChange { get; set; }

        [JsonProperty("fiftyTwoWeekHighChangePercent")]
        public float FiftyTwoWeekHighChangePercent { get; set; }

        [JsonProperty("fiftyTwoWeekLow")]
        public float FiftyTwoWeekLow { get; set; }

        [JsonProperty("fiftyTwoWeekHigh")]
        public float FiftyTwoWeekHigh { get; set; }

        [JsonProperty("fiftyDayAverage")]
        public float FiftyDayAverage { get; set; }

        [JsonProperty("fiftyDayAverageChange")]
        public float FiftyDayAverageChange { get; set; }

        [JsonProperty("fiftyDayAverageChangePercent")]
        public float FiftyDayAverageChangePercent { get; set; }

        [JsonProperty("twoHundredDayAverage")]
        public float TwoHundredDayAverage { get; set; }

        [JsonProperty("twoHundredDayAverageChange")]
        public float TwoHundredDayAverageChange { get; set; }

        [JsonProperty("twoHundredDayAverageChangePercent")]
        public float TwoHundredDayAverageChangePercent { get; set; }

        [JsonProperty("sourceInterval")]
        public int SourceInterval { get; set; }

        [JsonProperty("exchangeDataDelayedBy")]
        public int ExchangeDataDelayedBy { get; set; }

        [JsonProperty("tradeable")]
        public bool Tradeable { get; set; }

        [JsonProperty("cryptoTradeable")]
        public bool CryptoTradeable { get; set; }

        [JsonProperty("firstTradeDateMilliseconds")]
        public long FirstTradeDateMilliseconds { get; set; }

        [JsonProperty("priceHint")]
        public int PriceHint { get; set; }

        [JsonProperty("symbol", Required = Required.Always)]
        public string Symbol { get; set; }

        [JsonIgnore]
        public string Name
        {
            get
            {
                if (SymbolNameOverrides.TryGetValue(Symbol, out var name))
                    return name;

                return Symbol;
            }
        }

        [JsonIgnore]
        public string Description
        {
            get
            {
                if (SymbolDescriptionOverrides.TryGetValue(Symbol, out var description))
                    return description;

                return ShortName ?? string.Empty;
            }
        }
    }
}
