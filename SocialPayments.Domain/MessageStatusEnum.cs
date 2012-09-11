using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public enum MessageStatusEnum : int
    {
        Submitted = 1,
        Pending = 2,
        Complete = 3,
        CancelPending = 4,
        Cancelled = 5,
        RefundPending = 6,
        Refunded = 7,
        FailedPending = 8,
        Failed = 9,
        AcceptRequestPending = 10,
        RequestAccepted = 11,
        RejectRequestPending = 12,
        RequestRejected = 13,
        ReturnPending = 14,
        Returned = 15,
        SpamPending = 16,
        Spam = 17
    }
}
