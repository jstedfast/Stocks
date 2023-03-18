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
        UpdateXAxis(YahooFinanceTimeRange.OneDay, values, 0);
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
    string HourlyLabeler(double x)
    {
        int index = Math.Min(Math.Max((int)x, 0), values.Count - 1);

        if (index < 0)
            return string.Empty;

        var trade = values[index];
        var timestamp = trade.Timestamp.ToLocalTime();
        var hour = timestamp.Hour;

        if (x < 0)
            hour--;

        if (hour > 12)
            hour -= 12;

        return hour.ToString();
    }

    string DailyLabeler(double x)
    {
        int index = Math.Min(Math.Max((int)x, 0), values.Count - 1);

        if (index < 0)
            return string.Empty;

        var trade = values[index];

        return trade.Timestamp.ToLocalTime().ToString("dd");
    }

    string MonthlyLabeler(double x)
    {
        int index = Math.Min(Math.Max((int)x, 0), values.Count - 1);

        if (index < 0)
            return string.Empty;

        var trade = values[index];

        return trade.Timestamp.ToLocalTime().ToString("MMM");
    }

    string YearlyLabeler(double x)
    {
        int index = Math.Min(Math.Max((int)x, 0), values.Count - 1);

        if (index < 0)
            return string.Empty;

        var trade = values[index];

        return trade.Timestamp.ToLocalTime().ToString("yyyy");
    }

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

    static double GetHourlyStepUnits(YahooFinanceTimeInterval interval)
    {
        switch (interval)
        {
            case YahooFinanceTimeInterval.OneMinute: return 60;
            case YahooFinanceTimeInterval.TwoMinutes: return 30;
            case YahooFinanceTimeInterval.FiveMinutes: return 12;
            case YahooFinanceTimeInterval.FifteenMinutes: return 4;
            case YahooFinanceTimeInterval.ThirtyMinutes: return 2;
            case YahooFinanceTimeInterval.SixtyMinutes:
            case YahooFinanceTimeInterval.OneHour: return 1;
            case YahooFinanceTimeInterval.NinetyMinutes: return 0.75;
            default: return 0;
        }
    }

    static double GetDailyStepUnits(YahooFinanceTradingPeriod tradingPeriod, YahooFinanceTimeInterval interval)
    {
        var duration = tradingPeriod.End - tradingPeriod.Start;
        var minutesPerDay = duration.TotalMinutes;

        switch (interval)
        {
            case YahooFinanceTimeInterval.OneMinute: return minutesPerDay;
            case YahooFinanceTimeInterval.TwoMinutes: return minutesPerDay / 2;
            case YahooFinanceTimeInterval.FiveMinutes: return minutesPerDay / 5;
            case YahooFinanceTimeInterval.FifteenMinutes: return minutesPerDay / 15;
            case YahooFinanceTimeInterval.ThirtyMinutes: return minutesPerDay / 30;
            case YahooFinanceTimeInterval.SixtyMinutes:
            case YahooFinanceTimeInterval.OneHour: return minutesPerDay / 60;
            case YahooFinanceTimeInterval.NinetyMinutes: return minutesPerDay / 90;
            case YahooFinanceTimeInterval.OneDay: return minutesPerDay;
            default: return 0;
        }
    }

    static double GetMonthlyStepUnits(YahooFinanceTradingPeriod tradingPeriod, YahooFinanceTimeInterval interval)
    {
        var duration = tradingPeriod.End - tradingPeriod.Start;
        var minutesPerDay = duration.TotalMinutes;
        const double daysPerMonth = 365.25 / 12;
        const double tradingDaysPerMonth = (daysPerMonth / 7) * 5;
        var tradingMinutesPerMonth = minutesPerDay * tradingDaysPerMonth;

        switch (interval)
        {
            case YahooFinanceTimeInterval.OneMinute: return tradingMinutesPerMonth;
            case YahooFinanceTimeInterval.TwoMinutes: return tradingMinutesPerMonth / 2;
            case YahooFinanceTimeInterval.FiveMinutes: return tradingMinutesPerMonth / 5;
            case YahooFinanceTimeInterval.FifteenMinutes: return tradingMinutesPerMonth / 15;
            case YahooFinanceTimeInterval.ThirtyMinutes: return tradingMinutesPerMonth / 30;
            case YahooFinanceTimeInterval.SixtyMinutes:
            case YahooFinanceTimeInterval.OneHour: return tradingMinutesPerMonth / 60;
            case YahooFinanceTimeInterval.NinetyMinutes: return tradingMinutesPerMonth / 90;
            case YahooFinanceTimeInterval.OneDay: return tradingDaysPerMonth;
            case YahooFinanceTimeInterval.FiveDays: return tradingDaysPerMonth / 5;
            default: return 0;
        }
    }

    static double GetYearlyStepUnits(YahooFinanceTradingPeriod tradingPeriod, YahooFinanceTimeInterval interval)
    {
        var duration = tradingPeriod.End - tradingPeriod.Start;
        var minutesPerDay = duration.TotalMinutes;
        const double daysPerYear = 365.25 / 12;
        const double tradingDaysPerYear = (daysPerYear / 7) * 5;
        var tradingMinutesPerYear = minutesPerDay * tradingDaysPerYear;

        switch (interval)
        {
            case YahooFinanceTimeInterval.OneMinute: return tradingMinutesPerYear;
            case YahooFinanceTimeInterval.TwoMinutes: return tradingMinutesPerYear / 2;
            case YahooFinanceTimeInterval.FiveMinutes: return tradingMinutesPerYear / 5;
            case YahooFinanceTimeInterval.FifteenMinutes: return tradingMinutesPerYear / 15;
            case YahooFinanceTimeInterval.ThirtyMinutes: return tradingMinutesPerYear / 30;
            case YahooFinanceTimeInterval.SixtyMinutes:
            case YahooFinanceTimeInterval.OneHour: return tradingMinutesPerYear / 60;
            case YahooFinanceTimeInterval.NinetyMinutes: return tradingMinutesPerYear / 90;
            case YahooFinanceTimeInterval.OneDay: return tradingDaysPerYear;
            case YahooFinanceTimeInterval.FiveDays: return tradingDaysPerYear / 5;
            default: return 0;
        }
    }

    static double GetXAxesMinStep(YahooFinanceTradingPeriod tradingPeriod, YahooFinanceTimeRange range, YahooFinanceTimeInterval interval, List<StockTradePoint> values)
    {
        switch (range)
        {
            case YahooFinanceTimeRange.OneDay: return GetHourlyStepUnits(interval);
            case YahooFinanceTimeRange.FiveDay: return GetDailyStepUnits(tradingPeriod, interval);
            case YahooFinanceTimeRange.OneMonth: return GetDailyStepUnits(tradingPeriod, interval);
            case YahooFinanceTimeRange.ThreeMonth: return GetMonthlyStepUnits(tradingPeriod, interval);
            case YahooFinanceTimeRange.SixMonth: return GetMonthlyStepUnits(tradingPeriod, interval);
            case YahooFinanceTimeRange.OneYear: return GetMonthlyStepUnits(tradingPeriod, interval);
            case YahooFinanceTimeRange.TwoYear: return GetMonthlyStepUnits(tradingPeriod, interval);
            case YahooFinanceTimeRange.FiveYear: return GetYearlyStepUnits(tradingPeriod, interval);
            case YahooFinanceTimeRange.TenYear: return GetYearlyStepUnits(tradingPeriod, interval);
            case YahooFinanceTimeRange.Max: return GetYearlyStepUnits(tradingPeriod, interval);
            case YahooFinanceTimeRange.YearToDate:
            default:
                if (values != null && values.Count > 0)
                {
                    var endTime = values[values.Count - 1].Timestamp;
                    var startTime = values[0].Timestamp;
                    var elapsed = endTime - startTime;

                    if (elapsed.Days < 1)
                        return GetHourlyStepUnits(interval);
                    if (elapsed.Days <= 31)
                        return GetDailyStepUnits(tradingPeriod, interval);
                    if (elapsed.Days <= 730)
                        GetMonthlyStepUnits(tradingPeriod, interval);
                    return GetYearlyStepUnits(tradingPeriod, interval);
                }
                return GetHourlyStepUnits(interval);
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

    void UpdateXAxis(YahooFinanceTimeRange range, List<StockTradePoint> values, double minStep)
    {
        var gridColor = App.Current.UserAppTheme == AppTheme.Light ? SKColors.LightGray : SKColors.DarkSlateGray;
        var labelColor = App.Current.UserAppTheme == AppTheme.Light ? SKColors.Black : SKColors.White;
        var maxLimit = values != null ? values.Count : 0;
        var labeler = GetXAxesLabeler(range, values);

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
                MinStep = minStep,
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

    static int GetIntervalMinutes(YahooFinanceTimeInterval interval)
    {
        switch (interval)
        {
            case YahooFinanceTimeInterval.OneMinute: return 1;
            case YahooFinanceTimeInterval.TwoMinutes: return 2;
            case YahooFinanceTimeInterval.FiveMinutes: return 5;
            case YahooFinanceTimeInterval.FifteenMinutes: return 15;
            case YahooFinanceTimeInterval.ThirtyMinutes: return 30;
            case YahooFinanceTimeInterval.SixtyMinutes:
            case YahooFinanceTimeInterval.OneHour: return 60;
            case YahooFinanceTimeInterval.NinetyMinutes: return 90;
            default: return 0;
        }
    }

    void UpdateChart(YahooFinanceTimeRange range, YahooFinanceChart chart)
    {
        var regularTradingPeriod = GetRegularMarketTradingPeriod(chart);
        var now = DateTimeOffset.UtcNow.ToOffset(regularTradingPeriod.GmtOffset);
        var tradingPeriodStartTime = regularTradingPeriod.Start.TimeOfDay;
        var tradingPeriodEndTime = regularTradingPeriod.End.TimeOfDay;
        var interval = chart.Meta.DataGranularity;
        var quote = chart.Indicators.Quote[0];
        var timestamps = chart.Timestamp;
        double open = 0, close = 0;
        double high = 0, low = 0;

        if (quote.Close != null)
        {
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

                if (open == 0)
                {
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

            if (range == YahooFinanceTimeRange.OneDay)
            {
                // Fill the remainder of the day with null values
                var timestamp = values[values.Count - 1].Timestamp;
                var endTime = regularTradingPeriod.End.TimeOfDay;
                var seconds = GetIntervalMinutes(interval) * 60;

                while (timestamp.TimeOfDay < endTime)
                {
                    timestamp = timestamp.AddSeconds(seconds);

                    values.Add(new StockTradePoint(values.Count, timestamp, null, null, null, null));
                }
            }
        }
        else
        {
            values = new List<StockTradePoint>();
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
                LineSmoothness = 0,
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

        var minStep = GetXAxesMinStep(regularTradingPeriod, range, interval, values);

        UpdateXAxis(range, values, minStep);
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