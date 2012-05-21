using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SocialPayments.Domain.Interfaces
{
    public interface ITransactionBatch
    {
        Guid Id { get; set; }
        int TotalNumberOfDeposits { get; set; }
        int TotalNumberOfWithdrawals { get; set; }
        double TotalDepositAmount { get; set; }
        double TotalWithdrawalAmount { get; set; }
        DateTime CreateDate { get; set; }
        DateTime? ClosedDate { get; set; }
        DateTime? LastDateUpdated { get; set; }
        bool IsClosed { get; set; }
        List<ITransaction> Transactions { get; set; } 
    }
}
