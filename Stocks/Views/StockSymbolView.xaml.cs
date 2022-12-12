using System.Globalization;

namespace Stocks.Views;

public partial class StockSymbolView : ContentView
{
    public StockSymbolView()
    {
        InitializeComponent();
    }

    public string Symbol
    {
        get; set;
    }

    public string Name
    {
        get { return NameLabel.Text; }
        set { NameLabel.Text = value; }
    }

    public string Description
    {
        get { return DescriptionLabel.Text; }
        set { DescriptionLabel.Text = value; }
    }

    public double MarketPrice
    {
        //get { return PriceLabel.Text; }
        set
        {
            MarketPriceLabel.Text = value.ToString("0,0.00");
        }
    }

    public double MarketChange
    {
        //get { return ChangeLabel.Text; }
        set
        {
            if (value >= 0.0)
            {
                MarketChangeLabelBorder.Background = Color.Parse("Green");
                MarketChangeLabel.Text = string.Format("+{0:0.00}", value);
            }
            else
            {
                MarketChangeLabelBorder.Background = Color.Parse("Red");
                MarketChangeLabel.Text = string.Format("{0:0.00}", value);
            }
        }
    }
}