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
    readonly YahooStockQuote quote;

    public StockDetailsPage(YahooStockQuote quote)
    {
        InitializeComponent();
        this.quote = quote;

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
}