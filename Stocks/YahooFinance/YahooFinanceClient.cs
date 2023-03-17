using System.Net;

using Newtonsoft.Json.Linq;

namespace Stocks.YahooFinance
{
    /// <summary>
    /// A Yahoo! Finance client that can be used to obtain stock trade data.
    /// </summary>
    /// <remarks>
    /// A Yahoo! Finance client that can be used to obtain stock trade data.
    /// </remarks>
    public class YahooFinanceClient : IDisposable
    {
        static readonly string[] TimeIntervals = new string[] { "1m", "2m", "5m", "15m", "30m", "60m", "90m", "1h", "1d", "5d", "1wk", "1mo", "3mo" };
        static readonly string[] TimeRanges = new string[] { "1d", "5d", "1mo", "3mo", "6mo", "1y", "2y", "5y", "10y", "ytd", "max" };
        static readonly string[] Indicators = new string[] { "open", "close", "high", "low" };
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        const string crumb = "ibG1c1O0H9S";

        public static readonly YahooFinanceClient Default = new YahooFinanceClient();

        HttpClient client;

        static HttpClientHandler CreateDefaultHandler()
        {
            return new HttpClientHandler()
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };
        }

        /// <summary>
        /// Create a new instance of the Yahoo! Finance client.
        /// </summary>
        /// <remarks>
        /// Creates a new instance of the Yahoo! Finance client.
        /// </remarks>
        public YahooFinanceClient() : this(CreateDefaultHandler())
        {
        }

        /// <summary>
        /// Create a new instance of the Yahoo! Finance client.
        /// </summary>
        /// <remarks>
        /// Creates a new instance of the Yahoo! Finance client.
        /// </remarks>
        /// <param name="handler">The HTTP client handler.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler"/> is <c>null</c>.
        /// </exception>
        public YahooFinanceClient(HttpClientHandler handler)
        {
            client = new HttpClient(handler ?? throw new ArgumentNullException(nameof(handler)));
        }

        ~YahooFinanceClient()
        {
            Dispose(false);
        }

#if GENERATE_JSON_CLASSES

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

#endif // GENERATE_JSON_CLASSES

