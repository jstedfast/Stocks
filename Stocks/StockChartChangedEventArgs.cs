using Stocks.YahooFinance;

namespace Stocks
{
    public class StockChartChangedEventArgs
    {
        public StockChartChangedEventArgs(YahooFinanceTimeRange range, YahooFinanceChart chart)
        {
            Range = range;
            Chart = chart;
        }

        public YahooFinanceTimeRange Range { get; set; }

        public YahooFinanceChart Chart { get; }
    }
}
