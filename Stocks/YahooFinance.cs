using System.Net;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Stocks
{
    public static class YahooFinance
    {
        static readonly string[] TimeRanges = new string[] { "1d", "5d", "1mo", "3mo", "6mo", "1y", "2y", "5y", "10y", "ytd", "max" };
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        static HttpClient client;

        static YahooFinance()
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            handler.UseCookies = true;

            client = new HttpClient(handler);
        }

        static void GenerateCSharpFromJson(JArray results)
        {
            var properties = new SortedDictionary<string, JTokenType>();
            var counts = new Dictionary<string, int>();

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Type != JTokenType.Object)
                    continue;

                var result = (JObject)results[i];

                foreach (var property in result.Properties())
                {
                    if (!properties.TryGetValue(property.Name, out var type) || type == JTokenType.Null)
                        properties[property.Name] = property.Value.Type;

                    counts.TryGetValue(property.Name, out int count);
                    counts[property.Name] = count + 1;
                }
            }

            var sb = new StringBuilder();
            foreach (var property in properties)
            {
                sb.Append($"[JsonProperty(\"{property.Key}\"");
                if (property.Key == "symbol")
                    sb.Append(", Required = Required.Always");
                else if (counts[property.Key] < results.Count)
                    sb.Append(", NullValueHandling = NullValueHandling.Ignore");
                sb.AppendLine(")]");
                sb.Append("public ");
                switch (property.Value)
                {
                    case JTokenType.Boolean: sb.Append("bool"); break;
                    case JTokenType.String: sb.Append("string"); break;
                    case JTokenType.Integer: sb.Append("long"); break;
                    case JTokenType.Float: sb.Append("double"); break;
                    default: sb.Append(property.Value.ToString()); break;
                }
                if (property.Value != JTokenType.String && counts[property.Key] < results.Count)
                    sb.Append('?');
                sb.Append(' ');
                sb.Append(char.ToUpperInvariant(property.Key[0]));
                sb.Append(property.Key, 1, property.Key.Length - 1);
                sb.AppendLine(" { get; set; }");
                sb.AppendLine();
            }

            var text = sb.ToString();
        }

        public static async Task<List<YahooStockQuote>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default)
        {
            var requestUri = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={string.Join(",", symbols)}";
            string content;

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                request.Headers.Add("Accept-Language", "en-US");
                request.Headers.Add("Connection", "keep-alive");

                using (var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"HTTP response failure for '{requestUri}': {response.StatusCode}");
                    }
                }
            }

            var json = JObject.Parse(content);
            var list = new List<YahooStockQuote>();

            if (json.TryGetValue("quoteResponse", out var token) && token.Type == JTokenType.Object)
            {
                var quoteResponse = (JObject) token;

                if (quoteResponse.TryGetValue("result", out token) && token.Type == JTokenType.Array)
                {
                    var results = (JArray) token;

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].Type != JTokenType.Object)
                            continue;

                        var result = (JObject)results[i];

                        var quote = result.ToObject<YahooStockQuote>();

                        list.Add(quote);
                    }

                    //GenerateCSharpFromJson(results);
                }
            }

            return list;
        }

        // Note: I think this is the data used for generating the mini graph for each stock symbol on their repsective TableView rows in the iOS Stocks app.
        public static async Task GetSparkAsync(IEnumerable<string> symbols, YahooTimeRange range, CancellationToken cancellationToken = default)
        {
            string requestUri = $"https://query1.finance.yahoo.com/v7/finance/spark?symbols={string.Join(",", symbols)}&range={TimeRanges[(int)range]}&interval=5m&indicators=close&includeTimestamps=false&includePrePost=false&corsDomain=finance.yahoo.com&.tsrc=finance";
            string content;

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                request.Headers.Add("Accept-Language", "en-US");
                request.Headers.Add("Connection", "keep-alive");

                using (var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"HTTP response failure for '{requestUri}': {response.StatusCode}");
                    }
                }
            }

            var json = JObject.Parse(content);
            var list = new List<JObject>();

            if (json.TryGetValue("spark", out var token) && token.Type == JTokenType.Object)
            {
                var quoteResponse = (JObject)token;

                if (quoteResponse.TryGetValue("result", out token) && token.Type == JTokenType.Array)
                {
                    var results = (JArray)token;

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].Type != JTokenType.Object)
                            continue;

                        var result = (JObject)results[i];

                        list.Add(result);
                    }

                    GenerateCSharpFromJson(results);
                }
            }
        }

        static string GetChartInterval(YahooTimeRange range)
        {
            if (range == YahooTimeRange.OneDay)
                return "1m";
            if (range == YahooTimeRange.FiveDay)
                return "5m";
            return "1d";
        }

        static void GetChartParameters(YahooStockQuote quote, YahooTimeRange range, out DateTime period1, out DateTime period2)
        {
            DateTime firstTradeDate = UnixEpoch.AddMilliseconds(quote.FirstTradeDateMilliseconds);
            var tzOffset = quote.GmtOffset;
            DateTime now = DateTime.Now;

            var start = new DateTimeOffset(now.Year, now.Month, now.Day, 10, 0, 0, tzOffset);
            if (start > now)
                start = start.AddDays(-1);

            var end = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, tzOffset);

            switch (range)
            {
                case YahooTimeRange.OneDay:
                    break;
                case YahooTimeRange.FiveDay:
                    // Note: 5 business days ago is the same as 7 days ago.
                    start = start.AddDays(-7);
                    break;
                case YahooTimeRange.OneMonth:
                    start = start.AddMonths(-1);
                    break;
                case YahooTimeRange.ThreeMonth:
                    start = start.AddMonths(-3);
                    break;
                case YahooTimeRange.SixMonth:
                    start = start.AddMonths(-6);
                    break;
                case YahooTimeRange.OneYear:
                    start = start.AddYears(-1);
                    break;
                case YahooTimeRange.TwoYear:
                    start = start.AddYears(-2);
                    break;
                case YahooTimeRange.FiveYear:
                    start = start.AddYears(-5);
                    break;
                case YahooTimeRange.TenYear:
                    start = start.AddYears(-10);
                    break;
                case YahooTimeRange.YearToDate:
                    start = new DateTimeOffset(start.Year, 1, 1, 10, 0, 0, tzOffset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(range));
            }

            // convert to UTC date/time
            period1 = start.ToUniversalTime().DateTime;
            period2 = end.ToUniversalTime().DateTime;

            if (period1 < firstTradeDate)
                period1 = firstTradeDate;

            // TODO: Do we need to worry about the GMT offset at all?
        }

        // Note: Pretty sure this is the data used to generate the multi-tab chart in the Details View.
        public static async Task GetChartAsync(YahooStockQuote quote, YahooTimeRange range, CancellationToken cancellationToken = default)
        {
            const string format = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?symbol={1}&period1={2}&period2={3}&useYfid=true&interval={4}&includePrePost=true&events=div|split|earn&lang=en-US&region=US&crumb=ibG1c1O0H9S&corsDomain=finance.yahoo.com";
            var interval = GetChartInterval(range);

            //period1: 1670878800
            //period2: 1671051600

            // Approx 7 PM EST on Dec 14, 2022
            //var period1 = UnixEpoch.AddSeconds(1670878800);
            //var period2 = UnixEpoch.AddSeconds(1671051600);

            GetChartParameters(quote, range, out var period1, out var period2);

            var requestUri = string.Format(format, quote.Symbol, quote.Symbol, SecondsSinceEpoch(period1), SecondsSinceEpoch(period2), interval);
            string content;

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                request.Headers.Add("Accept-Language", "en-US");
                request.Headers.Add("Connection", "keep-alive");

                using (var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"HTTP response failure for '{requestUri}': {response.StatusCode}");
                    }
                }
            }

            var json = JObject.Parse(content);
            var list = new List<JObject>();

            if (json.TryGetValue("chart", out var token) && token.Type == JTokenType.Object)
            {
                var quoteResponse = (JObject)token;

                if (quoteResponse.TryGetValue("result", out token) && token.Type == JTokenType.Array)
                {
                    var results = (JArray)token;

                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i].Type != JTokenType.Object)
                            continue;

                        var result = (JObject)results[i];

                        list.Add(result);
                    }

                    //GenerateCSharpFromJson(results);
                }
            }
        }

        static long SecondsSinceEpoch(DateTime dt)
        {
            return (long)(dt - UnixEpoch).TotalSeconds;
        }

        public static async Task<YahooStockData> GetHistoryAsync(string symbol, DateTime start, DateTime end, CancellationToken cancellationToken = default)
        {
            const string format = "https://query1.finance.yahoo.com/v7/finance/download/{0}?period1={1}&period2={2}&interval=1d&events=history&includeAdjustedClose=true";
            var requestUri = string.Format(format, symbol, SecondsSinceEpoch(start), SecondsSinceEpoch(end));
            int retries = 0;

            // GET https://query1.finance.yahoo.com/v7/finance/download/AAPL?period1=1622906679&period2=1654442679&interval=1d&events=history&includeAdjustedClose=true HTTP/1.1
            // Host: query1.finance.yahoo.com
            // User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:100.0) Gecko/20100101 Firefox/100.0
            // Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8
            // Accept-Language: en-US,en;q=0.5
            // Accept-Encoding: gzip, deflate, br
            // Referer: https://finance.yahoo.com/quote/AAPL/history
            // Connection: keep-alive
            // Cookie: A1=d=AQABBNC5m2ICEChbmhRVuTXYEHia15UotzkFEgEBAQELnWKlYgAAAAAA_eMAAA&S=AQAAAliad9I6hSWkcEeAj9baCAQ; A3=d=AQABBNC5m2ICEChbmhRVuTXYEHia15UotzkFEgEBAQELnWKlYgAAAAAA_eMAAA&S=AQAAAliad9I6hSWkcEeAj9baCAQ; A1S=d=AQABBNC5m2ICEChbmhRVuTXYEHia15UotzkFEgEBAQELnWKlYgAAAAAA_eMAAA&S=AQAAAliad9I6hSWkcEeAj9baCAQ&j=US; GUC=AQEBAQFinQtipUIgIQS_; cmp=t=1654442040&j=0&u=1---; thamba=1; PRF=t%3DAAPL%252BBTC-USD%252BBTC
            // Upgrade-Insecure-Requests: 1
            // Sec-Fetch-Dest: document
            // Sec-Fetch-Mode: navigate
            // Sec-Fetch-Site: same-site
            // Sec-Fetch-User: ?1
            // Sec-GPC: 1

            do
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    request.Headers.Add("Accept-Language", "en-US");
                    request.Headers.Add("Referer", $"https://https://finance.yahoo.com/quote/{symbol}/history");
                    request.Headers.Add("Connection", "keep-alive");
                    //request.Headers.Add ("Cookie", "A1=d=AQABBNC5m2ICEChbmhRVuTXYEHia15UotzkFEgEBAQELnWKlYgAAAAAA_eMAAA&S=AQAAAliad9I6hSWkcEeAj9baCAQ; A3=d=AQABBNC5m2ICEChbmhRVuTXYEHia15UotzkFEgEBAQELnWKlYgAAAAAA_eMAAA&S=AQAAAliad9I6hSWkcEeAj9baCAQ; A1S=d=AQABBNC5m2ICEChbmhRVuTXYEHia15UotzkFEgEBAQELnWKlYgAAAAAA_eMAAA&S=AQAAAliad9I6hSWkcEeAj9baCAQ&j=US; GUC=AQEBAQFinQtipUIgIQS_; cmp=t=1654442040&j=0&u=1---");

                    using (var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                        {
                            if (response.IsSuccessStatusCode)
                                return await YahooStockData.LoadAsync(stream, cancellationToken).ConfigureAwait(false);

                            if (response.StatusCode == HttpStatusCode.Unauthorized && retries < 5)
                            {
                                await Task.Delay(1000).ConfigureAwait(false);
                                retries++;
                                continue;
                            }

                            using (var reader = new StreamReader(stream))
                            {
                                var text = reader.ReadToEnd();
                                var json = JObject.Parse(text);

                                var code = json.SelectToken("finance.error.code").ToString();
                                var description = json.SelectToken("finance.error.description").ToString();

                                throw new Exception($"{code}: {description}");
                            }
                        }
                    }
                }
            } while (true);
        }
    }
}
