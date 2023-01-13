using System;
using System.Net;
using System.Net.Http;

namespace Stocks
{
    class YahooFinanceException : Exception
    {
        public YahooFinanceException(HttpStatusCode statusCode, string code, string description) : base (description)
        {
            StatusCode = statusCode;
            Code = code;
        }

        public string Code { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}
