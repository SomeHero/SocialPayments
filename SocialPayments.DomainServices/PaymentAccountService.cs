using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer.Interfaces;
using NLog;
using SocialPayments.DataLayer;
using System.Configuration;

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

            Domain.PaymentAccount paymentAccount = null;
            try
            {
                using (var ctx = new Context())
                {
                    var userService = new UserService(ctx);
                    var amazonNotificationService = new AmazonNotificationService();

                    var user = userService.GetUserById(userId);

                    if (user == null)
                    {
                        throw new Exception(String.Format("User {0} Not Found", userId));
                    }

                    var paymentAccountType = PaymentAccountType.Checking;

                    if (accountType.ToUpper().Equals("SAVINGS"))
                        paymentAccountType = PaymentAccountType.Checking;

                    paymentAccount = ctx.PaymentAccounts.Add(new PaymentAccount()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.UserId,
                        NameOnAccount = _securityServices.Encrypt(nameOnAccount),
                        RoutingNumber = _securityServices.Encrypt(routingNumber),
                        AccountNumber = _securityServices.Encrypt(accountNumber),
                        AccountStatus = AccountStatusType.Submitted,
                        AccountType = paymentAccountType,
                        CreateDate = System.DateTime.Now,
                        BankName = "",
                        BankIconURL = "",
                        Nickname = ""
                    });

                    if (user.PreferredReceiveAccount == null)
                        user.PreferredReceiveAccount = paymentAccount;
                    if (user.PreferredSendAccount == null)
                        user.PreferredSendAccount = paymentAccount;

                    ctx.SaveChanges();

                    if (user.UserStatus == UserStatus.Registered)
                    {
                        try
                        {
                            amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["UserPostedTopicARN"], "Payment Account Added for user {0}", user.UserId.ToString());
                        }
                        catch (Exception ex)
                        {
                            _logger.Log(LogLevel.Error, string.Format("AmazonPNS failed when registering user {0}. Exception {1}.", user.UserId, ex.Message));
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Fatal, String.Format("Unhandled Exception Adding Payment Account. {0}", ex.Message));
                throw ex; ; 
            }

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
