using System.Collections.ObjectModel;

using Stocks.Models;
using Stocks.Views;

namespace Stocks;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        App.Current.UserAppTheme = AppTheme.Dark;

        InitializeComponent();
        StockTableView.SelectionMode = SelectionMode.Single;
        StockTableView.SelectionChanged += OnStockSymbolSelected;

        MauiProgram.YahooFinanceThread = new YahooFinanceThread(SynchronizationContext.Current);
        var portfolio = MauiProgram.Portfolios.FirstOrDefault();

        if (portfolio != null)
        {
            StockTableView.ItemsSource = new ObservableCollection<Stock>(portfolio.Stocks);
            MauiProgram.YahooFinanceThread.Watch(portfolio);

            //foreach (var stock in portfolio.Stocks)
            //{
            //    var view = new StockSymbolView(stock);
            //    view.Tapped += OnStockSymbolTapped;
            //
            //    StockTableView.AddLogicalChild(view);
            //}
        }
        else
        {
            StockTableView.ItemsSource = new ObservableCollection<Stock>();
        }
    }

    async void OnStockSymbolSelected(object sender, SelectionChangedEventArgs e)
    {
        var stock = e.CurrentSelection?[0] as Stock;

        if (stock != null)
        {
            var page = new StockDetailsPage(stock);

            await Navigation.PushAsync(page, true);
        }
    }
}

