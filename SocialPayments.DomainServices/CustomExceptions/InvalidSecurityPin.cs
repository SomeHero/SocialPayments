using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.DomainServices.CustomExceptions
{
    public class InvalidSecurityPin: ApplicationException
    {
        public InvalidSecurityPin() { }
        public InvalidSecurityPin(string message) : base(message) {}
        public InvalidSecurityPin(string message, Exception innerException) : base(message, innerException) { }
    }
}
