using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using SocialPayments.Domain;

namespace SocialPayments.DataLayer.Interfaces
{
    public interface IDbContext
    {
        IDbSet<Application> Applications { get; set; }
        IDbSet<User> Users { get; set; }
        IDbSet<UserAttribute> UserAttributes { get; set; }
        IDbSet<Role> Roles { get; set; }
        IDbSet<Message> Messages { get; set; }
        IDbSet<PaymentAccount> PaymentAccounts { get; set; }
        IDbSet<BatchFile> BatchFiles { get; set; }
        IDbSet<Calendar> Calendars { get; set; }
        IDbSet<EmailLog> EmailLog { get; set; }
        IDbSet<SMSLog> SMSLog { get; set; }
        IDbSet<Transaction> Transactions { get; set; }
        IDbSet<TransactionBatch> TransactionBatches { get; set; }
        IDbSet<BetaSignup> BetaSignUps { get; set; }
        IDbSet<MobileNumberSignUpKey> MobileNumberSignUpKeys { get; set; }
        IDbSet<MECode> MECodes { get; set; }

        void SaveChanges();
    }

}
