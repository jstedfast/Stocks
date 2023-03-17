using System;
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

using SkiaSharp;

using Stocks.Models;
using Stocks.YahooFinance;

using LinearGradientPaint = LiveChartsCore.SkiaSharpView.Painting.LinearGradientPaint;

namespace Stocks.Views;

public partial class StockDetailsPage : ContentPage
{
    static readonly StockPriceChartTimeRange[] TimeRanges = new StockPriceChartTimeRange[]
    {
        new StockPriceChartTimeRange("1D", YahooFinanceTimeRange.OneDay),
        new StockPriceChartTimeRange("1W", YahooFinanceTimeRange.FiveDay),
        new StockPriceChartTimeRange("1M", YahooFinanceTimeRange.OneMonth),
        new StockPriceChartTimeRange("3M", YahooFinanceTimeRange.ThreeMonth),
        new StockPriceChartTimeRange("6M", YahooFinanceTimeRange.SixMonth),
        new StockPriceChartTimeRange("YTD", YahooFinanceTimeRange.YearToDate),
        new StockPriceChartTimeRange("1Y", YahooFinanceTimeRange.OneYear),
        new StockPriceChartTimeRange("2Y", YahooFinanceTimeRange.TwoYear),
        new StockPriceChartTimeRange("5Y", YahooFinanceTimeRange.FiveYear),
        new StockPriceChartTimeRange("10Y", YahooFinanceTimeRange.TenYear),
        new StockPriceChartTimeRange("ALL", YahooFinanceTimeRange.Max),
    };

    const long OneTrillion = 1000000000000;
    const long OneHundredBillion = 100000000000;
    const long TenBillion = 10000000000;
    const long OneBillion = 1000000000;
    const long OneHundredMillion = 100000000;
    const long TenMillion = 10000000;
    const long OneMillion = 1000000;
    const double OneYear = 365.25;
    const double OneMonth = OneYear / 12;
    const double OneWeek = 7;
    const double OneDay = 1;
    readonly Stock stock;

    CancellationTokenSource cancellationTokenSource;
    List<StockTradePoint> values;

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
        StockPriceChart.DrawMargin = new Margin(10, 5, 70, 30);
        UpdateXAxis(YahooFinanceTimeRange.OneDay, YahooFinanceTimeInterval.OneMinute, values);
        UpdateYAxis(null, null);

