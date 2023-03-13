namespace Stocks;

public class StockQuoteChangedEventArgs : EventArgs
{
    public StockQuoteChangedEventArgs(YahooFinanceQuote quote)
    {
        Quote = quote;
    }

    public YahooFinanceQuote Quote { get; }
}
