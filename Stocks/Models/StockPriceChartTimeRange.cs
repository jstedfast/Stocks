using Stocks.YahooFinance;

namespace Stocks.Models;

public class StockPriceChartTimeRange
{
    public StockPriceChartTimeRange(string name, YahooFinanceChartTimeRange value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; private set; }

    public YahooFinanceChartTimeRange Value { get; private set; }
}