        static void SetDefaultRequestHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("Accept-Language", "en-US");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("User-Agent", "Mozilla/5.0");
        }

        static long SecondsSinceEpoch(DateTimeOffset dateTimeOffset)
        {
            return (long)(dateTimeOffset.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }

        #region Argument Validation

        static void ValidateSymbol(string symbol)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            if (symbol.Length == 0)
                throw new ArgumentException("The symbol name cannot be empty.", nameof(symbol));
        }

        static IList<string> ValidateSymbols(IEnumerable<string> symbols)
        {
            if (symbols == null)
                throw new ArgumentNullException(nameof(symbols));

            var list = symbols as IList<string> ?? symbols.ToArray();

            if (list.Count == 0)
                throw new ArgumentException("The list of symbols cannot be empty.", nameof(symbols));

            foreach (var symbol in list)
            {
                if (string.IsNullOrEmpty(symbol))
                    throw new ArgumentException("One or more symbols is null or empty.", nameof(symbols));
            }

            return list;
        }

        static void ValidateQuote(YahooFinanceQuote quote)
        {
            if (quote == null)
                throw new ArgumentNullException(nameof(quote));
        }

        static void ValidateIndicator(YahooFinanceIndicator indicator)
        {
            if (indicator < YahooFinanceIndicator.Open || indicator > YahooFinanceIndicator.Low)
                throw new ArgumentOutOfRangeException(nameof(indicator));
        }

        static void ValidateTimeRange(YahooFinanceTimeRange range)
        {
            if (range < YahooFinanceTimeRange.OneDay || range > YahooFinanceTimeRange.Max)
                throw new ArgumentOutOfRangeException(nameof(range));
        }

        static bool IsValidTimeInterval(YahooFinanceTimeRange range, YahooFinanceTimeInterval interval)
        {
            switch (range)
            {
                case YahooFinanceTimeRange.OneDay:
                    return interval < YahooFinanceTimeInterval.OneDay;
                case YahooFinanceTimeRange.FiveDay:
                    return interval < YahooFinanceTimeInterval.FiveDays;
                case YahooFinanceTimeRange.OneMonth:
                    return interval < YahooFinanceTimeInterval.OneMonth;
                case YahooFinanceTimeRange.ThreeMonth:
                    return interval < YahooFinanceTimeInterval.ThreeMonths;
                case YahooFinanceTimeRange.SixMonth:
                case YahooFinanceTimeRange.OneYear:
                case YahooFinanceTimeRange.TwoYear:
                case YahooFinanceTimeRange.FiveYear:
                case YahooFinanceTimeRange.TenYear:
                case YahooFinanceTimeRange.YearToDate:
                case YahooFinanceTimeRange.Max:
                    return true;
                default:
                    return false;
            }
        }

        static bool IsValidTimeInterval(DateTimeOffset startTime, DateTimeOffset endTime, DateTimeOffset now, YahooFinanceTimeInterval interval)
        {
            var timespan = endTime - startTime;
            var ago = now - startTime;

            switch (interval)
            {
                case YahooFinanceTimeInterval.OneMinute:      //  1m data not available for startTime=1676577600 and endTime=1678996800. Only 7 days worth of 1m granularity data are allowed to be fetched per request.
                    return timespan.Days <= 7;
                case YahooFinanceTimeInterval.TwoMinutes:     //  2m data not available for startTime=1671220800 and endTime=1678996800. The requested range must be within the last 60 days.
                case YahooFinanceTimeInterval.FiveMinutes:    //  5m data not available for startTime=1671220800 and endTime=1678996800. The requested range must be within the last 60 days.
                case YahooFinanceTimeInterval.FifteenMinutes: // 15m data not available for startTime=1671220800 and endTime=1678996800. The requested range must be within the last 60 days.
                case YahooFinanceTimeInterval.ThirtyMinutes:  // 30m data not available for startTime=1671220800 and endTime=1678996800. The requested range must be within the last 60 days.
                case YahooFinanceTimeInterval.NinetyMinutes:  // 90m data not available for startTime=1671220800 and endTime=1678996800. The requested range must be within the last 60 days.
                    return ago.Days <= 60;
                case YahooFinanceTimeInterval.SixtyMinutes:   // 60m data not available for startTime=1615924800 and endTime=1678996800. The requested range must be within the last 730 days.
                case YahooFinanceTimeInterval.OneHour:        //  1h data not available for startTime=1615924800 and endTime=1678996800. The requested range must be within the last 730 days.
                    return ago.Days <= 730;
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

        static IList<string> ValidateArguments(IEnumerable<string> symbols, YahooFinanceIndicator indicator, YahooFinanceTimeRange range, YahooFinanceTimeInterval interval)
        {
            var validSymbols = ValidateSymbols(symbols);

            ValidateIndicator(indicator);
            ValidateTimeRange(range);

            if (IsValidTimeInterval(range, interval))
                throw new ArgumentOutOfRangeException(nameof(interval));

            return validSymbols;
        }

        static void ValidateArguments(YahooFinanceQuote quote, DateTimeOffset startTime, DateTimeOffset endTime, YahooFinanceTimeInterval interval)
        {
            ValidateQuote(quote);

            var firstTradeDate = UnixEpoch.AddMilliseconds(quote.FirstTradeDateMilliseconds);
            var now = DateTimeOffset.UtcNow.ToOffset(quote.GmtOffset);

            if (startTime < firstTradeDate || startTime >= now)
                throw new ArgumentOutOfRangeException(nameof(startTime));

            if (endTime <= startTime || endTime >= now)
                throw new ArgumentOutOfRangeException(nameof(endTime));

            if (!IsValidTimeInterval(startTime, endTime, now, interval))
                throw new ArgumentOutOfRangeException(nameof(interval));
        }

        #endregion Argument Validation

        /// <summary>
        /// Get the latest stock trade quote for the specified stock symbol.
        /// </summary>
        /// <remarks>
        /// Gets the latest stock trade quote for the specified stock symbol.
        /// </remarks>
        /// <param name="symbol">The stock symbol.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A stock quotes.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="symbol"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="symbol"/> is empty.
        /// </exception>
        /// <exception cref="YahooFinanceException">
        /// The Yahoo! Finance server returned an error.
        /// </exception>
        public async Task<YahooFinanceQuote> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default)
        {
            ValidateSymbol(symbol);

            var requestUri = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={Uri.EscapeDataString(symbol)}";

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

                    return json.Data.Result[0];
                }
            }
        }

        /// <summary>
        /// Get the latest stock trade quote for each of the specified stock symbols.
        /// </summary>
        /// <remarks>
        /// Gets the latest stock trade quote for each of the specified stock symbols.
        /// </remarks>
        /// <param name="symbols">The stock symbols.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A dictionary of stock quotes that are indexed by their stock symbols.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="symbols"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="symbols"/> is empty.
        /// </exception>
        /// <exception cref="YahooFinanceException">
        /// The Yahoo! Finance server returned an error.
        /// </exception>
        public async Task<Dictionary<string, YahooFinanceQuote>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default)
        {
            var escapedSymbols = string.Join(",", ValidateSymbols(symbols).Select(Uri.EscapeDataString));
            var requestUri = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={escapedSymbols}";

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

        /// <summary>
        /// Get spark data for each of the specified stock symbols.
        /// </summary>
        /// <remarks>
        /// <para>Gets the latest trading period of closing data at a 1 minute interval for each of the specified stock symbols.</para>
        /// <para><see cref=""/></para>
        /// <para>A <see cref="YahooFinanceSpark"/> can be used to graph the fluctuation of stock prices over a period of time at a given interval.</para>
        /// </remarks>
        /// <param name="symbols">The stock symbols.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A dictionary of sparks that are indexed by their stock symbols.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="symbols"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="symbols"/> is empty.
        /// </exception>
        /// <exception cref="YahooFinanceException">
        /// The Yahoo! Finance server returned an error.
        /// </exception>
        public Task<Dictionary<string, YahooFinanceSpark>> GetSparksAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default)
        {
            return GetSparksAsync(symbols, YahooFinanceIndicator.Close, YahooFinanceTimeRange.OneDay, YahooFinanceTimeInterval.OneMinute, cancellationToken);
        }

        /// <summary>
        /// Get spark data for each of the specified stock symbols.
        /// </summary>
        /// <remarks>
        /// <para>Gets spark data for each of the specified stock symbols.</para>
        /// <para>A <see cref="YahooFinanceSpark"/> can be used to graph the fluctuation of stock prices over a period of time at a given interval.</para>
        /// </remarks>
        /// <param name="symbols">The stock symbols.</param>
        /// <param name="indicator">The stock price indicator to use.</param>
        /// <param name="range">The time range.</param>
        /// <param name="interval">The data interval.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A dictionary of sparks that are indexed by their stock symbols.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="symbols"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="symbols"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="indicator"/> is not a valid enum value.</para>
        /// <para>-or-</para>
        /// <para><paramref name="range"/> is not a valid enum value.</para>
        /// <para>-or-</para>
        /// <para><paramref name="interval"/> is not valid for the specified <paramref name="range"/>.</para>
        /// </exception>
        /// <exception cref="YahooFinanceException">
        /// The Yahoo! Finance server returned an error.
        /// </exception>
        public async Task<Dictionary<string, YahooFinanceSpark>> GetSparksAsync(IEnumerable<string> symbols, YahooFinanceIndicator indicator, YahooFinanceTimeRange range, YahooFinanceTimeInterval interval, CancellationToken cancellationToken = default)
        {
            const string format = "https://query1.finance.yahoo.com/v7/finance/spark?symbols={0}&range={1}&interval={2}&indicators={3}&includeTimestamps=false&includePrePost=false&corsDomain=finance.yahoo.com&.tsrc=finance";
            var escapedSymbols = string.Join(",", ValidateArguments(symbols, indicator, range, interval).Select(Uri.EscapeDataString));
            var requestUri = string.Format(format, escapedSymbols, TimeRanges[(int)range], TimeIntervals[(int)interval], Indicators[(int)indicator]);

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

#if false
        static YahooFinanceTimeInterval GetShortestAllowedTimeInterval(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var now = DateTimeOffset.UtcNow.ToOffset(startTime.Offset);
            var ago = now - startTime;

            if (ago.Days <= 7)
                return YahooFinanceTimeInterval.OneMinute;
            if (ago.TotalDays < 60)
                return YahooFinanceTimeInterval.TwoMinutes;
            if (ago.TotalDays < 730)
                return YahooFinanceTimeInterval.OneHour;
            return YahooFinanceTimeInterval.OneDay;
        }
#endif

        static YahooFinanceTimeInterval GetIdealTimeInterval(DateTimeOffset startTime, DateTimeOffset endTime, DateTimeOffset now)
        {
            var timespan = endTime - startTime;
            var ago = now - startTime;

            if (timespan.Days <= 1) // 1 day
                return YahooFinanceTimeInterval.OneMinute;

            if (ago.Days <= 7) // 1 week
                return YahooFinanceTimeInterval.TwoMinutes;

            if (ago.Days < 31) // ~1 month; max allowed for 5m intervals
                return YahooFinanceTimeInterval.FiveMinutes;

            if (ago.Days < 60) // ~2 months; max allowed for 30m intervals
                return YahooFinanceTimeInterval.ThirtyMinutes;

            if (ago.Days < 730) // ~2 years; max allowed for 60m/1h intervals
                return YahooFinanceTimeInterval.OneHour;

            if (ago.Days <= 2923) // 8 years
                return YahooFinanceTimeInterval.OneDay;

            if (ago.Days <= 8767) // 24 years
                return YahooFinanceTimeInterval.FiveDays;

            if (ago.Days <= 11689) // 32 years
                return YahooFinanceTimeInterval.OneWeek;

            if (ago.Days <= 46756) // 128 years
                return YahooFinanceTimeInterval.OneMonth;

            return YahooFinanceTimeInterval.ThreeMonths;
        }

        static void GetDefaultChartParameters(YahooFinanceQuote quote, YahooFinanceTimeRange range, out DateTimeOffset startTime, out DateTimeOffset endTime, out YahooFinanceTimeInterval interval)
        {
            ValidateQuote(quote);

            var firstTradeDate = UnixEpoch.AddMilliseconds(quote.FirstTradeDateMilliseconds);
            var now = DateTimeOffset.UtcNow.ToOffset(quote.GmtOffset);
            var tzOffset = quote.GmtOffset;
            DateTimeOffset close;

            close = new DateTimeOffset(now.Year, now.Month, now.Day, 16, 0, 0, tzOffset);

            switch (range)
            {
                case YahooFinanceTimeRange.OneDay:
                    startTime = new DateTimeOffset(now.Year, now.Month, now.Day, 10, 0, 0, tzOffset);
                    if (now < startTime)
                    {
                        // The market hasn't yet opened. Use yesterday's data.
                        startTime = startTime.AddDays(-1);
                        endTime = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        endTime = now >= close ? close : now;
                    }
                    break;
                case YahooFinanceTimeRange.FiveDay:
                    // Note: 5 business days ago is the same as 7 days ago.
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        endTime = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        endTime = close;
                    }
                    startTime = endTime.AddDays(-7);
                    break;
                case YahooFinanceTimeRange.OneMonth:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        endTime = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        endTime = close;
                    }
                    startTime = endTime.AddMonths(-1);
                    break;
                case YahooFinanceTimeRange.ThreeMonth:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        endTime = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        endTime = close;
                    }
                    startTime = endTime.AddMonths(-3);
                    break;
                case YahooFinanceTimeRange.SixMonth:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        endTime = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        endTime = close;
                    }
                    startTime = endTime.AddMonths(-6);
                    break;
                case YahooFinanceTimeRange.OneYear:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        endTime = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        endTime = close;
                    }
                    startTime = endTime.AddYears(-1);
                    break;
                case YahooFinanceTimeRange.TwoYear:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        endTime = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        endTime = close;
                    }
                    startTime = endTime.AddYears(-2);
                    break;
                case YahooFinanceTimeRange.FiveYear:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        endTime = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        endTime = close;
                    }
                    startTime = endTime.AddYears(-5);
                    break;
                case YahooFinanceTimeRange.TenYear:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        endTime = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        endTime = close;
                    }
                    startTime = endTime.AddYears(-10);
                    break;
                case YahooFinanceTimeRange.YearToDate:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        endTime = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        endTime = close;
                    }
                    startTime = new DateTimeOffset(now.Year, 1, 1, 10, 0, 0, tzOffset);
                    break;
                case YahooFinanceTimeRange.Max:
                    if (now < close)
                    {
                        // The market hasn't closed yet. End with yesterday's data.
                        endTime = close.AddDays(-1);
                    }
                    else
                    {
                        // Use today's data.
                        endTime = close;
                    }
                    startTime = new DateTimeOffset(firstTradeDate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(range));
            }

            if (startTime < firstTradeDate)
                startTime = firstTradeDate;

            interval = GetIdealTimeInterval(startTime, endTime, now);
        }

        /// <summary>
        /// Get chart data for the specified stock symbol.
        /// </summary>
        /// <remarks>
        /// <para>Gets chart data for the specified stock symbol.</para>
        /// </remarks>
        /// <param name="symbol">The stock symbol.</param>
        /// <param name="range">The time range.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The chart data.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="symbol"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="symbol"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="range"/> is not a valid enum value.
        /// </exception>
        /// <exception cref="YahooFinanceException">
        /// The Yahoo! Finance server returned an error.
        /// </exception>
        public async Task<YahooFinanceChart> GetChartAsync(string symbol, YahooFinanceTimeRange range, CancellationToken cancellationToken = default)
        {
            ValidateSymbol(symbol);
            ValidateTimeRange(range);

            var quote = await GetQuoteAsync(symbol, cancellationToken).ConfigureAwait(false);

            GetDefaultChartParameters(quote, range, out var startTime, out var endTime, out var interval);

            return await GetChartAsync(quote, startTime, endTime, interval, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get chart data.
        /// </summary>
        /// <remarks>
        /// <para>Gets chart data.</para>
        /// <para>A <see cref="YahooFinanceChart"/> can be used to graph the fluctuation of stock prices over a period of time.</para>
        /// </remarks>
        /// <param name="quote">The stock quote.</param>
        /// <param name="range">The time range.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The chart data.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="quote"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="range"/> is not a valid enum value.
        /// </exception>
        /// <exception cref="YahooFinanceException">
        /// The Yahoo! Finance server returned an error.
        /// </exception>
        public Task<YahooFinanceChart> GetChartAsync(YahooFinanceQuote quote, YahooFinanceTimeRange range, CancellationToken cancellationToken = default)
        {
            GetDefaultChartParameters(quote, range, out var startTime, out var endTime, out var interval);

            return GetChartAsync(quote, startTime, endTime, interval, cancellationToken);
        }

        /// <summary>
        /// Get chart data for the specified stock symbol.
        /// </summary>
        /// <remarks>
        /// <para>Gets chart data for the specified stock symbol.</para>
        /// </remarks>
        /// <param name="symbol">The stock symbol.</param>
        /// <param name="startTime">The start of the time range.</param>
        /// <param name="endTime">The end of the time range.</param>
        /// <param name="interval">The time interval.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The chart data.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="symbol"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="symbol"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="startTime"/> is not within the valid range of start times.</para>
        /// <para>-or-</para>
        /// <para><paramref name="endTime"/> is not within the valid range of start times.</para>
        /// <para>-or-</para>
        /// <para><paramref name="interval"/> is not a valid interval for the specified time range.</para>
        /// </exception>
        /// <exception cref="YahooFinanceException">
        /// The Yahoo! Finance server returned an error.
        /// </exception>
        public async Task<YahooFinanceChart> GetChartAsync(string symbol, DateTimeOffset startTime, DateTimeOffset endTime, YahooFinanceTimeInterval interval, CancellationToken cancellationToken = default)
        {
            var quote = await GetQuoteAsync(symbol, cancellationToken).ConfigureAwait(false);

            return await GetChartAsync(quote, startTime, endTime, interval, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get chart data for the specified stock quote.
        /// </summary>
        /// <remarks>
        /// <para>Gets chart data for the specified stock quote.</para>
        /// </remarks>
        /// <param name="quote">The stock quote.</param>
        /// <param name="startTime">The start of the time range.</param>
        /// <param name="endTime">The end of the time range.</param>
        /// <param name="interval">The time interval.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The chart data.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="quote"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="startTime"/> is not within the valid range of start times.</para>
        /// <para>-or-</para>
        /// <para><paramref name="endTime"/> is not within the valid range of start times.</para>
        /// <para>-or-</para>
        /// <para><paramref name="interval"/> is not a valid interval for the specified time range.</para>
        /// </exception>
        /// <exception cref="YahooFinanceException">
        /// The Yahoo! Finance server returned an error.
        /// </exception>
        public async Task<YahooFinanceChart> GetChartAsync(YahooFinanceQuote quote, DateTimeOffset startTime, DateTimeOffset endTime, YahooFinanceTimeInterval interval, CancellationToken cancellationToken = default)
        {
            ValidateArguments(quote, startTime, endTime, interval);

            const string format = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?symbol={0}&period1={1}&period2={2}&useYfid=true&interval={3}&includePrePost=true&events=div|split|earn&lang=en-US&region=US&crumb={4}&corsDomain=finance.yahoo.com";
            var requestUri = string.Format(format, quote.Symbol, SecondsSinceEpoch(startTime), SecondsSinceEpoch(endTime), TimeIntervals[(int)interval], crumb);

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

        /// <summary>
        /// Get historic stock trade data for the specified stock symbol.
        /// </summary>
        /// <remarks>
        /// Gets historic stock trade data for the specified stock symbol.
        /// </remarks>
        /// <param name="symbol">The stock symbol.</param>
        /// <param name="startTime">The start of the time range.</param>
        /// <param name="endTime">The end of the time range.</param>
        /// <param name="interval">The time interval.</param>
        /// <param name="includeAdjustedClose"><c>true</c> if adjusted close indicators should be included; otherwise, <c>false</c>.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The historic stock trade data.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="symbol"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="symbol"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="startTime"/> is not within the valid range of start times.</para>
        /// <para>-or-</para>
        /// <para><paramref name="endTime"/> is not within the valid range of start times.</para>
        /// <para>-or-</para>
        /// <para><paramref name="interval"/> is not a valid interval for the specified time range.</para>
        /// </exception>
        /// <exception cref="YahooFinanceException">
        /// The Yahoo! Finance server returned an error.
        /// </exception>
        public async Task<YahooFinanceHistoricTradeData> GetHistoricTradeDataAsync(string symbol, DateTimeOffset startTime, DateTimeOffset endTime, YahooFinanceTimeInterval interval, bool includeAdjustedClose, CancellationToken cancellationToken = default)
        {
            ValidateSymbol(symbol);

            const string format = "https://query1.finance.yahoo.com/v7/finance/download/{0}?period1={1}&period2={2}&interval={3}&events=history&includeAdjustedClose=true";
            var requestUri = string.Format(format, symbol, SecondsSinceEpoch(startTime), SecondsSinceEpoch(endTime), TimeIntervals[(int)interval], includeAdjustedClose ? "true" : "false");
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
                                return await YahooFinanceHistoricTradeData.LoadAsync(stream, cancellationToken).ConfigureAwait(false);

                            if (response.StatusCode == HttpStatusCode.Unauthorized && retries < 5)
                            {
                                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                                retries++;
                                continue;
                            }

                            using (var reader = new StreamReader(stream))
                            {
                                var text = reader.ReadToEnd();
                                var json = JObject.Parse(text);

                                var code = json.SelectToken("finance.error.code").ToString();
                                var description = json.SelectToken("finance.error.description").ToString();

                                throw new YahooFinanceException(response.StatusCode, code, description);
                            }
                        }
                    }
                }
            } while (true);
        }

        /// <summary>
		/// Release the unmanaged resources used by the <see cref="YahooFinanceClient"/> and
		/// optionally releases the managed resources.
		/// </summary>
		/// <remarks>
		/// Releases the unmanaged resources used by the <see cref="YahooFinanceClient"/> and
		/// optionally releases the managed resources.
		/// </remarks>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
		/// <c>false</c> to release only the unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (disposing && client != null)
            {
                client.Dispose();
                client = null;
            }
        }

        /// <summary>
		/// Release all resource used by the <see cref="YahooFinanceClient"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="YahooFinanceClient"/>. The
		/// <see cref="Dispose()"/> method leaves the <see cref="YahooFinanceClient"/> in an unusable state. After calling
		/// <see cref="Dispose()"/>, you must release all references to the <see cref="YahooFinanceClient"/> so the garbage
		/// collector can reclaim the memory that the <see cref="YahooFinanceClient"/> was occupying.</remarks>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}
