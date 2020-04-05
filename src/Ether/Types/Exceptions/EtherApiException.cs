using System;
using System.Net;

namespace Ether.Types.Exceptions
{
    public class EtherApiException : Exception
    {
        public EtherApiException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
    }
}
