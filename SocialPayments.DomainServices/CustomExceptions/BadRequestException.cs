using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.DomainServices.CustomExceptions
{
    public class BadRequestException : ApplicationException
    {
        public BadRequestException()
        { }

        public BadRequestException(string message)
            : base(message)
        {

        }
        public BadRequestException(string message, Exception innerException) :
            base(message, innerException)
        {

        }
    }
}
