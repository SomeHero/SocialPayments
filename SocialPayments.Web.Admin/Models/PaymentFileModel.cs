using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Web.Admin.Models
{
    public class PaymentFileModel
    {
        public Guid Id { get; set; }
        public DateTime OpenedDate { get; set; }
        public DateTime ClosedDate { get; set; }
        public int NumberOfDeposits { get; set; }
        public int NumberOfWithdrawls { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public double TotalDepositAmount { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public double TotalWithdrawlAmount { get; set; }
    }
}