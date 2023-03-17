using Stocks.YahooFinance;

namespace Stocks;

public class StockSparkChangedEventArgs : EventArgs
{
    public StockSparkChangedEventArgs(YahooFinanceSpark spark)
    {
        Spark = spark;
    }

    public YahooFinanceSpark Spark { get; }
}
