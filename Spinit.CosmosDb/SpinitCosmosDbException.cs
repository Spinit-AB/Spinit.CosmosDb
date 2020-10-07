using System;
using System.Net;

namespace Spinit.CosmosDb
{
    public class SpinitCosmosDbException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; private set; }

        public SpinitCosmosDbException(HttpStatusCode httpStatusCode, string message)
            : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }

        public SpinitCosmosDbException(HttpStatusCode httpStatusCode, string message, Exception innerException)
            : base(message, innerException)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}
