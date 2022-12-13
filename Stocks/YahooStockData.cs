﻿using System.Text;
using System.Globalization;

namespace Stocks
{
    public class YahooStockData
    {
        const NumberStyles CsvValueStyle = NumberStyles.AllowDecimalPoint | NumberStyles.Integer;
        static readonly char[] CsvDelimeters = { ',' };

        readonly List<object[]> values;
        string[] headers;

        YahooStockData()
        {
            values = new List<object[]>();
        }

        public int Columns
        {
            get
            {
                return headers.Length;
            }
        }

        public int Rows
        {
            get
            {
                return values.Count;
            }
        }

        public string GetHeader(int column)
        {
            return headers[column];
        }

        public object GetValue(int row, int column)
        {
            return values[row][column];
        }

        public static async Task<YahooStockData> LoadAsync(Stream stream, CancellationToken cancellationToken)
        {
            var stockData = new YahooStockData();

            using (var reader = new StreamReader(stream, Encoding.ASCII, false, 4096, true))
            {
                var line = await reader.ReadLineAsync();
                var tokens = line.Split(CsvDelimeters);

                stockData.headers = tokens;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    tokens = line.Split(CsvDelimeters);

                    if (tokens.Length != stockData.headers.Length)
                        Console.WriteLine("Inconsistent number of columns: {0} vs {1}", tokens.Length, stockData.headers.Length);

                    var values = new object[tokens.Length];

                    if (tokens[0] != "null")
                        values[0] = DateTime.Parse(tokens[0], CultureInfo.InvariantCulture);

                    for (int i = 1; i < tokens.Length; i++)
                    {
                        if (tokens[i] == "null")
                            continue;

                        if (!double.TryParse(tokens[i], CsvValueStyle, CultureInfo.InvariantCulture, out var value))
                        {
                            Console.WriteLine("Failed to parse CSV double value: {0}", tokens[i]);
                        }
                        else
                        {
                            values[i] = value;
                        }
                    }

                    stockData.values.Add(values);
                }
            }

            return stockData;
        }
    }
}