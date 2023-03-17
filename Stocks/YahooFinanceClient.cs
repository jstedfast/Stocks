using System.Net;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Stocks
{
    public class YahooFinanceClient : IDisposable
    {
        static readonly string[] TimeIntervals = new string[] { "1m", "2m", "5m", "15m", "30m", "60m", "90m", "1h", "1d", "5d", "1wk", "1mo", "3mo" };
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

        // Note: This is the data used for generating the mini graph for each stock symbol on their respective TableView rows in the iOS Stocks app.
        public async Task<Dictionary<string, YahooFinanceSpark>> GetSparksAsync(IEnumerable<string> symbols, YahooFinanceChartTimeRange range, YahooFinanceTimeInterval interval, CancellationToken cancellationToken = default)
        {
            const string format = "https://query1.finance.yahoo.com/v7/finance/spark?symbols={0}&range={1}&interval={2}&indicators=close&includeTimestamps=false&includePrePost=false&corsDomain=finance.yahoo.com&.tsrc=finance";
            var requestUri = string.Format(format, string.Join(",", symbols.Select(Uri.EscapeDataString)), TimeRanges[(int)range], TimeIntervals[(int)interval]);

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

        public static YahooFinanceTimeInterval GetSmallestAllowedTimeInterval(DateTimeOffset startDate)
        {
            var now = DateTimeOffset.UtcNow.ToOffset(startDate.Offset);
            var timespan = now - startDate;

            if (timespan.TotalDays < 7)
                return YahooFinanceTimeInterval.OneMinute;
            if (timespan.TotalDays < 60)
                return YahooFinanceTimeInterval.TwoMinutes;
            if (timespan.TotalDays < 730)
                return YahooFinanceTimeInterval.SixtyMinutes;
            return YahooFinanceTimeInterval.OneDay;
        }

        static YahooFinanceTimeInterval GetIdealTimeInterval(DateTimeOffset startDate, DateTimeOffset now)
        {
            // valid intervals: 1m (< 7 days data), 2m, 5m, 15m, 30m (< 60 days data), 60m (< 730 days), 90m (< 60 days data), 1h (< 130 days), 1d, 5d, 1wk, 1mo, 3mo
            var timespan = now - startDate;

            if (timespan.TotalDays < 1)
                return YahooFinanceTimeInterval.OneMinute;

            if (timespan.TotalDays < 7 + 1) // 1 week
                return YahooFinanceTimeInterval.TwoMinutes;

            if (timespan.TotalDays < 31 + 1) // 1 month
                return YahooFinanceTimeInterval.FiveMinutes;

            if (timespan.TotalDays < 60) // 2 months
                return YahooFinanceTimeInterval.ThirtyMinutes;

            if (timespan.TotalDays < 130) // max allowed for 1h
                return YahooFinanceTimeInterval.OneHour;

            if (timespan.TotalDays < 730) // 2 years
                return YahooFinanceTimeInterval.SixtyMinutes;

            if (timespan.TotalDays < 2923) // 8 years
                return YahooFinanceTimeInterval.OneDay;

            if (timespan.TotalDays < 8767) // 24 years
                return YahooFinanceTimeInterval.FiveDays;

            if (timespan.TotalDays < 11689) // 32 years
                return YahooFinanceTimeInterval.OneWeek;

            if (timespan.TotalDays < 46756) // 128 years
                return YahooFinanceTimeInterval.OneMonth;

            return YahooFinanceTimeInterval.ThreeMonths;
        }

        static bool IsAllowedTimeInterval(DateTimeOffset startDate, YahooFinanceTimeInterval interval)
        {
            // valid intervals: 1m (< 7 days data), 2m, 5m, 15m, 30m (< 60 days data), 60m (< 730 days), 90m (< 60 days data), 1h (< 130 days), 1d, 5d, 1wk, 1mo, 3mo
            var now = DateTimeOffset.UtcNow.ToOffset(startDate.Offset);
            var timespan = now - startDate;

            switch (interval)
            {
                case YahooFinanceTimeInterval.OneMinute:
                case YahooFinanceTimeInterval.TwoMinutes:
                    return timespan.Days <= 7;
                case YahooFinanceTimeInterval.FiveMinutes:
                case YahooFinanceTimeInterval.FifteenMinutes:
                    return timespan.Days <= 30;
                case YahooFinanceTimeInterval.ThirtyMinutes:
                    return timespan.Days <= 60;
                case YahooFinanceTimeInterval.SixtyMinutes:
                    return timespan.Days <= 730;
                case YahooFinanceTimeInterval.NinetyMinutes:
                    return timespan.Days <= 60;
                case YahooFinanceTimeInterval.OneHour:
                    return timespan.Days <= 130;
                case YahooFinanceTimeInterval.OneDay:
                case YahooFinanceTimeInterval.FiveDays:
                case YahooFinanceTimeInterval.OneWeek:
                case YahooFinanceTimeInterval.OneMonth:
                case YahooFinanceTimeInterval.ThreeMonths:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(interval));
            }
        }

        static void GetChartParameters(YahooFinanceQuote quote, YahooFinanceChartTimeRange range, out DateTimeOffset start, out DateTimeOffset end, out YahooFinanceTimeInterval interval)
        {
            if (quote == null)
                throw new ArgumentNullException(nameof(quote));

            var firstTradeDate = UnixEpoch.AddMilliseconds(quote.FirstTradeDateMilliseconds);
            var now = DateTimeOffset.UtcNow.ToOffset(quote.GmtOffset);
            var tzOffset = quote.GmtOffset;
            DateTimeOffset close;

            close = new DateTimeOffset(now.Year, now.Month, now.Day, 16, 0, 0, tzOffset);

            switch (range)
            {
                case YahooFinanceChartTimeRange.OneDay:
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
                case YahooFinanceChartTimeRange.FiveDay:
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
                case YahooFinanceChartTimeRange.OneMonth:
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
                case YahooFinanceChartTimeRange.ThreeMonth:
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
                case YahooFinanceChartTimeRange.SixMonth:
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
                case YahooFinanceChartTimeRange.OneYear:
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
                case YahooFinanceChartTimeRange.TwoYear:
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
                case YahooFinanceChartTimeRange.FiveYear:
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
                case YahooFinanceChartTimeRange.TenYear:
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
                case YahooFinanceChartTimeRange.YearToDate:
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
                case YahooFinanceChartTimeRange.Max:
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

            if (start < firstTradeDate)
                start = firstTradeDate;

            interval = GetIdealTimeInterval(start, now);
        }

        static long SecondsSinceEpoch(DateTimeOffset dateTimeOffset)
        {
            return (long)(dateTimeOffset.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }

        // Note: This is the data used for generating the graph for a stock symbol's detailed view in the iOS Stocks app.
        public Task<YahooFinanceChart> GetChartAsync(YahooFinanceQuote quote, YahooFinanceChartTimeRange range, CancellationToken cancellationToken = default)
        {
            //period1: 1670878800
            //period2: 1671051600

            // Approx 7 PM EST on Dec 14, 2022
            //var period1 = UnixEpoch.AddSeconds(1670878800); => 12/12/2022 4:00:00 PM
            //var period2 = UnixEpoch.AddSeconds(1671051600); => 12/14/2022 4:00:00 PM

            GetChartParameters(quote, range, out var period1, out var period2, out var interval);

            return GetChartAsync(quote, period1, period2, interval, cancellationToken);
        }

        static void ValidateArguments(YahooFinanceQuote quote, DateTimeOffset period1, DateTimeOffset period2, YahooFinanceTimeInterval interval)
        {
            if (quote == null)
                throw new ArgumentNullException(nameof(quote));

            var firstTradeDate = UnixEpoch.AddMilliseconds(quote.FirstTradeDateMilliseconds);
            var now = DateTimeOffset.UtcNow.ToOffset(quote.GmtOffset);

            if (period1 < firstTradeDate || period1 >= now)
                throw new ArgumentOutOfRangeException(nameof(period1));

            if (period2 <= period1 || period2 >= now)
                throw new ArgumentOutOfRangeException(nameof(period2));

            if (!IsAllowedTimeInterval(period1, interval))
                throw new ArgumentOutOfRangeException(nameof(interval), $"interval was {TimeIntervals[(int)interval]}");
        }

        // Note: This is the data used for generating the graph for a stock symbol's detailed view in the iOS Stocks app.
        public async Task<YahooFinanceChart> GetChartAsync(YahooFinanceQuote quote, DateTimeOffset period1, DateTimeOffset period2, YahooFinanceTimeInterval interval, CancellationToken cancellationToken = default)
        {
            ValidateArguments(quote, period1, period2, interval);

            const string format = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?symbol={1}&period1={2}&period2={3}&useYfid=true&interval={4}&includePrePost=true&events=div|split|earn&lang=en-US&region=US&crumb=ibG1c1O0H9S&corsDomain=finance.yahoo.com";
            var requestUri = string.Format(format, quote.Symbol, quote.Symbol, SecondsSinceEpoch(period1), SecondsSinceEpoch(period2), TimeIntervals[(int)interval]);

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

        public async Task<YahooStockData> GetHistoryAsync(string symbol, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
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