        foreach (var radio in StockPriceChartTimeRangesLayout.Children.OfType<RadioButton>())
        {
            if (radio.Value is YahooFinanceTimeRange range && range == YahooFinanceTimeRange.OneDay)
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
        if (quote.RegularMarketChange > 0)
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

        if (units.Equals("wk".AsSpan(), StringComparison.Ordinal))
            return TimeSpan.FromDays(value * 7);

        if (units.Equals("mo".AsSpan(), StringComparison.Ordinal))
            return TimeSpan.FromDays(value * OneMonth);

        return TimeSpan.FromSeconds(value);
    }

#if false
    static string HourlyLabeler(double ticks)
    {
        var utcTimestamp = new DateTime((long)ticks, DateTimeKind.Utc);
        var timestamp = utcTimestamp.ToLocalTime();
        var hour = timestamp.Hour;

        if (hour > 12)
            hour -= 12;

        return hour.ToString();
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
#else
    string HourlyLabeler(double index)
    {
        var trade = values[Math.Min(Math.Max((int)index, 0), values.Count - 1)];
        var timestamp = trade.Timestamp.ToLocalTime();
        var hour = timestamp.Hour;

        if (index < 0)
            hour--;

        if (hour > 12)
            hour -= 12;

        return hour.ToString();
    }

    string DailyLabeler(double index)
    {
        var trade = values[Math.Min(Math.Max((int)index, 0), values.Count - 1)];
        return trade.Timestamp.ToLocalTime().ToString("dd");
    }

    string MonthlyLabeler(double index)
    {
        var trade = values[Math.Min(Math.Max((int)index, 0), values.Count - 1)];
        return trade.Timestamp.ToLocalTime().ToString("MMM");
    }

    string YearlyLabeler(double index)
    {
        var trade = values[Math.Min(Math.Max((int)index, 0), values.Count - 1)];
        return trade.Timestamp.ToLocalTime().ToString("yyyy");
    }
#endif

    Func<double, string> GetXAxesLabeler(YahooFinanceTimeRange range, List<StockTradePoint> values)
    {
        switch (range)
        {
            case YahooFinanceTimeRange.OneDay:     return HourlyLabeler;
            case YahooFinanceTimeRange.FiveDay:    return DailyLabeler;
            case YahooFinanceTimeRange.OneMonth:   return DailyLabeler;
            case YahooFinanceTimeRange.ThreeMonth: return MonthlyLabeler;
            case YahooFinanceTimeRange.SixMonth:   return MonthlyLabeler;
            case YahooFinanceTimeRange.OneYear:    return MonthlyLabeler;
            case YahooFinanceTimeRange.TwoYear:    return MonthlyLabeler;
            case YahooFinanceTimeRange.FiveYear:   return YearlyLabeler;
            case YahooFinanceTimeRange.TenYear:    return YearlyLabeler;
            case YahooFinanceTimeRange.Max:        return YearlyLabeler;
            case YahooFinanceTimeRange.YearToDate:
            default:
                if (values != null && values.Count > 0)
                {
                    var endTime = values[values.Count - 1].Timestamp;
                    var startTime = values[0].Timestamp;
                    var elapsed = endTime - startTime;

                    if (elapsed.Days < 1)
                        return HourlyLabeler;
                    if (elapsed.Days <= 31)
                        return DailyLabeler;
                    if (elapsed.Days <= 730)
                        return MonthlyLabeler;
                    return YearlyLabeler;
                }
                return HourlyLabeler;
        }
    }

    static string StockTradeTooltipFormatter(ChartPoint<StockTradePoint, BezierPoint<CircleGeometry>, LabelGeometry> point)
    {
        var dateTime = point.Model.Timestamp.ToLocalTime().ToString("dddd, MMMM dd h:mm tt");
        var close = Format(point.Model.Close);
        var open = Format(point.Model.Open);
        var high = Format(point.Model.High);
        var low = Format(point.Model.Low);

        return $"{dateTime}\r\nOpen: {open}\r\nClose: {close}\r\nHigh: {high}\r\nLow: {low}";
    }

    void UpdateXAxis(YahooFinanceTimeRange range, YahooFinanceTimeInterval interval, List<StockTradePoint> values)
    {
        var gridColor = App.Current.UserAppTheme == AppTheme.Light ? SKColors.LightGray : SKColors.DarkSlateGray;
        var labelColor = App.Current.UserAppTheme == AppTheme.Light ? SKColors.Black : SKColors.White;
        var maxLimit = values != null ? values.Count : 0;
        var labeler = GetXAxesLabeler(range, values);
        TimeSpan timeRange;
        double minStep = 1;

        if (values != null && values.Count > 0)
        {
            var close = values[values.Count - 1].Timestamp;
            var open = values[0].Timestamp;

            timeRange = close - open;
        }
        else
        {
            timeRange = TimeSpan.Zero;
        }

        if (timeRange.TotalDays < OneDay)
        {
            minStep = TimeSpan.FromHours(1).Ticks;
        }
        else if (timeRange.TotalDays < OneWeek)
        {
            minStep = TimeSpan.FromDays(OneDay).Ticks;
        }
        else if (timeRange.TotalDays < OneMonth)
        {
            minStep = TimeSpan.FromDays(OneWeek).Ticks;
        }
        else if (timeRange.TotalDays < OneMonth * 6)
        {
            minStep = TimeSpan.FromDays(OneMonth).Ticks;
        }
        else if (timeRange.TotalDays < OneYear * 2)
        {
            var months = timeRange.TotalDays / OneMonth;
            var multiplier = months / 5;

            minStep = TimeSpan.FromDays(OneMonth * multiplier).Ticks;
        }
        else
        {
            var years = timeRange.TotalDays / OneYear;
            var multiplier = years / 5;

            minStep = TimeSpan.FromDays(OneYear * multiplier).Ticks;
        }

        StockPriceChart.XAxes = new[] {
            new Axis {
                SeparatorsPaint = new SolidColorPaint(gridColor),
                LabelsPaint = new SolidColorPaint(labelColor),
                LabelsAlignment = Align.Start,
                Position = AxisPosition.Start,
                //SeparatorsAtCenter = false,
                ShowSeparatorLines = true,
                //ForceStepToMin = true,
                //UnitWidth = timeUnit.Ticks,
                MinLimit = 0,
                MaxLimit = maxLimit,
                //MinStep = minStep,
                Labeler = labeler,
                IsVisible = true
            }
        };
    }

    void UpdateYAxis(double? minLimit, double? maxLimit)
    {
        var gridColor = App.Current.UserAppTheme == AppTheme.Light ? SKColors.LightGray : SKColors.DarkSlateGray;
        var labelColor = App.Current.UserAppTheme == AppTheme.Light ? SKColors.Black : SKColors.White;

        // Note: grow the vertical scale by 1% in each direction so we have a little buffer above/below.
        //if (maxLimit.HasValue)
        //    maxLimit *= 1.01;
        //if (minLimit.HasValue)
        //    minLimit *= 0.99;

        StockPriceChart.YAxes = new[] {
            new Axis {
                SeparatorsPaint = new SolidColorPaint(gridColor),
                LabelsPaint = new SolidColorPaint(labelColor),
                LabelsAlignment = Align.Start,
                Position = AxisPosition.End,
                ShowSeparatorLines = true,
                MinLimit = minLimit,
                MaxLimit = maxLimit,
                IsVisible = true
            }
        };
    }

    static YahooFinanceTradingPeriod GetRegularMarketTradingPeriod(YahooFinanceChart chart)
    {
        return chart.Meta.CurrentTradingPeriod?.Regular ?? chart.Meta.TradingPeriods?.Regular[0][0];
    }

    void UpdateChart(YahooFinanceTimeRange range, YahooFinanceChart chart)
    {
        var regularTradingPeriod = GetRegularMarketTradingPeriod(chart);
        var tradingPeriodStartTime = regularTradingPeriod.Start.TimeOfDay;
        var tradingPeriodEndTime = regularTradingPeriod.End.TimeOfDay;
        var interval = chart.Meta.DataGranularity;
        var quote = chart.Indicators.Quote[0];
        var timestamps = chart.Timestamp;
        double open = 0, close = 0;
        double high = 0, low = 0;

        values = new List<StockTradePoint>(quote.Close.Length);

        for (int i = 0; i < quote.Close.Length; i++)
        {
            // Ignore null values
            if (!quote.High[i].HasValue && !quote.Open[i].HasValue && !quote.Close[i].HasValue && !quote.Low[i].HasValue)
                continue;

            var timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamps[i]).ToOffset(chart.Meta.GmtOffset);

            // Note: Larger time intervals aren't necessarily timestamped within the regular trading period timeframe.
            if (interval < YahooFinanceTimeInterval.OneWeek)
            {
                var time = timestamp.TimeOfDay;

                // Ignore pre/post trading data
                if (time < tradingPeriodStartTime || time > tradingPeriodEndTime)
                    continue;
            }

            values.Add(new StockTradePoint(values.Count, timestamp, quote.High[i], quote.Open[i], quote.Close[i], quote.Low[i]));

            if (open == 0) {
                if (quote.Open[i].HasValue)
                    open = quote.Open[i].Value;
                else if (quote.Close[i].HasValue)
                    open = quote.Close[i].Value;
                low = open;
            }

            if (quote.High[i].HasValue)
                high = Math.Max(high, quote.High[i].Value);

            if (quote.Low[i].HasValue)
                low = Math.Min(low, quote.Low[i].Value);

            if (quote.Close[i].HasValue)
                close = quote.Close[i].Value;
        }

        var color = close >= open ? SKColors.Green : SKColors.Red;

        StockPriceChart.Series = new ISeries[]
        {
            new LineSeries<StockTradePoint>
            {
                Values = values,
                Stroke = new SolidColorPaint(color, 2),
                Fill = new LinearGradientPaint(color.WithAlpha(0x00), color.WithAlpha(0xaa), new SKPoint(0, 1), new SKPoint(0, 0)),
                TooltipLabelFormatter = StockTradeTooltipFormatter,
                EnableNullSplitting = false,
                GeometryFill = null,
                GeometrySize = 0,
                Mapping = (trade, point) =>
                {
                    point.SecondaryValue = trade.Index;
                    if (trade.Close.HasValue)
                        point.PrimaryValue = trade.Close.Value;
                    //point.PrimaryValue = trade.High;
                    //point.TertiaryValue = stock.Open;
                    //point.QuaternaryValue = stock.Close;
                    //point.QuinaryValue = stock.Low;
                },
            }
        };

        UpdateXAxis(range, interval, values);
        UpdateYAxis(low, high);
    }

    async void OnStockPriceChartTimeRangeRadioChecked(object sender, CheckedChangedEventArgs e)
    {
        var radio = (RadioButton)sender;

        if (radio.Value is YahooFinanceTimeRange range)
        {
            if (radio.IsChecked)
            {
                var cancellation = new CancellationTokenSource();
                cancellationTokenSource = cancellation;

                try
                {
                    var chart = await YahooFinanceClient.Default.GetChartAsync(stock.Quote, range, cancellation.Token);
                    UpdateChart(range, chart);
                    //MauiProgram.YahooFinanceThread.WatchChart(stock);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
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