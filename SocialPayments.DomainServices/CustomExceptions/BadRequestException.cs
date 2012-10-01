using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.DomainServices.CustomExceptions
{
    public class BadRequestException : ApplicationException
    {
        public int ErrorCode { get; set; }

        public BadRequestException()
        { }

        public BadRequestException(string message)
            : base(message)
        {
            ErrorCode = 0;
        }
        public BadRequestException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }
        public BadRequestException(string message, Exception innerException) :
            base(message, innerException)
        {
            ErrorCode = 0;
        }
        public BadRequestException(string message, int errorCode, Exception innerException) :
            base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
