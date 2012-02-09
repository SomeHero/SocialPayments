using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.Web.Admin.Models
{
    public class OverviewModel
    {
        public OverviewModel() {
            CurrentBatchStats = new CurrentBatchStats();
            ClosedBatches = new List<ClosedBatch>();
        }

        public CurrentBatchStats CurrentBatchStats { get; set; }
        public List<ClosedBatch> ClosedBatches { get; set; }
    }
    public class CurrentBatchStats
    {
        public DateTime BatchCreated { get; set; }
        public int TotalNumberOfPayments { get; set; }
        public int TotalNumberOfDeposits { get; set; }
        public int TotalNumberOfWithdrawals { get; set; }
        public double TotalDepositAmount { get; set; }
        public double TotalWithdrawalAmount { get; set; }
        public int TotalVerifiedPayments { get; set; }
        public int TotalUnVerifiedPayments { get; set; }
    }
    public class ClosedBatch
    {
        public DateTime BatchCreated { get; set; }
        public DateTime? BatchClosed { get; set; }
        public int TotalNumberOfDeposits { get; set; }
        public int TotalNumberOfWithdrawals { get; set; }
        public double TotalDepositAmount { get; set; }
        public double TotalWithdrawalAmount { get; set; }
    }
}