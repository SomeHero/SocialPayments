using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain.CustomAttributes;

namespace SocialPayments.Domain
{
    public enum PaystreamMessageStatus
    {
        [MessageStatusAttribute(RecipientDescription = "Submitted", SenderDescription = "Submitted", IsCancellable=true)]
        SubmittedPayment = 0,
        [MessageStatusAttribute(RecipientDescription = "Submitted", SenderDescription = "Submitted", IsCancellable=true)]
        SubmittedRequest = 1,
        [MessageStatusAttribute(RecipientDescription = "Action Needed", SenderDescription = "Notified", IsCancellable=true)]
        NotifiedPayment = 2,
        [MessageStatusAttribute(RecipientDescription = "Processing", SenderDescription = "Processing", IsCancellable = true)]
        ProcessingPayment = 3,
        [MessageStatusAttribute(RecipientDescription = "Processing", SenderDescription = "Processing", IsCancellable = true)]
        HoldPayment = 4,
        [MessageStatusAttribute(RecipientDescription = "Sent to Bank", SenderDescription = "Sent to Bank", IsCancellable = false)]
        ProcessedPayment = 5,
        [MessageStatusAttribute(RecipientDescription = "Failed", SenderDescription = "Failed", IsCancellable = false)]
        FailedPayment = 6,
        [MessageStatusAttribute(RecipientDescription = "Cancelled", SenderDescription = "Cancelled", IsCancellable = false)]
        CancelledPayment = 7,
        [MessageStatusAttribute(RecipientDescription = "Complete", SenderDescription = "Complete", IsCancellable = false)]
        CompletePayment = 8,
        [MessageStatusAttribute(RecipientDescription = "Returned", SenderDescription = "", IsCancellable = false)]
        ReturnedPayment = 9,
        [MessageStatusAttribute(RecipientDescription = "", SenderDescription = "Request Sent", IsCancellable = true)]
        NotifiedRequest = 10,
        [MessageStatusAttribute(RecipientDescription = "Action Needed", SenderDescription = "Awaiting Response", IsCancellable = true)]
        PendingRequest = 11,
        [MessageStatusAttribute(RecipientDescription = "Accepted", SenderDescription = "Accepted", IsCancellable = false)]
        AcceptedRequest = 12,
        [MessageStatusAttribute(RecipientDescription = "Rejected", SenderDescription = "Rejected", IsCancellable = false)]
        RejectedRequest = 13,
        [MessageStatusAttribute(RecipientDescription = "Cancelled", SenderDescription = "Cancelled", IsCancellable = false)]
        CancelledRequest = 14,
        [MessageStatusAttribute(RecipientDescription = "Submitted", SenderDescription = "Submitted", IsCancellable = false)]
        SubmittedPledge = 15,
        [MessageStatusAttribute(RecipientDescription = "", SenderDescription = "Request Sent", IsCancellable = false)]
        NotifiedPledge = 16,
        [MessageStatusAttribute(RecipientDescription = "Action Needed", SenderDescription = "Awaiting Response", IsCancellable = false)]
        PendingPledge = 17,
        [MessageStatusAttribute(RecipientDescription = "Accepted", SenderDescription = "Accepted", IsCancellable = false)]
        AcceptedPledge = 18,
        [MessageStatusAttribute(RecipientDescription = "Rejected", SenderDescription = "Rejected", IsCancellable = false)]
        RejectedPledge = 19,
        [MessageStatusAttribute(RecipientDescription = "Cancelled", SenderDescription = "Cancelled", IsCancellable = false)]
        CancelledPledge = 20,
        [MessageStatusAttribute(RecipientDescription = "Donation Submitted", SenderDescription = "Donation Submitted", IsCancellable = true)]
        SubmittedDonation = 21,
        
    }
}
