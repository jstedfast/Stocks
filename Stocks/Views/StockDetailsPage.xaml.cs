using System.Collections.ObjectModel;

using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Maui.ApplicationModel;
using SkiaSharp;

namespace Stocks.Views;

public partial class StockDetailsPage : ContentPage
{
    const long OneTrillion = 1000000000000;
    const long OneHundredBillion = 100000000000;
    const long TenBillion = 10000000000;
    const long OneBillion = 1000000000;
    const long OneHundredMillion = 100000000;
    const long TenMillion = 10000000;
    const long OneMillion = 1000000;
    readonly Stock stock;
    Button currentTab;
    Task task;

    public StockDetailsPage(Stock stock)
    {
        InitializeComponent();
        this.stock = stock;

        StockPriceChart.Series = new ISeries[]
        {
            new CandlesticksSeries<FinancialPoint>
            {
                Values = new ObservableCollection<FinancialPoint>()
                {
                    new(new DateTime(2021, 1, 1), 523, 500, 450, 400),
                    new(new DateTime(2021, 1, 2), 500, 450, 425, 400),
                    new(new DateTime(2021, 1, 3), 490, 425, 400, 380),
                    new(new DateTime(2021, 1, 4), 420, 400, 420, 380),
                    new(new DateTime(2021, 1, 5), 520, 420, 490, 400),
                    new(new DateTime(2021, 1, 6), 580, 490, 560, 440),
                    new(new DateTime(2021, 1, 7), 570, 560, 350, 340),
                    new(new DateTime(2021, 1, 8), 380, 350, 380, 330),
                    new(new DateTime(2021, 1, 9), 440, 380, 420, 350),
                    new(new DateTime(2021, 1, 10), 490, 420, 460, 400),
                    new(new DateTime(2021, 1, 11), 520, 460, 510, 460),
                    new(new DateTime(2021, 1, 12), 580, 510, 560, 500),
                    new(new DateTime(2021, 1, 13), 600, 560, 540, 510),
                    new(new DateTime(2021, 1, 14), 580, 540, 520, 500),
                    new(new DateTime(2021, 1, 15), 580, 520, 560, 520),
                    new(new DateTime(2021, 1, 16), 590, 560, 580, 520),
                    new(new DateTime(2021, 1, 17), 650, 580, 630, 550),
                    new(new DateTime(2021, 1, 18), 680, 630, 650, 600),
                    new(new DateTime(2021, 1, 19), 670, 650, 600, 570),
                    new(new DateTime(2021, 1, 20), 640, 600, 610, 560),
                    new(new DateTime(2021, 1, 21), 630, 610, 630, 590),
                }
            },
        };
        StockPriceChart.XAxes = new[] { new Axis { Position = AxisPosition.Start, ShowSeparatorLines = true, IsVisible = true } };
        StockPriceChart.YAxes = new[] { new Axis { Position = AxisPosition.End, ShowSeparatorLines = true, IsVisible = true } };
    }

    protected override void OnAppearing()
    {
        UpdateQuote(stock.Quote);
        stock.StockQuoteChanged += OnStockQuoteChanged;

        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        stock.StockQuoteChanged -= OnStockQuoteChanged;

        base.OnDisappearing();
    }

    void UpdateChart(YahooFinanceChart chart)
    {
        //var series = (CandlesticksSeries<FinancialPoint>)StockPriceChart.Series.FirstOrDefault();

        //series..Stroke = new SolidColorPaint(color, 1);
        //series.Fill = new LinearGradientPaint(color.WithAlpha(0x00), color.WithAlpha(0xaa), new SKPoint(0, 1), new SKPoint(0, 0)),
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

    void OnChartTabClicked(object sender, EventArgs e)
    {
        //if (Enum.TryParse(currentTab.StyleId, out YahooTimeRange range))
        //{
        //    var chart = await YahooFinanceClient.Default.GetChartAsync(stock.Quote, range);
        //    UpdateChart(chart);
        //}
    }
}