namespace Stocks.Models;

class StockTradePoint
{
    public StockTradePoint(int index, DateTimeOffset timestamp, double? high, double? open, double? close, double? low)
    {
        Index = index;
        Timestamp = timestamp;
        Open = open;
        Close = close;
        High = high;
        Low = low;
    }

    public int Index { get; }

    public DateTimeOffset Timestamp { get; }

    public double? Open { get; }

    public double? Close { get; }

    public double? High { get; }

    public double? Low { get; }
}
