using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface IPaymentAccount
    {
        Guid Id { get; set; }
        Guid UserId { get; set; }
        IUser User { get; set; }
        string NameOnAccount { get; set; }
        string RoutingNumber { get; set; }
        string AccountNumber { get; set; }
        int PaymentAccountTypeId { get; set; }
        PaymentAccountType AccountType { get; set; }
        bool IsActive { get; set; }
        DateTime CreateDate { get; set; }
        DateTime? LastUpdatedDate { get; set; }
    }
}
