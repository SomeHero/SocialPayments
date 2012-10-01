using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer.Interfaces;
using NLog;
using SocialPayments.DataLayer;
using System.Configuration;
using System.Threading.Tasks;
using SocialPayments.DomainServices.PaymentAccountProcessing;
using SocialPayments.ThirdPartyServices.FedACHService;

namespace SocialPayments.DomainServices
{
    public class PaymentAccountService
    {
        private IDbContext _ctx;
        private Logger _logger;
        private SecurityService _securityServices;
        private string defaultBankIconUrl = ConfigurationManager.AppSettings["DefaultBankIcon"];

        public PaymentAccountService()
        {
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

            return GetPaymentAccount(paymentAccountIdGuid);
        }
        public PaymentAccount GetPaymentAccount(Guid paymentAccountId)
        {
            var paymentAccount = _ctx.PaymentAccounts
                .FirstOrDefault(p => p.Id.Equals(paymentAccountId));

            return paymentAccount;
        }
        public PaymentAccount AddPaymentAccount(User user, string nickName, string nameOnAccount, string routingNumber,
            string accountNumber, string accountType)
        {
            return AddPaymentAccount(user, nickName, nameOnAccount, routingNumber, accountNumber, accountType, "", null, "");
        }
        public PaymentAccount AddPaymentAccount(User user, string nickName, string nameOnAccount, string routingNumber, 
            string accountNumber, string accountType, string securityPin, int? securityQuestionId, string securityQuestionAnswer)
        {
            if (!(accountType.ToUpper().Equals("SAVINGS") || accountType.ToUpper().Equals("CHECKING")))
                throw new ArgumentException("Invalid Account Type Specifieid", "accountType");

            Domain.PaymentAccount paymentAccount = null;
            FedACHService fedACHService = new FedACHService();

            FedACHList fedACHList = null;
            var result = fedACHService.getACHByRoutingNumber(routingNumber, out fedACHList);

            if (!result)
            {
                throw new Exception("Invalid Routing Number");
            }

            try
            {
                var paymentAccountType = PaymentAccountType.Checking;

                if (accountType.ToUpper().Equals("SAVINGS"))
                    paymentAccountType = PaymentAccountType.Checking;

                paymentAccount = _ctx.PaymentAccounts.Add(new PaymentAccount()
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
                    BankIconURL = defaultBankIconUrl,
                    Nickname = nickName,
                    IsActive = true
                });

                if (user.PreferredReceiveAccount == null)
                    user.PreferredReceiveAccount = paymentAccount;
                if (user.PreferredSendAccount == null)
                    user.PreferredSendAccount = paymentAccount;

                if(!String.IsNullOrEmpty(securityPin))
                    user.SecurityPin = _securityServices.Encrypt(securityPin);

                if(securityQuestionId != null && !String.IsNullOrEmpty(securityQuestionAnswer))
                {
                    user.SecurityQuestionID = securityQuestionId.Value;
                    user.SecurityQuestionAnswer = _securityServices.Encrypt(securityQuestionAnswer);
                }

                _ctx.SaveChanges();

                Task.Factory.StartNew(() =>
                {
                    _logger.Log(LogLevel.Info, String.Format("Started Summitted Payment Account Task. {0} to {1}", user.UserName, paymentAccount.Id));

                    SubmittedPaymentAccountTask task = new SubmittedPaymentAccountTask();
                    task.Excecute(paymentAccount.Id);

                }).ContinueWith(task =>
                {
                    _logger.Log(LogLevel.Info, String.Format("Completed Summitted Payment AccountTask. {0} to {1}", user.UserName, paymentAccount.Id));
                });

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

            if (paymentAccountGuid == null)
                throw new ArgumentException("Payment Account Specified is Invalid", id);

            var paymentAccount = _ctx.PaymentAccounts.FirstOrDefault(p => p.Id.Equals(paymentAccountGuid));

            if (paymentAccount == null)
                throw new ArgumentException("Payment Account Not Found", id);

            paymentAccount.AccountStatus = AccountStatusType.Deleted;
            paymentAccount.LastUpdatedDate = System.DateTime.Now;

            _ctx.SaveChanges();
        }
        public bool VerifyPaymentAccount(string userId, string paymentAccountId, double depositAmount1, double depositAmount2)
        {
            _logger.Log(LogLevel.Info, String.Format("Verifying Payment Account {0}:{1}", depositAmount1, depositAmount2));

            Guid paymentAccountGuid;
            Guid userGuid;

            Guid.TryParse(userId, out userGuid);
            Guid.TryParse(paymentAccountId, out paymentAccountGuid);

            if (userGuid == null)
                throw new ArgumentException(String.Format("Invalid User {0} Specified", userId));

            if (paymentAccountGuid == null)
                throw new ArgumentException(String.Format("Payment Account {0} is Invalid", paymentAccountId));

            using (var ctx = new Context())
            {
                var paymentAccountVerification = ctx.PaymentAccountVerifications.FirstOrDefault(p => p.PaymentAccountId == paymentAccountGuid
                    && (p.StatusValue.Equals((int)PaymentAccountVerificationStatus.Submitted) || p.StatusValue.Equals((int)PaymentAccountVerificationStatus.Delivered)));

                if (paymentAccountVerification == null)
                    throw new Exception(String.Format("Invalid Payment Account. Not Found"));

                if (paymentAccountVerification.PaymentAccountId != paymentAccountGuid)
                    throw new Exception(String.Format("Invalid Payment Account. Account Mismatch"));

                if (paymentAccountVerification.PaymentAccount.UserId != userGuid)
                    throw new Exception(String.Format("Invalid Payment Account.  User Mismatch"));

                if (paymentAccountVerification.DepositAmount1 != depositAmount1 || paymentAccountVerification.DepositAmount2 != depositAmount2)
                {
                    paymentAccountVerification.NumberOfFailures += 1;

                    ctx.SaveChanges();

                    return false;
                }

                paymentAccountVerification.Status = PaymentAccountVerificationStatus.Verified;
                paymentAccountVerification.VerificationDate = System.DateTime.UtcNow;
                paymentAccountVerification.PaymentAccount.AccountStatus = AccountStatusType.Verified;

                ctx.SaveChanges();

                return true;
            }
        }
    }
}
