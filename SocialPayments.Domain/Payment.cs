using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid ApiKey { get; set; }
        [ForeignKey("ApiKey")]
        public virtual Application Application { get; set; }
        public Guid FromAccountId { get; set; }
        [ForeignKey("FromAccountId")]
        public virtual PaymentAccount FromAccount { get; set; }

        public string FromMobileNumber { get; set; }

        public Guid? ToAccountId { get; set; }
        [ForeignKey("ToAccountId")]
        public virtual PaymentAccount ToAccount { get; set; }

        public string ToMobileNumber { get; set; }
        public double PaymentAmount { get; set; }

        public int StandardEntryClassValue { get; set; }
        public StandardEntryClass StandardEntryClass
        {
            get { return (StandardEntryClass)StandardEntryClassValue; }
            set { StandardEntryClassValue = (int)value; }
        }
        public int PaymentChannelTypeValue { get; set; }
        public PaymentChannelType PaymentChannelType
        {
            get { return (PaymentChannelType)PaymentChannelTypeValue; }
            set { PaymentChannelTypeValue = (int)value; }
        }
        public int PaymentStatusValue { get; set; }
        public PaymentStatus PaymentStatus
        {
            get { return (PaymentStatus)PaymentStatusValue; }
            set { PaymentStatusValue = (int)value; }
        }


        public string Comments { get; set; }
        public string ACHTransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
  
        public virtual List<Transaction> Transactions { get; set; } 
    }
}
