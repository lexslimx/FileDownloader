using System;
using System.Net;

namespace FileDownloader.Core.Exceptions
{
    public class CustomException : Exception
    {
        public string Code { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }

        public CustomException()
        {
        }

        public CustomException(string code, string message, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            Code = code;
            HttpStatusCode = httpStatusCode;
        }
    }
}