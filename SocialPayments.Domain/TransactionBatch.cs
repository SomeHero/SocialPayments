using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SocialPayments.Domain
{
    public class TransactionBatch
    {
        public Guid Id { get; set; }
        public int TotalNumberOfDeposits { get; set; }
        public int TotalNumberOfWithdrawals { get; set; }
        public double TotalDepositAmount { get; set; }
        public double TotalWithdrawalAmount { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? LastDateUpdated { get; set; }
        public bool IsClosed { get; set; }

        public virtual Collection<Transaction> Transactions { get; set; } 
    }
}
