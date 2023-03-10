using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Stocks.Views;

namespace Stocks;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appDataDir, "com.microsoft.maui-samples.stocks");
        var path = Path.Combine(dir, "Portfolio.json");

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (File.Exists(path))
            File.Delete(path);

        if (!File.Exists(path))
        {
            using var resource = GetType().Assembly.GetManifestResourceStream("Stocks.Portfolio.json");
            using var output = File.Create(path);
            resource.CopyTo(output);
            output.Flush();
        }

        var text = File.ReadAllText(path);
        var json = JObject.Parse(text);

        if (json.TryGetValue("symbols", out var token) && token.Type == JTokenType.Array)
        {
            var symbols = (JArray)token;

            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i].Type != JTokenType.Object)
                    continue;

                var quote = ((JObject)symbols[i]).ToObject<YahooFinanceQuote>();
                var view = new StockSymbolView(quote);
                view.Tapped += OnStockSymbolTapped;

                StockTableView.Root[0].Add(view);
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        RefreshStocks();
    }

    void RefreshStocks()
    {
        var dict = new Dictionary<string, StockSymbolView>();
        var symbols = new List<string>();
        YahooFinanceQuote[] quotes;

        foreach (var stockView in StockTableView.Root[0].OfType<StockSymbolView>())
        {
            dict.Add(stockView.Symbol, stockView);
            symbols.Add(stockView.Symbol);
        }

        //try
        //{
        //    await YahooFinanceClient.Default.GetSparkAsync(symbols, YahooTimeRange.OneDay);
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex);
        //    //throw;
        //}

        try
        {
            quotes = YahooFinanceClient.Default.GetQuotesAsync(symbols).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }

        foreach (var quote in quotes)
        {
            if (!dict.TryGetValue(quote.Symbol, out var view))
                continue;

            try
            {
                // update view
                view.Update(quote);
            }
            catch (Exception ex)
            {
                throw;
            }

            try
            {
                var chartData = YahooFinanceClient.Default.GetChartAsync(quote, YahooTimeRange.OneDay).GetAwaiter().GetResult();
                view.Update(chartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    async void OnStockSymbolTapped(object sender, EventArgs args)
    {
        var view = (StockSymbolView)sender;

        var page = new StockDetailsPage(view.Quote);

        await Navigation.PushAsync(page, true);
    }
}

