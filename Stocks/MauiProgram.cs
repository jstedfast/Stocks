using Newtonsoft.Json.Linq;

using SkiaSharp.Views.Maui.Controls.Hosting;

using Stocks.Models;

namespace Stocks;

public static class MauiProgram
{
    public static List<StockPortfolio> Portfolios { get; private set; }
    public static YahooFinanceThread YahooFinanceThread { get; set; }

    public static MauiApp CreateMauiApp()
    {
        Portfolios = new List<StockPortfolio>();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appDataDir, "com.microsoft.maui-samples.stocks");
        var path = Path.Combine(dir, "Portfolios.json");

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (File.Exists(path))
            File.Delete(path);

        if (!File.Exists(path))
        {
            using var resource = typeof(MauiProgram).Assembly.GetManifestResourceStream("Stocks.Portfolios.json");
            using var output = File.Create(path);
            resource.CopyTo(output);
            output.Flush();
        }

        var text = File.ReadAllText(path);
        var json = JObject.Parse(text);

        if (json.TryGetValue("portfolios", out var token) && token.Type == JTokenType.Array)
        {
            var array = (JArray)token;

            for (int i = 0; i < array.Count; i++)
            {
                if (array[i].Type != JTokenType.Object)
                    continue;

                var portfolio = ((JObject)array[i]).ToObject<StockPortfolio>();
                Portfolios.Add(portfolio);
            }
        }

        return builder.Build();
    }
}
