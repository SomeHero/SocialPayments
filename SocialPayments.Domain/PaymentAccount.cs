using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
namespace SocialPayments.Domain
{
    public class PaymentAccount
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public string NameOnAccount { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public int PaymentAccountTypeId { get; set; }
        public PaymentAccountType AccountType 
        {
            get { return (PaymentAccountType)PaymentAccountTypeId; }
            set { PaymentAccountTypeId = (int)value; }
        }
        public bool IsActive { get; set; }
       // public int AccountStatusValue { get; set; }
       // [ForeignKey("AccountStatusValue")]
       // public AccountStatusType AccountStatus { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
    }
}
