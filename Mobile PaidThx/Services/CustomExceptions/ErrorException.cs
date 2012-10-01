using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Services.CustomExceptions
{
    public class ErrorException: ApplicationException
    {

        public int ErrorCode { get; set; }

                public ErrorException()
        { }

        public ErrorException(string message)
            : base(message)
        {
            ErrorCode = 0;
        }
        public ErrorException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }
        public ErrorException(string message, Exception innerException) :
            base(message, innerException)
        {
            ErrorCode = 0;
        }
        public ErrorException(string message, int errorCode, Exception innerException) :
            base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}