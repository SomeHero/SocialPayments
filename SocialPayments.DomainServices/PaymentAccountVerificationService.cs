using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SocialPayments.DataLayer.Interfaces;
using NLog;
using SocialPayments.Domain;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class PaymentAccountVerificationService
    {
        private IDbContext _ctx;
        private Logger _logger;
        private static readonly int _numberOfFailuresThreshold = 3;
       
        public PaymentAccountVerificationService() {
            _ctx = new Context();
            _logger = LogManager.GetCurrentClassLogger();
        }
        public PaymentAccountVerificationService(IDbContext context, Logger logger)
        {
            _ctx = context;
            _logger = logger;
        }
        public PaymentAccountVerification AddVerification(string userId, string paymentAccountId,
            double depositAmount1, double depositAmount2, double withdrawalAmount, DateTime sentDate,
            DateTime estimatedSettlementDate)
        {
             //validate that user is owned by paymentAccount 
            Guid userIdGuid;
            Guid paymentAccountIdGuid;

            Guid.TryParse(userId, out userIdGuid);

            if (userIdGuid == null)
                throw new ArgumentException("UserId is an Invalid GUID", "UserId");

            Guid.TryParse(paymentAccountId, out paymentAccountIdGuid);

            if (paymentAccountIdGuid == null)
                throw new ArgumentException("PaymentAccountId is an Invalid GUID", "PaymentAccountId");

            var paymentAccountVerification = _ctx.PaymentAccountVerifications.Add(new PaymentAccountVerification()
            {
                DepositAmount1 = depositAmount1,
                DepositAmount2 = depositAmount2,
                WithdrawalAmount = withdrawalAmount,
                EstimatedSettlementDate = estimatedSettlementDate,
                Id = Guid.NewGuid(),
                PaymentAccountId = paymentAccountIdGuid,
                Sent = sentDate,
                Status = PaymentAccountVerificationStatus.Submitted
            });

            return paymentAccountVerification;
        }
        public bool VerifyAccount(string userId, string paymentAccountId, double depositAmount1, double depositAmount2)
        {
            Guid userIdGuid;
            Guid paymentAccountIdGuid;

            Guid.TryParse(userId, out userIdGuid);

            if (userIdGuid == null)
                throw new ArgumentException("UserId is an Invalid GUID", "UserId");

            Guid.TryParse(paymentAccountId, out paymentAccountIdGuid);

            if (paymentAccountIdGuid == null)
                throw new ArgumentException("PaymentAccountId is an Invalid GUID", "PaymentAccountId");


            var paymentAccount = _ctx.PaymentAccounts
                .FirstOrDefault(a => a.UserId == userIdGuid && a.Id == paymentAccountIdGuid);

            if (paymentAccount == null)
                throw new Exception("Invalid payment account.  The payment account was not found or the user specified does not have the appropriate permissions to activate the account.");

            if(paymentAccount.AccountStatus != AccountStatusType.PendingActivation)
                throw new Exception("Invalid payment account.  The payment account specified is not pending an activation");

            var paymentAccountVerification = _ctx.PaymentAccountVerifications
                .FirstOrDefault(p => p.PaymentAccountId == paymentAccount.Id);

            if (paymentAccountVerification == null)
                throw new Exception("Invalid payment account.  A pending verification was not found for the specified payment account");

            if (paymentAccountVerification.DepositAmount1 == depositAmount1 && paymentAccountVerification.DepositAmount2 == depositAmount2)
            {
                paymentAccountVerification.VerificationDate = System.DateTime.Now;
                paymentAccountVerification.Status = PaymentAccountVerificationStatus.Verified;

                paymentAccount.AccountStatus = AccountStatusType.Verified;

                _ctx.SaveChanges();

                return true;
            }
            else
            {
                paymentAccountVerification.NumberOfFailures += 1;

                if (paymentAccountVerification.NumberOfFailures >= _numberOfFailuresThreshold)
                {
                    paymentAccountVerification.Status = PaymentAccountVerificationStatus.Failed;

                    paymentAccount.AccountStatus = AccountStatusType.NeedsReVerification;

                    _ctx.SaveChanges();

                    return false;
                }
            }

            return false;
        }
    }
}
