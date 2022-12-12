using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stocks
{
    public class YahooStockQuote
    {
        static readonly Dictionary<string, string> SymbolDescriptionOverrides;
        static readonly Dictionary<string, string> SymbolNameOverrides;
        readonly JObject quote;

        static YahooStockQuote ()
        {
            SymbolNameOverrides = new Dictionary<string, string>
            {
                { "^DJI", "Dow Jones" },
                { "^IXIC", "NASDAQ" },
                { "^GSPC", "S&P 500" }
            };

            SymbolDescriptionOverrides = new Dictionary<string, string>
            {
                { "^GSPC", "Standard & Poor's 500" }
            };
        }

        public YahooStockQuote (JObject quote)
        {
            this.quote = quote;
        }

        public string Symbol
        {
            get {
                if (quote.TryGetValue("symbol", out var token) && token.Type == JTokenType.String)
                    return (string) token;

                return string.Empty;
            }
        }

        public string Name
        {
            get
            {
                var symbol = Symbol;

                if (SymbolNameOverrides.TryGetValue(symbol, out var name))
                    return name;

                return symbol;
            }
        }

        public string Description
        {
            get
            {
                var symbol = Symbol;

                if (SymbolDescriptionOverrides.TryGetValue(symbol, out var description))
                    return description;

                if (quote.TryGetValue("shortName", out var token) && token.Type == JTokenType.String)
                    return (string) token;

                return string.Empty;
            }
        }

        public double MarketPrice
        {
            get
            {
                if (quote.TryGetValue("regularMarketPrice", out var token) && token.Type == JTokenType.Float)
                    return (double) token;

                return 0;
            }
        }

        public double MarketChange
        {
            get
            {
                if (quote.TryGetValue("regularMarketChange", out var token) && token.Type == JTokenType.Float)
                    return (double) token;

                return 0;
            }
        }
    }
}
