using System.Collections.ObjectModel;
using System.Globalization;

using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

using LinearGradientPaint = LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint;

namespace Stocks.Views;

public partial class StockDetailsPage : ContentPage
{
    static readonly StockPriceChartTimeRange[] TimeRanges = new StockPriceChartTimeRange[]
    {
        new StockPriceChartTimeRange("1D", YahooTimeRange.OneDay),
        new StockPriceChartTimeRange("1W", YahooTimeRange.FiveDay),
        new StockPriceChartTimeRange("1M", YahooTimeRange.OneMonth),
        new StockPriceChartTimeRange("3M", YahooTimeRange.ThreeMonth),
        new StockPriceChartTimeRange("6M", YahooTimeRange.SixMonth),
        new StockPriceChartTimeRange("YTD", YahooTimeRange.YearToDate),
        new StockPriceChartTimeRange("1Y", YahooTimeRange.OneYear),
        new StockPriceChartTimeRange("2Y", YahooTimeRange.TwoYear),
        new StockPriceChartTimeRange("5Y", YahooTimeRange.FiveYear),
        new StockPriceChartTimeRange("10Y", YahooTimeRange.TenYear),
        new StockPriceChartTimeRange("ALL", YahooTimeRange.Max),
    };

    const long OneTrillion = 1000000000000;
    const long OneHundredBillion = 100000000000;
    const long TenBillion = 10000000000;
    const long OneBillion = 1000000000;
    const long OneHundredMillion = 100000000;
    const long TenMillion = 10000000;
    const long OneMillion = 1000000;
    readonly Stock stock;

    CancellationTokenSource cancellationTokenSource;

    public StockDetailsPage(Stock stock)
    {
        InitializeComponent();
        this.stock = stock;

        StockPriceChart.Series = new ISeries[] {
            new LineSeries<FinancialPoint>
            {
                Values = Array.Empty<FinancialPoint>(),
                GeometryFill = null,
                GeometrySize = 0
            }
        };
        StockPriceChart.TooltipFindingStrategy = TooltipFindingStrategy.CompareOnlyX;
        StockPriceChart.DrawMargin = new Margin(0);
        UpdateXAxis(YahooTimeRange.OneDay, null, null);
        UpdateYAxis(TimeSpan.FromMinutes(1), null, null);

        foreach (var radio in StockPriceChartTimeRangesLayout.Children.OfType<RadioButton>())
        {
            if (radio.Value is YahooTimeRange range && range == YahooTimeRange.OneDay)
            {
                radio.IsChecked = true;
                OnStockPriceChartTimeRangeRadioChecked(radio, null);
            }

            radio.CheckedChanged += OnStockPriceChartTimeRangeRadioChecked;
        }
    }

    public StockPriceChartTimeRange[] StockPriceChartTimeRanges => TimeRanges;

    protected override Size ArrangeOverride(Rect bounds)
    {
        double maxWidth = 0, maxHeight = 0;

        foreach (var radio in StockPriceChartTimeRangesLayout.Children.OfType<RadioButton>())
        {
            maxHeight = Math.Max(maxHeight, radio.Height);
            maxWidth = Math.Max(maxWidth, radio.Width);
        }

        foreach (var radio in StockPriceChartTimeRangesLayout.Children.OfType<RadioButton>())
        {
            radio.HeightRequest = maxHeight;
            radio.WidthRequest = maxWidth;
        }

        return base.ArrangeOverride(bounds);
    }

    protected override void OnAppearing()
    {
        UpdateQuote(stock.Quote);
        stock.StockQuoteChanged += OnStockQuoteChanged;

        base.OnAppearing();
    }

