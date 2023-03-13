using Newtonsoft.Json;

namespace Stocks;

public class Stock
{
    [JsonProperty("symbol", Required = Required.Always)]
    public string Symbol { get; set; }

    [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
    public string DisplayName { get; set; }

    [JsonProperty("longName", NullValueHandling = NullValueHandling.Ignore)]
    public string LongName { get; set; }

    [JsonProperty("shortName", NullValueHandling = NullValueHandling.Ignore)]
    public string ShortName { get; set; }

    [JsonProperty("currency", Required = Required.Always)]
    public string Currency { get; set; }

    [JsonProperty("exchange", Required = Required.Always)]
    public string Exchange { get; set; }

    [JsonProperty("exchangeTimezoneName", Required = Required.Always)]
    public string ExchangeTimezoneName { get; set; }

    [JsonProperty("exchangeTimezoneShortName", Required = Required.Always)]
    public string ExchangeTimezoneShortName { get; set; }

    [JsonProperty("gmtOffSetMilliseconds")]
    public long GmtOffSetMilliseconds { get; set; }

    [JsonIgnore]
    public YahooFinanceQuote Quote { get; private set; }

    [JsonIgnore]
    public YahooFinanceSpark Spark { get; private set; }

    public event EventHandler<StockQuoteChangedEventArgs> StockQuoteChanged;

    public event EventHandler<StockSparkChangedEventArgs> StockSparkChanged;

    internal void OnStockQuoteChanged(YahooFinanceQuote quote)
    {
        Quote = quote;

        DisplayName = quote.DisplayName;
        LongName = quote.LongName;
        ShortName = quote.ShortName;
        Currency = quote.Currency;
        Exchange = quote.Exchange;
        ExchangeTimezoneName = quote.ExchangeTimezoneName;
        ExchangeTimezoneShortName = quote.ExchangeTimezoneShortName;
        GmtOffSetMilliseconds = quote.GmtOffSetMilliseconds;

        StockQuoteChanged?.Invoke(this, new StockQuoteChangedEventArgs(quote));
    }

    internal void OnStockSparkChanged(YahooFinanceSpark spark)
    {
        Spark = spark;
        StockSparkChanged?.Invoke(this, new StockSparkChangedEventArgs(spark));
    }
}
