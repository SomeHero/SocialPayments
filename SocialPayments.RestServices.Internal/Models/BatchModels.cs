using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class BatchModels
    {
        public class BatchResponse
        {
            public Guid Id { get; set; }
            public int TotalNumberOfDeposits { get; set; }
            public int TotalNumberOfWithdrawals { get; set; }
            public double TotalDepositAmount { get; set; }
            public double TotalWithdrawalAmount { get; set; }
            public string CreateDate { get; set; }
            public string ClosedDate { get; set; }
            public string LastDateUpdated { get; set; }
            public bool IsClosed { get; set; }

            public virtual IEnumerable<TransactionModels.TransactionResponse> Transactions { get; set; } 
        }
    }
}