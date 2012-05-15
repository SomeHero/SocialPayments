using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum MessageStatus
    {
        Submitted = 0,
        Pending = 1,
        Completed = 2,
        Cancelled = 3,
        Refunded = 4,
        Failed = 5,
        CancelPending = 6,
        RefundPending = 7,
        RequestAccepted = 8,
        RequestRejected = 9
    }
}
