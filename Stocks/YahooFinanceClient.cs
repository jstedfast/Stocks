﻿using System.Net;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Stocks
{
    public class YahooFinanceClient : IDisposable
    {
        static readonly string[] TimeRanges = new string[] { "1d", "5d", "1mo", "3mo", "6mo", "1y", "2y", "5y", "10y", "ytd", "max" };
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public static readonly YahooFinanceClient Default = new YahooFinanceClient();
        
        HttpClient client;

        public YahooFinanceClient()
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            handler.UseCookies = true;

            client = new HttpClient(handler);
        }

        static string GenerateCSharpFromJson(string name, SortedDictionary<string, JTokenType> properties, Dictionary<string, int> counts = null, int arrayCount = 0)
        {
            var builder = new StringBuilder();

            builder.AppendLine("using Newtonsoft.Json;");
            builder.AppendLine();
            builder.AppendLine("namespace Stocks");
            builder.AppendLine("{");
            builder.AppendLine($"    public class {name}");
            builder.AppendLine("    {");

            foreach (var property in properties)
            {
                builder.Append($"        [JsonProperty(\"{property.Key}\"");
                if (property.Key == "symbol")
                    builder.Append(", Required = Required.Always");
                else if (counts != null && counts[property.Key] < arrayCount)
                    builder.Append(", NullValueHandling = NullValueHandling.Ignore");
                builder.AppendLine(")]");
                builder.Append("        public ");

                var propertyName = char.ToUpperInvariant(property.Key[0]) + property.Key.Substring(1, property.Key.Length - 1);

                switch (property.Value)
                {
                    case JTokenType.Boolean: builder.Append("bool"); break;
                    case JTokenType.String: builder.Append("string"); break;
                    case JTokenType.Integer: builder.Append("long"); break;
                    case JTokenType.Float: builder.Append("double"); break;
                    case JTokenType.Object: builder.Append($"{name}{propertyName}"); break;
                    case JTokenType.Array: builder.Append($"{name}{propertyName}[]"); break;
                    default: builder.Append(property.Value.ToString()); break;
                }
                if (counts != null && property.Value != JTokenType.String && counts[property.Key] < arrayCount)
                    builder.Append('?');
                builder.Append(' ');
                builder.Append(propertyName);
                builder.AppendLine(" { get; set; }");
                builder.AppendLine();
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");

            var text = builder.ToString();

            return text;
        }

        static string GenerateCSharpFromJson(string name, JObject item)
        {
            var properties = new SortedDictionary<string, JTokenType>();

            foreach (var property in item.Properties())
            {
                if (!properties.TryGetValue(property.Name, out var type) || type == JTokenType.Null)
                {
                    properties[property.Name] = property.Value.Type;

                    string typeName;

                    switch (property.Value.Type)
                    {
                        case JTokenType.Array:
                            typeName = name + char.ToUpperInvariant(property.Name[0]) + property.Name.Substring(1, property.Name.Length - 1);
                            GenerateCSharpFromJson(typeName, (JArray)property.Value);
                            break;
                        case JTokenType.Object:
                            typeName = name + char.ToUpperInvariant(property.Name[0]) + property.Name.Substring(1, property.Name.Length - 1);
                            GenerateCSharpFromJson(typeName, (JObject)property.Value);
                            break;
                    }
                }
            }

            return GenerateCSharpFromJson(name, properties);
        }

        static string GenerateCSharpFromJson(string name, JArray array)
        {
            var properties = new SortedDictionary<string, JTokenType>();
            var counts = new Dictionary<string, int>();

            for (int i = 0; i < array.Count; i++)
            {
                if (array[i].Type != JTokenType.Object)
                    continue;

                var result = (JObject)array[i];

                foreach (var property in result.Properties())
                {
                    if (!properties.TryGetValue(property.Name, out var type) || type == JTokenType.Null)
                    {
                        properties[property.Name] = property.Value.Type;

                        string typeName;

                        switch (property.Value.Type)
                        {
                            case JTokenType.Array:
                                typeName = name + char.ToUpperInvariant(property.Name[0]) + property.Name.Substring(1, property.Name.Length - 1);
                                GenerateCSharpFromJson(typeName, (JArray)property.Value);
                                break;
                            case JTokenType.Object:
                                typeName = name + char.ToUpperInvariant(property.Name[0]) + property.Name.Substring(1, property.Name.Length - 1);
                                GenerateCSharpFromJson(typeName, (JObject)property.Value);
                                break;
                        }
                    }

                    counts.TryGetValue(property.Name, out int count);
                    counts[property.Name] = count + 1;
                }
            }

            return GenerateCSharpFromJson(name, properties, counts, array.Count);
        }

        static void SetDefaultRequestHeaders (HttpRequestMessage request)
        {
            request.Headers.Add("Accept-Language", "en-US");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("User-Agent", "Mozilla/5.0");
        }

        public async Task<Dictionary<string, YahooFinanceQuote>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default)
        {
            var requestUri = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={string.Join(",", symbols.Select(Uri.EscapeDataString))}";

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                SetDefaultRequestHeaders(request);

                using (var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                        throw new YahooFinanceException(response.StatusCode, string.Empty, content);

                    var json = JObject.Parse(content).ToObject<YahooFinanceQuoteResponse>();

                    if (json.Data?.Error != null)
                        throw new YahooFinanceException(response.StatusCode, json.Data.Error.Code, json.Data.Error.Description);

                    var quotes = new Dictionary<string, YahooFinanceQuote>();

                    foreach (var result in json.Data.Result)
                        quotes.Add(result.Symbol, result);

                    return quotes;
                }
            }
        }

        // Note: I think this is the data used for generating the mini graph for each stock symbol on their repsective TableView rows in the iOS Stocks app.
        public async Task<Dictionary<string, YahooFinanceSpark>> GetSparksAsync(IEnumerable<string> symbols, YahooTimeRange range, CancellationToken cancellationToken = default)
        {
            string requestUri = $"https://query1.finance.yahoo.com/v7/finance/spark?symbols={string.Join(",", symbols.Select(Uri.EscapeDataString))}&range={TimeRanges[(int)range]}&interval=5m&indicators=close&includeTimestamps=false&includePrePost=false&corsDomain=finance.yahoo.com&.tsrc=finance";

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                SetDefaultRequestHeaders(request);

                using (var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                        throw new YahooFinanceException(response.StatusCode, string.Empty, content);

                    var json = JObject.Parse(content).ToObject<YahooFinanceSparkResponse>();

                    if (json.Data?.Error != null)
                        throw new YahooFinanceException(response.StatusCode, json.Data.Error.Code, json.Data.Error.Description);

                    var sparks = new Dictionary<string, YahooFinanceSpark>();

                    foreach (var result in json.Data.Result)
                        sparks.Add(result.Symbol, result.Response[0]);

                    return sparks;
                }
            }
        }

        static string GetChartInterval(YahooTimeRange range)
        {
            if (range < YahooTimeRange.OneMonth)
                return "1m"; // Yahoo! Finance only supports 1m intervals for up to 7 days worth of data.
            if (range < YahooTimeRange.ThreeMonth)
                return "5m"; // Yahoo! Finance only supports 5m intervals for up to 60 days worth of data.
            return "1d";
        }

        static void GetChartParameters(YahooFinanceQuote quote, YahooTimeRange range, out DateTime period1, out DateTime period2)
        {
            var firstTradeDate = UnixEpoch.AddMilliseconds(quote.FirstTradeDateMilliseconds);
            var now = DateTimeOffset.UtcNow.ToOffset(quote.GmtOffset);
            DateTimeOffset start, end, close;
            var tzOffset = quote.GmtOffset;

            close = new DateTimeOffset(now.Year, now.Month, now.Day, 16, 0, 0, tzOffset);

            switch (range)
            {
                case YahooTimeRange.OneDay:
                    start = new DateTimeOffset(now.Year, now.Month, now.Day, 10, 0, 0, tzOffset);
                    if (now < start)
                    {
                        // The market hasn't yet opened. Use yesterday's data.
                        start = start.AddDays(-1);
                        end = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        end = now >= close ? close : now;
                    }
                    break;
                case YahooTimeRange.FiveDay:
                    // Note: 5 business days ago is the same as 7 days ago.
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        end = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        end = close;
                    }
                    start = end.AddDays(-7);
                    break;
                case YahooTimeRange.OneMonth:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        end = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        end = close;
                    }
                    start = end.AddMonths(-1);
                    break;
                case YahooTimeRange.ThreeMonth:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        end = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        end = close;
                    }
                    start = end.AddMonths(-3);
                    break;
                case YahooTimeRange.SixMonth:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        end = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        end = close;
                    }
                    start = end.AddMonths(-6);
                    break;
                case YahooTimeRange.OneYear:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        end = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        end = close;
                    }
                    start = end.AddYears(-1);
                    break;
                case YahooTimeRange.TwoYear:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        end = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        end = close;
                    }
                    start = end.AddYears(-2);
                    break;
                case YahooTimeRange.FiveYear:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        end = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        end = close;
                    }
                    start = end.AddYears(-5);
                    break;
                case YahooTimeRange.TenYear:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        end = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        end = close;
                    }
                    start = end.AddYears(-10);
                    break;
                case YahooTimeRange.YearToDate:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        end = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        end = close;
                    }
                    start = new DateTimeOffset(now.Year, 1, 1, 10, 0, 0, tzOffset);
                    break;
                case YahooTimeRange.Max:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        end = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        end = close;
                    }
                    start = new DateTimeOffset(firstTradeDate);
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
        public async Task<YahooFinanceChart> GetChartAsync(YahooFinanceQuote quote, YahooTimeRange range, CancellationToken cancellationToken = default)
        {
            const string format = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?symbol={1}&period1={2}&period2={3}&useYfid=true&interval={4}&includePrePost=true&events=div|split|earn&lang=en-US&region=US&crumb=ibG1c1O0H9S&corsDomain=finance.yahoo.com";
            var interval = GetChartInterval(range);

            //period1: 1670878800
            //period2: 1671051600

            // Approx 7 PM EST on Dec 14, 2022
            //var period1 = UnixEpoch.AddSeconds(1670878800); => 12/12/2022 4:00:00 PM
            //var period2 = UnixEpoch.AddSeconds(1671051600); => 12/14/2022 4:00:00 PM

            GetChartParameters(quote, range, out var period1, out var period2);

            var requestUri = string.Format(format, quote.Symbol, quote.Symbol, SecondsSinceEpoch(period1), SecondsSinceEpoch(period2), interval);

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                SetDefaultRequestHeaders(request);

                using (var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                        throw new YahooFinanceException(response.StatusCode, string.Empty, content);

                    var json = JObject.Parse(content).ToObject<YahooFinanceChartResponse>();

                    if (json.Data?.Error != null)
                        throw new YahooFinanceException(response.StatusCode, json.Data.Error.Code, json.Data.Error.Description);

                    return json.Data.Result[0];
                }
            }
        }

        static long SecondsSinceEpoch(DateTime dt)
        {
            return (long)(dt - UnixEpoch).TotalSeconds;
        }

        public async Task<YahooStockData> GetHistoryAsync(string symbol, DateTime start, DateTime end, CancellationToken cancellationToken = default)
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
                    SetDefaultRequestHeaders(request);
                    request.Headers.Add("Referer", $"https://https://finance.yahoo.com/quote/{symbol}/history");
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

        public void Dispose()
        {
            if (client != null) {
                client.Dispose();
                client = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
