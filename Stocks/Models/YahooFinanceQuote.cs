using Newtonsoft.Json;

namespace Stocks
{
    public class YahooFinanceQuote
    {
        const int MillisecondsPerSecond = 1000;
        const int MillisecondsPerMinute = 60 * MillisecondsPerSecond;
        const int MillisecondsPerHour = 60 * MillisecondsPerMinute;

        static readonly Dictionary<string, string> SymbolDescriptionOverrides;
        static readonly Dictionary<string, string> SymbolNameOverrides;

        static YahooFinanceQuote ()
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

        [JsonProperty("ask")]
        public double Ask { get; set; }

        [JsonProperty("askSize")]
        public long AskSize { get; set; }

        [JsonProperty("averageAnalystRating", NullValueHandling = NullValueHandling.Ignore)]
        public string AverageAnalystRating { get; set; }

        [JsonProperty("averageDailyVolume10Day")]
        public long AverageDailyVolume10Day { get; set; }

        [JsonProperty("averageDailyVolume3Month")]
        public long AverageDailyVolume3Month { get; set; }

        [JsonProperty("bid")]
        public double Bid { get; set; }

        [JsonProperty("bidSize")]
        public long BidSize { get; set; }

        [JsonProperty("bookValue", NullValueHandling = NullValueHandling.Ignore)]
        public double? BookValue { get; set; }

        [JsonProperty("cryptoTradeable")]
        public bool CryptoTradeable { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("customPriceAlertConfidence")]
        public string CustomPriceAlertConfidence { get; set; }

        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("dividendDate", NullValueHandling = NullValueHandling.Ignore)]
        public long? DividendDate { get; set; }

        [JsonProperty("earningsTimestamp", NullValueHandling = NullValueHandling.Ignore)]
        public long? EarningsTimestamp { get; set; }

        [JsonProperty("earningsTimestampEnd", NullValueHandling = NullValueHandling.Ignore)]
        public long? EarningsTimestampEnd { get; set; }

        [JsonProperty("earningsTimestampStart", NullValueHandling = NullValueHandling.Ignore)]
        public long? EarningsTimestampStart { get; set; }

        [JsonProperty("epsCurrentYear", NullValueHandling = NullValueHandling.Ignore)]
        public double? EpsCurrentYear { get; set; }

        [JsonProperty("epsForward", NullValueHandling = NullValueHandling.Ignore)]
        public double? EpsForward { get; set; }

        [JsonProperty("epsTrailingTwelveMonths", NullValueHandling = NullValueHandling.Ignore)]
        public double? EpsTrailingTwelveMonths { get; set; }

        [JsonProperty("esgPopulated")]
        public bool EsgPopulated { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("exchangeDataDelayedBy")]
        public long ExchangeDataDelayedBy { get; set; }

        [JsonProperty("exchangeTimezoneName")]
        public string ExchangeTimezoneName { get; set; }

        [JsonProperty("exchangeTimezoneShortName")]
        public string ExchangeTimezoneShortName { get; set; }

        [JsonProperty("fiftyDayAverage")]
        public double FiftyDayAverage { get; set; }

        [JsonProperty("fiftyDayAverageChange")]
        public double FiftyDayAverageChange { get; set; }

        [JsonProperty("fiftyDayAverageChangePercent")]
        public double FiftyDayAverageChangePercent { get; set; }

        [JsonProperty("fiftyTwoWeekHigh")]
        public double FiftyTwoWeekHigh { get; set; }

        [JsonProperty("fiftyTwoWeekHighChange")]
        public double FiftyTwoWeekHighChange { get; set; }

        [JsonProperty("fiftyTwoWeekHighChangePercent")]
        public double FiftyTwoWeekHighChangePercent { get; set; }

        [JsonProperty("fiftyTwoWeekLow")]
        public double FiftyTwoWeekLow { get; set; }

        [JsonProperty("fiftyTwoWeekLowChange")]
        public double FiftyTwoWeekLowChange { get; set; }

        [JsonProperty("fiftyTwoWeekLowChangePercent")]
        public double FiftyTwoWeekLowChangePercent { get; set; }

        [JsonProperty("fiftyTwoWeekRange")]
        public string FiftyTwoWeekRange { get; set; }

        [JsonProperty("financialCurrency", NullValueHandling = NullValueHandling.Ignore)]
        public string FinancialCurrency { get; set; }

        [JsonProperty("firstTradeDateMilliseconds")]
        public long FirstTradeDateMilliseconds { get; set; }

        [JsonProperty("forwardPE", NullValueHandling = NullValueHandling.Ignore)]
        public double? ForwardPE { get; set; }

        [JsonProperty("fullExchangeName")]
        public string FullExchangeName { get; set; }

        [JsonProperty("gmtOffSetMilliseconds")]
        public long GmtOffSetMilliseconds { get; set; }

        [JsonProperty("ipoExpectedDate", NullValueHandling = NullValueHandling.Ignore)]
        public string IpoExpectedDate { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("longName", NullValueHandling = NullValueHandling.Ignore)]
        public string LongName { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("marketCap", NullValueHandling = NullValueHandling.Ignore)]
        public long? MarketCap { get; set; }

