﻿using System.Globalization;

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
    readonly Stock stock;
    SKData data;

    public StockSymbolView(Stock stock)
    {
        InitializeComponent();
        this.stock = stock;
    }

    public string Symbol { get { return stock.Symbol; } }

    public Stock Stock => stock;

    protected override void OnAppearing()
    {
        stock.StockQuoteChanged += OnQuoteChanged;
        stock.StockSparkChanged += OnSparkChanged;

        if (stock.Quote != null)
            UpdateQuote(stock.Quote);

        if (stock.Spark != null)
        {
            try
            {
                UpdateSpark(stock.Spark);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        stock.StockQuoteChanged -= OnQuoteChanged;
        stock.StockSparkChanged -= OnSparkChanged;

        base.OnDisappearing();
    }

    void UpdateQuote(YahooFinanceQuote quote)
    {
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

    static int GetDataGranularityInSeconds(string dataGranularity)
    {
        int index = 0;

        while (index < dataGranularity.Length && char.IsAsciiDigit(dataGranularity[index]))
            index++;

        if (!int.TryParse(dataGranularity.AsSpan(0, index), NumberStyles.None, CultureInfo.InvariantCulture, out var value))
            return 1;

        var units = dataGranularity.AsSpan(index);

        if (units.Equals("m".AsSpan(), StringComparison.Ordinal))
            return value * 60;

        if (units.Equals("h".AsSpan(), StringComparison.Ordinal))
            return value * 60 * 60;

        if (units.Equals("d".AsSpan(), StringComparison.Ordinal))
            return value * 24 * 60 * 60;

        return value;
    }

    void UpdateSpark(YahooFinanceSpark spark)
    {
        var granularity = GetDataGranularityInSeconds(spark.Meta.DataGranularity);
        var tradingStart = spark.Meta.TradingPeriods[0][0].Start;
        var tradingEnd = spark.Meta.TradingPeriods[0][0].End;
        var tradingSeconds = tradingEnd - tradingStart;
        var dataPoints = tradingSeconds / granularity;
        var baseline = new double[dataPoints];
        for (int i = 0; i < baseline.Length; i++)
            baseline[i] = spark.Meta.PreviousClose;

        var close = spark.Indicators.Quote[0].Close;
        var values = new double?[dataPoints];
        var min = spark.Meta.PreviousClose;
        var max = spark.Meta.PreviousClose;

        for (int i = 0, v = 0; i < close.Length; i++)
        {
            if (spark.Timestamp[i] >= tradingStart && spark.Timestamp[i] < tradingEnd)
            {
                values[v++] = close[i];
                if (close[i].HasValue)
                {
                    min = Math.Min(min, close[i].Value);
                    max = Math.Max(max, close[i].Value);
                }
            }
            else
            {
                Console.WriteLine();
            }
        }

        var color = stock.Quote.RegularMarketChange >= 0.0 ? SKColors.Green : SKColors.Red;

        var cartesianChart = new SKCartesianChart
        {
            Background = SKColors.Transparent,
            Width = (int)MarketPriceChartImage.WidthRequest,
            Height = (int)MarketPriceChartImage.WidthRequest / 2,
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

    void OnQuoteChanged(object sender, StockQuoteChangedEventArgs e)
    {
        UpdateQuote(e.Quote);
    }

    void OnSparkChanged(object sender, StockSparkChangedEventArgs e)
    {
        try
        {
            UpdateSpark(e.Spark);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}