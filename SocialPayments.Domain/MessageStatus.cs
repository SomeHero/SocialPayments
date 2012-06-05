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
        RequestAcceptedPending = 8,
        RequestRejectedPending = 9,
        RequestRejected = 10,
        ReturnPending = 11,
        Returned = 12, 
        Spam = 13
    }
}
