using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SocialPayments.Services.ServiceContracts;
using SocialPayments.Services.DataContracts.PaymentAccount;
using SocialPayments.DataLayer;
using NLog;
using System.ServiceModel.Activation;
using SocialPayments.DomainServices;

namespace SocialPayments.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PaymentAccountService : IPaymentAccountService
    {
        private Context _ctx = new Context();

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SecurityService securityService = new SecurityService();
        
        public PaymentAccountReponse AddPaymentAccount(PaymentAccountRequest request)
        {
            logger.Log(LogLevel.Info, string.Format("Adding new payment account for user {0}", request.UserId));

            Domain.PaymentAccount paymentAccount;

            try
            {
                paymentAccount = _ctx.PaymentAccounts.Add(new Domain.PaymentAccount()
                {
                    Id = Guid.NewGuid(),
                    AccountNumber = securityService.Encrypt(request.AccountNumber),
                    NameOnAccount = securityService.Encrypt(request.NameOnAccount),
                    AccountType = Domain.PaymentAccountType.Checking,
                    RoutingNumber = securityService.Encrypt(request.RoutingNumber),
                    UserId = new Guid(request.UserId),
                });

                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, string.Format("Adding new payment account for user {0}. Exception {1}.", request.UserId, ex.Message));

                throw ex;
            }

            return new PaymentAccountReponse()
            {
                Id = paymentAccount.Id.ToString(),
                AccountNumber = paymentAccount.AccountNumber,
                AccountType = "Checking",
                NameOnAccount = paymentAccount.NameOnAccount,
                RoutingNumber = paymentAccount.RoutingNumber,
            };

        }

        public List<PaymentAccountReponse> GetPaymentAccounts()
        {
            throw new NotImplementedException();
        }

        public PaymentAccountReponse GetPaymentAccount(string id)
        {
            return new PaymentAccountReponse()
            {
                AccountNumber = "123412342",
                AccountType = "Checking",
                Id = "1",
                NameOnAccount = "Test USer",
                RoutingNumber = "123123123"
            };
        }

        public void UpdatePayment(PaymentAccountRequest request)
        {
            throw new NotImplementedException();
        }

        public void DeletePayment(PaymentAccountRequest request)
        {
            throw new NotImplementedException();
        }
    }
}