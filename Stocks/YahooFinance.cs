using System.Net;

using Newtonsoft.Json.Linq;

namespace Stocks
{
    public static class YahooFinance
    {
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        static HttpClient client;

        static YahooFinance()
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            handler.UseCookies = true;

            client = new HttpClient(handler);
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
                }
            }

            return list;
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