        [JsonProperty("marketState")]
        public string MarketState { get; set; }

        [JsonProperty("messageBoardId")]
        public string MessageBoardId { get; set; }

        [JsonProperty("nameChangeDate", NullValueHandling = NullValueHandling.Ignore)]
        public string NameChangeDate { get; set; }

        [JsonProperty("postMarketChange", NullValueHandling = NullValueHandling.Ignore)]
        public double? PostMarketChange { get; set; }

        [JsonProperty("postMarketChangePercent", NullValueHandling = NullValueHandling.Ignore)]
        public double? PostMarketChangePercent { get; set; }

        [JsonProperty("postMarketPrice", NullValueHandling = NullValueHandling.Ignore)]
        public double? PostMarketPrice { get; set; }

        [JsonProperty("postMarketTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? PostMarketTime { get; set; }

        [JsonProperty("prevName", NullValueHandling = NullValueHandling.Ignore)]
        public string PrevName { get; set; }

        [JsonProperty("priceEpsCurrentYear", NullValueHandling = NullValueHandling.Ignore)]
        public double? PriceEpsCurrentYear { get; set; }

        [JsonProperty("priceHint")]
        public long PriceHint { get; set; }

        [JsonProperty("priceToBook", NullValueHandling = NullValueHandling.Ignore)]
        public double? PriceToBook { get; set; }

        [JsonProperty("quoteSourceName")]
        public string QuoteSourceName { get; set; }

        [JsonProperty("quoteType")]
        public string QuoteType { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("regularMarketChange")]
        public double RegularMarketChange { get; set; }

        [JsonProperty("regularMarketChangePercent")]
        public double RegularMarketChangePercent { get; set; }

        [JsonProperty("regularMarketDayHigh")]
        public double RegularMarketDayHigh { get; set; }

        [JsonProperty("regularMarketDayLow")]
        public double RegularMarketDayLow { get; set; }

        [JsonProperty("regularMarketDayRange")]
        public string RegularMarketDayRange { get; set; }

        [JsonProperty("regularMarketOpen")]
        public double RegularMarketOpen { get; set; }

        [JsonProperty("regularMarketPreviousClose")]
        public double RegularMarketPreviousClose { get; set; }

        [JsonProperty("regularMarketPrice")]
        public double RegularMarketPrice { get; set; }

        [JsonProperty("regularMarketTime")]
        public long RegularMarketTime { get; set; }

        [JsonProperty("regularMarketVolume")]
        public long RegularMarketVolume { get; set; }

        [JsonProperty("sharesOutstanding", NullValueHandling = NullValueHandling.Ignore)]
        public long? SharesOutstanding { get; set; }

        [JsonProperty("shortName")]
        public string ShortName { get; set; }

        [JsonProperty("sourceInterval")]
        public long SourceInterval { get; set; }

        [JsonProperty("symbol", Required = Required.Always)]
        public string Symbol { get; set; }

        [JsonProperty("tradeable")]
        public bool Tradeable { get; set; }

        [JsonProperty("trailingAnnualDividendRate", NullValueHandling = NullValueHandling.Ignore)]
        public double? TrailingAnnualDividendRate { get; set; }

        [JsonProperty("trailingAnnualDividendYield", NullValueHandling = NullValueHandling.Ignore)]
        public double? TrailingAnnualDividendYield { get; set; }

        [JsonProperty("trailingPE", NullValueHandling = NullValueHandling.Ignore)]
        public double? TrailingPE { get; set; }

        [JsonProperty("triggerable")]
        public bool Triggerable { get; set; }

        [JsonProperty("twoHundredDayAverage")]
        public double TwoHundredDayAverage { get; set; }

        [JsonProperty("twoHundredDayAverageChange")]
        public double TwoHundredDayAverageChange { get; set; }

        [JsonProperty("twoHundredDayAverageChangePercent")]
        public double TwoHundredDayAverageChangePercent { get; set; }

        [JsonProperty("typeDisp")]
        public string TypeDisp { get; set; }

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

                return LongName ?? ShortName ?? DisplayName ?? Symbol;
            }
        }

        [JsonIgnore]
        public string ExchangeDisplayName
        {
            get
            {
                if (FullExchangeName == "DJI")
                    return "Dow Jones";

                if (FullExchangeName.StartsWith("NASDAQ", StringComparison.OrdinalIgnoreCase))
                    return "NASDAQ";

                if (FullExchangeName == "SNP")
                    return "S&P 500";

                return FullExchangeName;
            }
        }

        [JsonIgnore]
        public TimeSpan GmtOffset
        {
            get
            {
                var offset = GmtOffSetMilliseconds;

                int hours = (int) offset / MillisecondsPerHour;
                offset = offset % MillisecondsPerHour;
                int minutes = (int) offset / MillisecondsPerMinute;
                offset = offset % MillisecondsPerMinute;
                int seconds = (int) offset / MillisecondsPerSecond;

                return new TimeSpan(hours, minutes, seconds);
            }
        }
    }
}
