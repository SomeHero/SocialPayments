using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum PaymentAccountVerificationStatus
    {
        Submitted = 0,
        Delivered = 1,
        Verified = 2,
        Expired = 3,
        Failed = 4
    }
}
