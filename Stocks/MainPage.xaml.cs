using Stocks.Views;

namespace Stocks;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        App.Current.UserAppTheme = AppTheme.Dark;

        InitializeComponent();

        MauiProgram.YahooFinanceThread = new YahooFinanceThread(SynchronizationContext.Current);
        var portfolio = MauiProgram.Portfolios.FirstOrDefault();

        if (portfolio != null)
        {
            MauiProgram.YahooFinanceThread.Watch(portfolio);

            foreach (var stock in portfolio.Stocks)
            {
                var view = new StockSymbolView(stock);
                view.Tapped += OnStockSymbolTapped;

                StockTableView.Root[0].Add(view);
            }
        }
    }

    async void OnStockSymbolTapped(object sender, EventArgs args)
    {
        var view = (StockSymbolView)sender;

        var page = new StockDetailsPage(view.Stock);

        await Navigation.PushAsync(page, true);
    }
}