    void CancelChartUpdateOperation()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
        }
    }

    protected override void OnDisappearing()
    {
        stock.StockQuoteChanged -= OnStockQuoteChanged;

        CancelChartUpdateOperation();

        base.OnDisappearing();
    }

    static string Format(long? value)
    {
        if (!value.HasValue)
            return "-";

        long v = value.Value;

        if (v > OneTrillion)
            return string.Format("{0:0.000}T", ((double)v) / OneTrillion);

        if (v > OneHundredBillion)
            return string.Format("{0:0.0}B", ((double)v) / OneBillion);

        if (v > TenBillion)
            return string.Format("{0:0.00}B", ((double)v) / OneBillion);

        if (v > OneBillion)
            return string.Format("{0:0.000}B", ((double)v) / OneBillion);

        if (v > OneHundredMillion)
            return string.Format("{0:0.0}M", ((double)v) / OneMillion);

        if (v > TenMillion)
            return string.Format("{0:0.00}M", ((double)v) / OneMillion);

        if (v > OneMillion)
            return string.Format("{0:0.000}M", ((double)v) / OneMillion);

        return value.Value.ToString("0,0");
    }

    static string Format(double? value)
    {
        if (!value.HasValue)
            return "-";

        return value.Value.ToString("0,0.00");
    }

    void UpdateQuote(YahooFinanceQuote quote)
    {
        Title = quote.Name;

        SubtitleLabel.Text = quote.Description;

        MarketPriceLabel.Text = Format(quote.RegularMarketPrice);
        if (quote.RegularMarketPrice > 0)
        {
            MarketChangeLabel.Text = string.Format("+{0:0.00}", quote.RegularMarketChange);
            MarketChangeLabel.TextColor = Color.Parse("Green");
        }
        else
        {
            MarketChangeLabel.Text = string.Format("{0:0.00}", quote.RegularMarketChange);
            MarketChangeLabel.TextColor = Color.Parse("Red");
        }

        ExchangeLabel.Text = quote.ExchangeDisplayName;
        CurrencyLabel.Text = quote.Currency;

        OpenLabel.Text = Format(quote.RegularMarketOpen);
        HighLabel.Text = Format(quote.RegularMarketDayHigh);
        LowLabel.Text = Format(quote.RegularMarketDayLow);

        VolumeLabel.Text = Format(quote.RegularMarketVolume);
        PELabel.Text = Format(quote.TrailingPE);
        MarketCapLabel.Text = Format(quote.MarketCap);

        FiftyTwoWeekHighLabel.Text = Format(quote.FiftyTwoWeekHigh);
        FiftyTwoWeekLowLabel.Text = Format(quote.FiftyTwoWeekLow);
        AverageVolumeLabel.Text = Format(quote.AverageDailyVolume3Month);

        YieldLabel.Text = "-";
        BetaLabel.Text = "-";
        EPSLabel.Text = Format(quote.EpsTrailingTwelveMonths);
    }

    void OnStockQuoteChanged(object sender, StockQuoteChangedEventArgs e)
    {
        UpdateQuote(e.Quote);
    }

    static TimeSpan GetDataGranularity(string dataGranularity)
    {
        int index = 0;

        while (index < dataGranularity.Length && char.IsAsciiDigit(dataGranularity[index]))
            index++;

        if (!int.TryParse(dataGranularity.AsSpan(0, index), NumberStyles.None, CultureInfo.InvariantCulture, out var value))
            return TimeSpan.FromSeconds(1);

        var units = dataGranularity.AsSpan(index);

        if (units.Equals("m".AsSpan(), StringComparison.Ordinal))
            return TimeSpan.FromMinutes(value);

        if (units.Equals("h".AsSpan(), StringComparison.Ordinal))
            return TimeSpan.FromHours(value);

        if (units.Equals("d".AsSpan(), StringComparison.Ordinal))
            return TimeSpan.FromDays(value);

        return TimeSpan.FromSeconds(value);
    }

    static string HourlyLabeler(double ticks)
    {
        var utcTimestamp = new DateTime((long)ticks, DateTimeKind.Utc);
        var timestamp = utcTimestamp.ToLocalTime();

        return timestamp.ToString("hh");
    }

    static string DailyLabeler(double ticks)
    {
        return new DateTime((long)ticks, DateTimeKind.Utc).ToLocalTime().ToString("dd");
    }

    static string MonthlyLabeler(double ticks)
    {
        return new DateTime((long)ticks, DateTimeKind.Utc).ToLocalTime().ToString("MMM");
    }

    static string YearlyLabeler(double ticks)
    {
        return new DateTime((long)ticks, DateTimeKind.Utc).ToLocalTime().ToString("yyyy");
    }

    static string HourlyTooltipFormatter(ChartPoint<FinancialPoint, BezierPoint<CircleGeometry>, LabelGeometry> point)
    {
        var value = point.Model.Close.HasValue ? point.Model.Close.Value.ToString("0,0.00") : "-";
        var dateTime = point.Model.Date.ToLocalTime().ToString("t");

        return $"{dateTime}\r\n{value}";
    }

    static string DefaultTooltipFormatter(ChartPoint<FinancialPoint, BezierPoint<CircleGeometry>, LabelGeometry> point)
    {
        var value = point.Model.Close.HasValue ? point.Model.Close.Value.ToString("0,0.00") : "-";
        var dateTime = point.Model.Date.ToLocalTime().ToString("dddd, MMMM dd h:mm tt");

        return $"{dateTime}\r\n{value}";
    }

    void UpdateXAxis(YahooTimeRange range, double? minLimit, double? maxLimit)
    {
        var theme = App.Current.UserAppTheme;
        var labelColor = theme == AppTheme.Light ? SKColors.Black : SKColors.White;
        var gridColor = theme == AppTheme.Light ? SKColors.LightGray : SKColors.DarkSlateGray;
        Func<double, string> labeler;
        double minStep;

        if (range == YahooTimeRange.OneDay)
        {
            minStep = TimeSpan.FromHours(1).Ticks;
            labeler = HourlyLabeler;
        }
        else if (range < YahooTimeRange.ThreeMonth)
        {
            minStep = TimeSpan.FromDays(1).Ticks;
            labeler = DailyLabeler;
        }
        else if (range < YahooTimeRange.FiveYear || range == YahooTimeRange.YearToDate)
        {
            minStep = TimeSpan.FromDays(30.4375).Ticks;
            labeler = MonthlyLabeler;
        }
        else
        {
            minStep = TimeSpan.FromDays(365.25).Ticks;
            labeler = YearlyLabeler;
        }

        StockPriceChart.XAxes = new[] {
            new Axis {
                SeparatorsPaint = new SolidColorPaint(gridColor),
                LabelsPaint = new SolidColorPaint(labelColor),
                Position = AxisPosition.Start,
                ShowSeparatorLines = true,
                MinLimit = minLimit,
                MaxLimit = maxLimit,
                MinStep = minStep,
                Labeler = labeler,
                IsVisible = true
            }
        };
    }

    void UpdateYAxis(TimeSpan timeUnit, double? minLimit, double? maxLimit)
    {
        var theme = App.Current.UserAppTheme;
        var labelColor = theme == AppTheme.Light ? SKColors.Black : SKColors.White;
        var gridColor = theme == AppTheme.Light ? SKColors.LightGray : SKColors.DarkSlateGray;

        StockPriceChart.YAxes = new[] {
            new Axis {
                SeparatorsPaint = new SolidColorPaint(gridColor),
                LabelsPaint = new SolidColorPaint(labelColor),
                Position = AxisPosition.End,
                UnitWidth = timeUnit.Ticks,
                ShowSeparatorLines = true,
                MinLimit = minLimit,
                MaxLimit = maxLimit,
                IsVisible = true
            }
        };
    }

    void UpdateChart(YahooTimeRange range, YahooFinanceChart chart)
    {
        var timeUnits = GetDataGranularity(chart.Meta.DataGranularity);
        var quote = chart.Indicators.Quote[0];
        double startPrice = -1, endPrice = -1;
        var timestamps = chart.Timestamp;
        double? minClosingPrice = null;
        double? maxClosingPrice = null;
        double? minTimestamp = null;
        double? maxTimestamp = null;

        var values = new FinancialPoint[quote.Close.Length];

        if (quote.Close.Length > 0)
        {
            maxTimestamp = DateTime.UnixEpoch.AddSeconds(timestamps[timestamps.Length - 1]).Ticks;
            minTimestamp = DateTime.UnixEpoch.AddSeconds(timestamps[0]).Ticks;
        }

        for (int i = 0; i < quote.Close.Length; i++)
        {
            values[i] = new FinancialPoint(DateTime.UnixEpoch.AddSeconds(timestamps[i]), quote.High[i], quote.Open[i], quote.Close[i], quote.Low[i]);

            if (quote.Close[i].HasValue)
            {
                minClosingPrice = minClosingPrice.HasValue ? Math.Min(minClosingPrice.Value, quote.Close[i].Value) : quote.Close[i].Value;
                maxClosingPrice = maxClosingPrice.HasValue ? Math.Max(maxClosingPrice.Value, quote.Close[i].Value) : quote.Close[i].Value;

                if (startPrice < 0)
                    startPrice = quote.Close[i].Value;

                endPrice = quote.Close[i].Value;
            }
        }

        var color = endPrice >= startPrice ? SKColors.Green : SKColors.Red;

        StockPriceChart.Series = new ISeries[]
        {
            new LineSeries<FinancialPoint>
            {
                Values = values,
                Stroke = new SolidColorPaint(color, 1),
                Fill = new LinearGradientPaint(color.WithAlpha(0x00), color.WithAlpha(0xaa), new SKPoint(0, 1), new SKPoint(0, 0)),
                TooltipLabelFormatter = DefaultTooltipFormatter,
                GeometryFill = null,
                GeometrySize = 0
            }
        };

        UpdateXAxis(range, minTimestamp, maxTimestamp);
        UpdateYAxis(timeUnits, minClosingPrice, maxClosingPrice);
    }

    async void OnStockPriceChartTimeRangeRadioChecked(object sender, CheckedChangedEventArgs e)
    {
        var radio = (RadioButton)sender;

        if (radio.Value is YahooTimeRange range)
        {
            if (radio.IsChecked)
            {
                var cancellation = new CancellationTokenSource();
                cancellationTokenSource = cancellation;

                try
                {
                    var chart = await YahooFinanceClient.Default.GetChartAsync(stock.Quote, range, cancellation.Token);
                    UpdateChart(range, chart);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    cancellationTokenSource = null;
                    cancellation.Dispose();
                }
            }
            else
            {
                CancelChartUpdateOperation();
            }
        }
    }
}