using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.DomainServices.CustomExceptions
{
    public class AccountLockedPinCodeFailures: ApplicationException
    {
        public int NumberOfFailures { get; set; }
        public int LockOutInterval { get; set; }
        public bool TemporaryLockout { get; set; }

        public AccountLockedPinCodeFailures()
        { }

        public AccountLockedPinCodeFailures(string message)
            : base(message)
        {

        }
        public AccountLockedPinCodeFailures(string message, Exception innerException) :
            base(message, innerException)
        {

        }
    
    }
}
