namespace Stocks.Views;

public class StockPriceChartTimeRange
{
    public string Name { get; private set; }
    public YahooTimeRange Value { get; private set; }

    public StockPriceChartTimeRange(string name, YahooTimeRange value)
    {
        Name = name;
        Value = value;
    }
}
