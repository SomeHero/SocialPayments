﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain.CustomAttributes;

namespace SocialPayments.Domain
{
    public enum PaystreamMessageStatus
    {
        [MessageStatusAttribute(RecipientDescription = "Submitted", SenderDescription = "Submitted")]
        SubmittedPayment = 0,
        [MessageStatusAttribute(RecipientDescription = "Submitted", SenderDescription = "Submitted")]
        SubmittedRequest = 1,
        [MessageStatusAttribute(RecipientDescription = "Action Needed", SenderDescription = "Notified")]
        NotifiedPayment = 2,
        [MessageStatusAttribute(RecipientDescription = "Processing", SenderDescription = "Processing")]
        ProcessingPayment = 3,
        [MessageStatusAttribute(RecipientDescription = "Processing", SenderDescription = "Processing")]
        HoldPayment = 4,
        [MessageStatusAttribute(RecipientDescription = "Sent to Bank", SenderDescription = "Sent to Bank")]
        ProcessedPayment = 5,
        [MessageStatusAttribute(RecipientDescription = "Failed", SenderDescription = "Failed")]
        FailedPayment = 6,
        [MessageStatusAttribute(RecipientDescription = "Cancelled", SenderDescription = "Cancelled")]
        CancelledPayment = 7,
        [MessageStatusAttribute(RecipientDescription = "Complete", SenderDescription = "Complete")]
        CompletePayment = 8,
        [MessageStatusAttribute(RecipientDescription = "Returned", SenderDescription = "")]
        ReturnedPayment = 9,
        [MessageStatusAttribute(RecipientDescription = "", SenderDescription = "Request Sent")]
        NotifiedRequest = 10,
        [MessageStatusAttribute(RecipientDescription = "Action Needed", SenderDescription = "Awaiting Response")]
        PendingRequest = 11,
        [MessageStatusAttribute(RecipientDescription = "Accepted", SenderDescription = "Accepted")]
        AcceptedRequest = 12,
        [MessageStatusAttribute(RecipientDescription = "Rejected", SenderDescription = "Rejected")]
        RejectedRequest = 13,
        [MessageStatusAttribute(RecipientDescription = "Cancelled", SenderDescription = "Cancelled")]
        CancelledRequest = 14,
        [MessageStatusAttribute(RecipientDescription = "Submitted", SenderDescription = "Submitted")]
        SubmittedPledge = 15,
        [MessageStatusAttribute(RecipientDescription = "", SenderDescription = "Request Sent")]
        NotifiedPledge = 16,
        [MessageStatusAttribute(RecipientDescription = "Action Needed", SenderDescription = "Awaiting Response")]
        PendingPledge = 17,
        [MessageStatusAttribute(RecipientDescription = "Accepted", SenderDescription = "Accepted")]
        AcceptedPledge = 18,
        [MessageStatusAttribute(RecipientDescription = "Rejected", SenderDescription = "Rejected")]
        RejectedPledge = 19,
        [MessageStatusAttribute(RecipientDescription = "Cancelled", SenderDescription = "Cancelled")]
        CancelledPledge = 20,
        [MessageStatusAttribute(RecipientDescription = "Donation Submitted", SenderDescription = "Donation Submitted")]
        SubmittedDonation = 21,
        
    }
}
