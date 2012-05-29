using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer.Interfaces;
using NLog;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class PaymentAccountService
    {
        private IDbContext _ctx;
        private Logger _logger;
        private SecurityService _securityServices;

        public PaymentAccountService() { 
            _ctx = new Context();
            _logger = LogManager.GetCurrentClassLogger();
           _securityServices = new SecurityService();
        }

        public PaymentAccountService(IDbContext context)
        {
             _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();
           _securityServices = new SecurityService();

        }
        public PaymentAccount GetPaymentAccount(string paymentAccountId)
        {
            Guid paymentAccountIdGuid;

            Guid.TryParse(paymentAccountId, out paymentAccountIdGuid);

            if (paymentAccountIdGuid == null)
                throw new ArgumentException("Payment Account Specified is Invalid", paymentAccountId);

            var paymentAccount = _ctx.PaymentAccounts
                .FirstOrDefault(p => p.Id.Equals(paymentAccountIdGuid));

            return paymentAccount;
        }
        public PaymentAccount AddPaymentAccount(string userId, string nameOnAccount, string routingNumber, string accountNumber, string accountType)
        {
            if (!(accountType.ToUpper().Equals("SAVINGS") || accountType.ToUpper().Equals("CHECKING")))
                throw new ArgumentException("Invalid Account Type Specifieid", "accountType");

            Guid userIdGuid;

            Guid.TryParse(userId, out userIdGuid);
            
            if(userIdGuid == null)
                throw new ArgumentException("Invalid User Id Specified", "userId");

            var paymentAccountType = PaymentAccountType.Checking;

            if(accountType.ToUpper().Equals("SAVINGS"))
                paymentAccountType = PaymentAccountType.Checking;

            var paymentAccount = _ctx.PaymentAccounts.Add(new PaymentAccount()
            {
                Id = Guid.NewGuid(),
                NameOnAccount = _securityServices.Encrypt(nameOnAccount),
                RoutingNumber = _securityServices.Encrypt(routingNumber),
                AccountNumber = _securityServices.Encrypt(accountNumber),
                AccountStatus = AccountStatusType.Submitted,
                AccountType = paymentAccountType,
                CreateDate = System.DateTime.Now,
                UserId = userIdGuid
            });

            _ctx.SaveChanges();

            return paymentAccount;
        }

        public void UpdatePaymentAccount(PaymentAccount paymentAccount)
        {
            _ctx.SaveChanges();
        }
        public void DeletePaymentAccount(string id)
        {
            Guid paymentAccountGuid;

            Guid.TryParse(id, out paymentAccountGuid);

            if(paymentAccountGuid == null)
                throw new ArgumentException("Payment Account Specified is Invalid", id);

            var paymentAccount = _ctx.PaymentAccounts.FirstOrDefault(p => p.Id.Equals(paymentAccountGuid));

            if(paymentAccount == null)
                throw new ArgumentException("Payment Account Not Found", id);

            paymentAccount.AccountStatus = AccountStatusType.Deleted;
            paymentAccount.LastUpdatedDate = System.DateTime.Now;

            _ctx.SaveChanges();
        }
    }
}
