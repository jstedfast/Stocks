namespace Stocks.Views;

public class StockPriceChartTimeRange
{
    public string Name { get; private set; }
    public YahooFinanceChartTimeRange Value { get; private set; }

    public StockPriceChartTimeRange(string name, YahooFinanceChartTimeRange value)
    {
        Name = name;
        Value = value;
    }
}
