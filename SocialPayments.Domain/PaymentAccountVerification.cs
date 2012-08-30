using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class PaymentAccountVerification
    {
        public Guid Id { get; set; }
        public Guid PaymentAccountId { get; set; }
        public double DepositAmount1 { get; set; }
        public double DepositAmount2 { get; set; }
        public double WithdrawalAmount { get; set; }
        public DateTime Sent { get; set; }
        public DateTime EstimatedSettlementDate { get; set; }
        public DateTime? VerificationDate { get; set; }
        public int StatusValue { get; set; }
        public PaymentAccountVerificationStatus Status
        {
            get { return (PaymentAccountVerificationStatus)StatusValue; }
            set { StatusValue = (int)value; }
        }
        public int NumberOfFailures { get; set; }

        [ForeignKey("PaymentAccountId")]
        public virtual PaymentAccount PaymentAccount { get; set; }
    }
}
