using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

using LinearGradientPaint = LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint;

namespace Stocks.Views;

public partial class StockSymbolView : ViewCell
{
    YahooFinanceQuote quote;
    YahooFinanceChart chart;
    SKData data;

    public StockSymbolView(YahooFinanceQuote quote)
    {
        InitializeComponent();
        Update(quote);
    }

    public string Symbol { get { return quote.Symbol; } }

    public YahooFinanceQuote Quote { get { return quote; } }

    public void Update(YahooFinanceQuote quote)
    {
        this.quote = quote;

        NameLabel.Text = quote.Name;
        DescriptionLabel.Text = quote.Description;
        MarketPriceLabel.Text = quote.RegularMarketPrice.ToString("0,0.00");

        if (quote.RegularMarketChange >= 0.0)
        {
            MarketChangeLabelBorder.BackgroundColor = Color.Parse("#65C466");
            MarketChangeLabel.Text = string.Format("+{0:0.00}", quote.RegularMarketChange);
        }
        else
        {
            MarketChangeLabelBorder.BackgroundColor = Color.Parse("#EA4F3D");
            MarketChangeLabel.Text = string.Format("{0:0.00}", quote.RegularMarketChange);
        }
    }

    public void Update(YahooFinanceChart chart)
    {
        this.chart = chart;

        var color = quote.RegularMarketChange >= 0.0 ? SKColors.Green : SKColors.Red;

        var tradingStart = chart.Meta.TradingPeriods.Regular[0][0].Start;
        var tradingEnd = chart.Meta.TradingPeriods.Regular[0][0].End;
        var tradingMinutes = (int) ((tradingEnd - tradingStart) / 60);
        var baseline = new double[tradingMinutes];
        for (int i = 0; i < baseline.Length; i++)
            baseline[i] = quote.RegularMarketOpen;

        var close = chart.Indicators.Quote[0].Close;
        var values = new double?[tradingMinutes];
        var min = quote.RegularMarketOpen;
        var max = quote.RegularMarketOpen;

        for (int i = 0, v = 0; i < close.Length; i++)
        {
            if (chart.Timestamp[i] >= tradingStart && chart.Timestamp[i] < tradingEnd)
            {
                values[v++] = close[i];
                if (close[i].HasValue)
                {
                    min = Math.Min(min, close[i].Value);
                    max = Math.Max(max, close[i].Value);
                }
            }
        }

        var cartesianChart = new SKCartesianChart
        {
            Width = (int) MarketPriceChartImage.WidthRequest,
            Height = (int) MarketPriceChartImage.WidthRequest / 2,
            DrawMargin = new Margin(0),
            DrawMarginFrame = null,
            Series = new ISeries[]
            {
                new LineSeries<double?> {
                    Values = values,
                    Stroke = new SolidColorPaint(color, 1),
                    Fill = new LinearGradientPaint(color.WithAlpha(0x00), color.WithAlpha(0xaa), new SKPoint(0, 1), new SKPoint(0, 0)),
                    DataPadding = new LvcPoint(0, 0),
                    DataLabelsPadding = new Padding(0),
                    GeometryFill = null,
                    GeometrySize = 0
                },
                new LineSeries<double> {
                    Values = baseline,
                    Stroke = new SolidColorPaint(color, 1) { PathEffect = new DashEffect(new float[] { 6, 3 })},
                    Fill = null,
                    DataPadding = new LvcPoint(0, 0),
                    DataLabelsPadding = new Padding(0),
                    GeometryFill = null,
                    GeometrySize = 0
                }
            },
            XAxes = new[] { new Axis { ShowSeparatorLines = false, IsVisible = false, } },
            YAxes = new[] { new Axis { ShowSeparatorLines = false, IsVisible = false, MinLimit = min, MaxLimit = max } }
        };

        using var image = cartesianChart.GetImage();

        data?.Dispose();
        data = image.Encode(SKEncodedImageFormat.Png, 100);

        MarketPriceChartImage.Source = ImageSource.FromStream(data.AsStream);
    }
}