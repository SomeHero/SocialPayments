using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class AccountVerificationRequest
    {
        public double depositAmount1 { get; set; }
        public double depositAmount2 { get; set; }
    }
    public class AddVerificationRequest
    {
        public double DepositAmount1 { get; set; }
        public double DepositAmount2 { get; set; }
        public double WithdrawalAmount { get; set; }
        public DateTime SentDate { get; set; }
        public DateTime EstimatedSettlementDate { get; set; }
    }
}
