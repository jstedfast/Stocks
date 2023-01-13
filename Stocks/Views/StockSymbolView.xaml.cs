﻿namespace Stocks.Views;

public partial class StockSymbolView : ViewCell
{
    YahooFinanceQuote quote;

    public StockSymbolView(YahooFinanceQuote quote)
    {
        InitializeComponent();
        Update(quote);
    }

    public string Symbol { get { return quote.Symbol; } }

    public YahooFinanceQuote Quote { get { return quote; } }

    public void Update (YahooFinanceQuote quote)
    {
        this.quote = quote;

        NameLabel.Text = quote.Name;
        DescriptionLabel.Text = quote.Description;
        MarketPriceLabel.Text = quote.RegularMarketPrice.ToString("0,0.00");

        if (quote.RegularMarketChange >= 0.0)
        {
            MarketChangeLabelBorder.BackgroundColor = Color.Parse("Green");
            MarketChangeLabel.Text = string.Format("+{0:0.00}", quote.RegularMarketChange);
        }
        else
        {
            MarketChangeLabelBorder.BackgroundColor = Color.Parse("Red");
            MarketChangeLabel.Text = string.Format("{0:0.00}", quote.RegularMarketChange);
        }
    }
}