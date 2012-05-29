﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum PaymentStatus
    {
        Submitted = 0,
        Pending = 1,
        Complete = 2,
        ReturnedNSF = 3,
        Cancelled = 4,
        Refunded = 5
    }
}
