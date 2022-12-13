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
        JToken token;

        if (json.TryGetValue("symbols", out token) && token.Type == JTokenType.Array)
        {
            var symbols = (JArray)token;

            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i].Type != JTokenType.Object)
                    continue;

                var item = (JObject)symbols[i];
                if (!item.TryGetValue("symbol", out token) || token.Type != JTokenType.String)
                    continue;

                string symbol = (string)token;
                string name, description;
                double price, change;

                if (item.TryGetValue("name", out token) && token.Type == JTokenType.String)
                    name = (string)token;
                else
                    name = string.Empty;

                if (item.TryGetValue("description", out token) && token.Type == JTokenType.String)
                    description = (string)token;
                else
                    description = string.Empty;

                if (item.TryGetValue("marketPrice", out token) && token.Type == JTokenType.Float)
                    price = (double)token;
                else
                    price = 0;

                if (item.TryGetValue("marketChange", out token) && token.Type == JTokenType.Float)
                    change = (double)token;
                else
                    change = 0;

                var view = new StockSymbolView
                {
                    Symbol = symbol,
                    Name = name,
                    Description = description,
                    MarketPrice = price,
                    MarketChange = change
                };

                StockTableView.Root[0].Add(view);
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        RefreshStocksAsync();
    }

    async Task RefreshStocksAsync ()
    {
        var dict = new Dictionary<string, StockSymbolView>();
        var symbols = new List<string>();

        foreach (var stockView in StockTableView.Root[0].OfType<StockSymbolView>())
        {
            dict.Add(stockView.Symbol, stockView);
            symbols.Add(stockView.Symbol);
        }

        var quotes = await YahooFinance.GetQuotesAsync(symbols);

        foreach (var quote in quotes)
        {
            if (!dict.TryGetValue(quote.Symbol, out var view))
                continue;

            // update view
            view.Update(quote);
        }
    }
}

