using Stocks.YahooFinance;

namespace Stocks.Models;

public class StockPriceChartTimeRange
{
    public StockPriceChartTimeRange(string name, YahooFinanceTimeRange value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; private set; }

    public YahooFinanceTimeRange Value { get; private set; }
}
