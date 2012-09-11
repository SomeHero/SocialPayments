using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.DomainServices.CustomExceptions
{
    public class NotFoundException: ApplicationException
    {
        public int NumberOfFailures { get; set; }
        public int LockOutInterval { get; set; }
        public bool TemporaryLockout { get; set; }

        public NotFoundException()
        { }

        public NotFoundException(string message)
            : base(message)
        {

        }
        public NotFoundException(string message, Exception innerException) :
            base(message, innerException)
        {

        }
    }
}
