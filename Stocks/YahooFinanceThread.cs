using Stocks.Models;
using Stocks.YahooFinance;

namespace Stocks;

public class YahooFinanceThread
{
    readonly SynchronizationContext synchronizationContext;
    readonly CancellationTokenSource cancellation;
    readonly YahooFinanceClient client;
    readonly Thread thread;

    StockPortfolio portfolio;

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

    void Start()
    {
        if (thread.ThreadState.HasFlag(ThreadState.Unstarted))
            thread.Start();
    }

    public void Stop()
    {
        if (!thread.ThreadState.HasFlag(ThreadState.Running))
            return;

        cancellation.Cancel();
    }

    class PortfolioContext
    {
        public StockPortfolio Portfolio { get; set; }
        public Dictionary<string, YahooFinanceQuote> Quotes { get; set; }
        public Dictionary<string, YahooFinanceSpark> Sparks { get; set; }
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

        //var json = JsonConvert.SerializeObject(context.Portfolio);
    }

    async void MainLoop()
    {
        var context = new PortfolioContext();

        while (!cancellation.IsCancellationRequested)
        {
            string[] symbols;

            lock(thread)
            {
                context.Portfolio = portfolio;
            }

            symbols = new string[portfolio.Stocks.Length];
            for (int i = 0; i < portfolio.Stocks.Length; i++)
                symbols[i] = portfolio.Stocks[i].Symbol;

            if (symbols.Length > 0)
            {
                try
                {
                    context.Quotes = await client.GetQuotesAsync(symbols, cancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    context.Quotes = null;
                }

                try
                {
                    context.Sparks = await client.GetSparksAsync(symbols, YahooFinanceIndicator.Close, YahooFinanceTimeRange.OneDay, YahooFinanceTimeInterval.OneMinute, cancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    context.Sparks = null;
                }

                if (context.Quotes != null || context.Sparks != null)
                    synchronizationContext.Post(UpdateStocks, context);
            }

            try
            {
                await Task.Delay(15 * 1000, cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
