using Stocks.Models;
using Stocks.YahooFinance;

namespace Stocks;

public class YahooFinanceThread
{
    readonly SynchronizationContext synchronizationContext;
    readonly YahooFinanceClient client;
    readonly Thread thread;

    CancellationTokenSource cancellation;
    StockPortfolio portfolio;

    YahooFinanceTimeRange range;
    Stock stock;

    public YahooFinanceThread(SynchronizationContext synchronizationContext)
    {
        this.synchronizationContext = synchronizationContext;
        cancellation = new CancellationTokenSource();
        client = new YahooFinanceClient();
        thread = new Thread(MainLoop) {
            Name = "Yahoo! Finance",
            IsBackground = true,
        };
    }

    public void Watch(StockPortfolio portfolio)
    {
        if (portfolio is null)
            throw new ArgumentNullException(nameof(portfolio));

        lock(thread)
        {
            this.portfolio = portfolio;
        }

        Start();
    }

    public void Watch(Stock stock, YahooFinanceTimeRange range)
    {
        lock (thread)
        {
            this.stock = stock;
            this.range = range;
        }
    }

    void Start()
    {
        if (thread.ThreadState.HasFlag(ThreadState.Unstarted))
        {
            cancellation = new CancellationTokenSource();
            thread.Start();
        }
    }

    public void Stop()
    {
        if (!thread.ThreadState.HasFlag(ThreadState.Running))
            return;

        cancellation.Cancel();

        try
        {
            thread.Join();
        }
        finally
        {
            cancellation.Dispose();
            cancellation = null;
        }
    }

    class PortfolioContext
    {
        public StockPortfolio Portfolio { get; set; }
        public Dictionary<string, YahooFinanceQuote> Quotes { get; set; }
        public Dictionary<string, YahooFinanceSpark> Sparks { get; set; }

        public Stock Stock { get; set; }
        public YahooFinanceTimeRange Range { get; set; }
        public YahooFinanceChart Chart { get; set; }
    }

    void UpdateStocks(object state)
    {
        var context = (PortfolioContext)state;

        foreach (var stock in context.Portfolio.Stocks)
        {
            if (context.Quotes != null && context.Quotes.TryGetValue(stock.Symbol, out var quote))
                stock.OnStockQuoteChanged(quote);

            if (context.Sparks != null && context.Sparks.TryGetValue(stock.Symbol, out var spark))
                stock.OnStockSparkChanged(spark);
        }

        if (context.Stock != null && context.Chart != null)
            context.Stock.OnStockChartChanged(context.Range, context.Chart);

        //var json = JsonConvert.SerializeObject(context.Portfolio);
    }

    static bool IsUpdatable(YahooFinanceTimeRange range)
    {
        switch (range)
        {
            case YahooFinanceTimeRange.OneDay:
            case YahooFinanceTimeRange.YearToDate:
            case YahooFinanceTimeRange.Max:
                return true;
            default:
                return false;
        }
    }

    void MainLoop()
    {
        while (!cancellation.IsCancellationRequested)
        {
            var context = new PortfolioContext();
            string[] symbols;

            lock(thread)
            {
                context.Portfolio = portfolio;
                context.Stock = stock;
                context.Range = range;
            }

            symbols = new string[portfolio.Stocks.Length];
            for (int i = 0; i < portfolio.Stocks.Length; i++)
                symbols[i] = portfolio.Stocks[i].Symbol;

            if (symbols.Length > 0)
            {
                try
                {
                    context.Quotes = client.GetQuotesAsync(symbols, cancellation.Token).GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                }

                try
                {
                    context.Sparks = client.GetSparksAsync(symbols, YahooFinanceIndicator.Close, YahooFinanceTimeRange.OneDay, YahooFinanceTimeInterval.OneMinute, cancellation.Token).GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                }
            }

            if (context.Stock != null && IsUpdatable(context.Range))
            {
                try
                {
                    context.Chart = client.GetChartAsync(context.Stock.Quote, context.Range, cancellation.Token).GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                }
            }

            if (context.Quotes != null || context.Sparks != null || context.Chart != null)
                synchronizationContext.Post(UpdateStocks, context);

            // Wait 15 seconds or until the thread is cancelled.
            cancellation.Token.WaitHandle.WaitOne(15 * 1000);
            if (cancellation.IsCancellationRequested)
                break;
        }
    }
}
