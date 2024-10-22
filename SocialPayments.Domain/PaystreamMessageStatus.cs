﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain.CustomAttributes;

namespace SocialPayments.Domain
{
    public enum PaystreamMessageStatus
    {
        [MessageStatusAttribute(RecipientDescription = "Submitted", SenderDescription = "Submitted", IsAcceptable=false, IsRejectable=false, IsCancellable=true, IsRemindable=false)]
        SubmittedPayment = 0,
        [MessageStatusAttribute(RecipientDescription = "Submitted", SenderDescription = "Submitted", IsAcceptable=true, IsRejectable=false, IsCancellable=true, IsRemindable=false)]
        SubmittedRequest = 1,
        [MessageStatusAttribute(RecipientDescription = "Respond", SenderDescription = "Notified", IsAcceptable=false, IsRejectable=false, IsCancellable=true, IsRemindable=true)]
        NotifiedPayment = 2,
        [MessageStatusAttribute(RecipientDescription = "Processing", SenderDescription = "Processing", IsAcceptable=false, IsRejectable=false, IsCancellable = true, IsRemindable=false)]
        ProcessingPayment = 3,
        [MessageStatusAttribute(RecipientDescription = "Processing", SenderDescription = "Processing", IsAcceptable = false, IsRejectable = false, IsCancellable = true, IsRemindable = false)]
        HoldPayment = 4,
        [MessageStatusAttribute(RecipientDescription = "Sent to Bank", SenderDescription = "Sent to Bank", IsAcceptable = false, IsRejectable = false, IsCancellable = false, IsRemindable = false)]
        ProcessedPayment = 5,
        [MessageStatusAttribute(RecipientDescription = "Failed", SenderDescription = "Failed", IsAcceptable = false, IsRejectable = false, IsCancellable = false, IsRemindable = false)]
        FailedPayment = 6,
        [MessageStatusAttribute(RecipientDescription = "Cancelled", SenderDescription = "Cancelled", IsAcceptable = false, IsRejectable = false, IsCancellable = false, IsRemindable = false)]
        CancelledPayment = 7,
        [MessageStatusAttribute(RecipientDescription = "Complete", SenderDescription = "Complete", IsAcceptable = false, IsRejectable = false, IsCancellable = false, IsRemindable = false)]
        CompletePayment = 8,
        [MessageStatusAttribute(RecipientDescription = "Returned", SenderDescription = "", IsAcceptable = false, IsRejectable = false, IsCancellable = false, IsRemindable = false)]
        ReturnedPayment = 9,
        [MessageStatusAttribute(RecipientDescription = "", SenderDescription = "Notified", IsAcceptable = true, IsRejectable = true, IsCancellable = true, IsRemindable = true)]
        NotifiedRequest = 10,
        [MessageStatusAttribute(RecipientDescription = "Respond", SenderDescription = "Waiting", IsAcceptable = true, IsRejectable = true, IsCancellable = true, IsRemindable = false)]
        PendingRequest = 11,
        [MessageStatusAttribute(RecipientDescription = "Accepted", SenderDescription = "Accepted", IsAcceptable = false, IsRejectable = false, IsCancellable = false, IsRemindable = false)]
        AcceptedRequest = 12,
        [MessageStatusAttribute(RecipientDescription = "Rejected", SenderDescription = "Rejected", IsAcceptable = false, IsRejectable = false, IsCancellable = false, IsRemindable = false)]
        RejectedRequest = 13,
        [MessageStatusAttribute(RecipientDescription = "Cancelled", SenderDescription = "Cancelled", IsAcceptable = false, IsRejectable = false, IsCancellable = false, IsRemindable = false)]
        CancelledRequest = 14,
        [MessageStatusAttribute(RecipientDescription = "Processing", SenderDescription = "Processing", IsAcceptable = false, IsRejectable = false, IsCancellable = true, IsRemindable = true)]
        PendingPayment = 21
    }
}
