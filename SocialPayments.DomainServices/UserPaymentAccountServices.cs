using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using System.Data.Entity;

namespace SocialPayments.DomainServices
{
    public class UserPaymentAccountServices
    {
        public Domain.PaymentAccount AddPaymentAccount(string userId, string nickName, string bankIconUrl, string nameOnAccount, string routingNumber,
            string accountNumber, string accountTypeName, string securityPin, int securityQuestionId, string securityQuestionAnswer)
        {
            using (var ctx = new Context())
            {
                var securityServices = new DomainServices.SecurityService();
                var userService = new DomainServices.UserService(ctx);
                var paymentAccountService = new DomainServices.PaymentAccountService(ctx);
                Domain.PaymentAccount paymentAccount = null;

                var user = userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                if (String.IsNullOrEmpty(securityQuestionAnswer))
                    throw new CustomExceptions.BadRequestException("Security Question Answer is Required");

                //TODO: validate routing number

                Domain.PaymentAccountType accountType = Domain.PaymentAccountType.Checking;

                if (accountTypeName.ToUpper() == "CHECKING")
                    accountType = Domain.PaymentAccountType.Checking;
                else if (accountTypeName.ToUpper() == "SAVINGS")
                    accountType = Domain.PaymentAccountType.Savings;


                paymentAccount = ctx.PaymentAccounts.Add(new Domain.PaymentAccount()
                {
                    Id = Guid.NewGuid(),
                    UserId = user.UserId,
                    NameOnAccount = securityServices.Encrypt(nameOnAccount),
                    RoutingNumber = securityServices.Encrypt(routingNumber),
                    AccountNumber = securityServices.Encrypt(accountNumber),
                    AccountStatus = Domain.AccountStatusType.Submitted,
                    AccountType = accountType,
                    CreateDate = System.DateTime.UtcNow,
                    BankName = "",
                    BankIconURL = bankIconUrl,
                    Nickname = nickName,
                    IsActive = true
                });

                if (user.PreferredReceiveAccount == null)
                    user.PreferredReceiveAccount = paymentAccount;
                if (user.PreferredSendAccount == null)
                    user.PreferredSendAccount = paymentAccount;

                user.SecurityPin = securityServices.Encrypt(securityPin);
                user.SecurityQuestionID = securityQuestionId;
                user.SecurityQuestionAnswer = securityServices.Encrypt(securityQuestionAnswer);

                user.SetupSecurityPin = true;

                ctx.SaveChanges();

                return paymentAccount;
            }
        }
        public Domain.PaymentAccount AddPaymentAccount(string userId, string nickName, string bankIconUrl, string nameOnAccount, string routingNumber,
            string accountNumber, string accountTypeName, string securityPin)
        {
            using (var ctx = new Context())
            {
                var securityServices = new DomainServices.SecurityService();
                var userService = new DomainServices.UserService(ctx);
                var paymentAccountService = new DomainServices.PaymentAccountService(ctx);
                Domain.PaymentAccount paymentAccount = null;

                var user = userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                if (user.SecurityPin != securityServices.Encrypt(securityPin))
                    throw new CustomExceptions.BadRequestException("Invalid Security Pin");

                //TODO: validate routing number

                Domain.PaymentAccountType accountType = Domain.PaymentAccountType.Checking;

                if (accountTypeName.ToUpper() == "CHECKING")
                    accountType = Domain.PaymentAccountType.Checking;
                else if (accountTypeName.ToUpper() == "SAVINGS")
                    accountType = Domain.PaymentAccountType.Savings;

                if (nickName == null || nickName == "")
                {
                    nickName = accountTypeName + " " + securityServices.GetLastFour(accountNumber);
                }

                paymentAccount = ctx.PaymentAccounts.Add(new Domain.PaymentAccount()
                {
                    Id = Guid.NewGuid(),
                    UserId = user.UserId,
                    NameOnAccount = securityServices.Encrypt(nameOnAccount),
                    RoutingNumber = securityServices.Encrypt(routingNumber),
                    AccountNumber = securityServices.Encrypt(accountNumber),
                    AccountStatus = Domain.AccountStatusType.Submitted,
                    AccountType = accountType,
                    CreateDate = System.DateTime.UtcNow,
                    BankName = "",
                    BankIconURL = bankIconUrl,
                    Nickname = nickName,
                    IsActive = true
                });

                if (user.PreferredReceiveAccount == null)
                    user.PreferredReceiveAccount = paymentAccount;
                if (user.PreferredSendAccount == null)
                    user.PreferredSendAccount = paymentAccount;

                ctx.SaveChanges();

                return paymentAccount;
            }
        }
        public List<Domain.PaymentAccount> GetPaymentAccounts(string userId)
        {
            using (var _ctx = new Context())
            {
                DomainServices.UserService userService = new DomainServices.UserService(_ctx);

                var user = userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found"));

                var accounts = _ctx.PaymentAccounts
                    .Where(a => a.UserId == user.UserId && a.IsActive).ToList();

                return accounts;
            }
        }
        public Domain.PaymentAccount GetPaymentAccount(string userId, string paymentAccountId)
        {
            using (var _ctx = new Context())
            {
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

                var user = _userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found"));

                Guid paymentAccountGuid;

                Guid.TryParse(paymentAccountId, out paymentAccountGuid);

                if (paymentAccountGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment Account {0} Not Found", paymentAccountId));

                var account = _ctx.PaymentAccounts
                    .FirstOrDefault(a => a.Id == paymentAccountGuid && a.UserId == user.UserId && a.IsActive);

                return account;
            }
        }
        public void SetPreferredSendAccount(string userId, string paymentAccountId, string securityPin)
        {
            using (var _ctx = new Context())
            {
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService paymentAccountService = new DomainServices.PaymentAccountService(_ctx);
                DomainServices.SecurityService securityService = new DomainServices.SecurityService();

                var user = _userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                var paymentAccount = paymentAccountService.GetPaymentAccount(paymentAccountId);

                if (paymentAccount == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Payment Account {0} Not Found", paymentAccountId));

                if (!(securityService.Encrypt(securityPin).Equals(user.SecurityPin)))
                    throw new CustomExceptions.BadRequestException("Unable to Change Preferred Send Account. Security Pin Invalid");

                user.PreferredSendAccount = paymentAccount;

                _ctx.SaveChanges();

            }
        }
        public void SetPreferredReceiveAccount(string userId, string paymentAccountId, string securityPin)
        {
            using (var _ctx = new Context())
            {
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService paymentAccountService = new DomainServices.PaymentAccountService(_ctx);
                DomainServices.SecurityService securityService = new DomainServices.SecurityService();

                var user = _userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                var paymentAccount = paymentAccountService.GetPaymentAccount(paymentAccountId);

                if (paymentAccount == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Payment Account {0} Not Found", paymentAccountId));

                if (!(securityService.Encrypt(securityPin).Equals(user.SecurityPin)))
                    throw new CustomExceptions.BadRequestException("Unable to Change Preferred Send Account. Security Pin Invalid");

                user.PreferredReceiveAccount = paymentAccount;

                _ctx.SaveChanges();

            }
        }
        public bool VerifyAccount(string userId, string paymentAccountId, double depositAmount1, double depositAmount2)
        {
            using (var ctx = new Context())
            {
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
                DomainServices.UserService _userService = new DomainServices.UserService();
                DomainServices.PaymentAccountService verificationService =
                    new DomainServices.PaymentAccountService();
                Guid paymentAccountGuid;

                Guid.TryParse(paymentAccountId, out paymentAccountGuid);

                if (paymentAccountGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment Account {0} Not Found"));

                if (depositAmount1 == 0.0 || depositAmount2 == 0.0)
                    throw new CustomExceptions.BadRequestException(String.Format("Invalid Payment Amounts"));

                var paymentAccountVerification = ctx.PaymentAccountVerifications.FirstOrDefault(p => p.PaymentAccountId == paymentAccountGuid
                    && (p.StatusValue.Equals((int)Domain.PaymentAccountVerificationStatus.Submitted) || p.StatusValue.Equals((int)Domain.PaymentAccountVerificationStatus.Delivered)));

                if (paymentAccountVerification == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Invalid Payment Account. Not Found"));

                if (paymentAccountVerification.PaymentAccountId.ToString() != paymentAccountId)
                    throw new CustomExceptions.BadRequestException(String.Format("Invalid Payment Account. Account Mismatch"));

                if (paymentAccountVerification.PaymentAccount.UserId.ToString() != userId)
                    throw new CustomExceptions.BadRequestException(String.Format("Invalid Payment Account.  User Mismatch"));

                if (paymentAccountVerification.DepositAmount1 != depositAmount1 || paymentAccountVerification.DepositAmount2 != depositAmount2)
                {
                    paymentAccountVerification.NumberOfFailures += 1;

                    ctx.SaveChanges();

                    return false;
                }

                paymentAccountVerification.Status = Domain.PaymentAccountVerificationStatus.Verified;
                paymentAccountVerification.VerificationDate = System.DateTime.UtcNow;
                paymentAccountVerification.PaymentAccount.AccountStatus = Domain.AccountStatusType.Verified;

                ctx.SaveChanges();

                return true;
                
            }
        }
        public void UpdatePaymentAccount(string userId, string paymentAccountId, string nickName, string nameOnAccount, string routingNumber, string accountTypeName)
        {
            using (var _ctx = new Context())
            {
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
                Guid paymentAccountGuid;

                var user = _userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                Guid.TryParse(paymentAccountId, out paymentAccountGuid);

                if (paymentAccountGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment Account {0} Not Found", paymentAccountId));

                var userAccount = user.PaymentAccounts.FirstOrDefault(p => p.Id == paymentAccountGuid);

                if (userAccount == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment Account {0} Not Found", paymentAccountId));

                Domain.PaymentAccountType accountType = Domain.PaymentAccountType.Checking;

                if (accountTypeName.ToUpper() == "CHECKING")
                    accountType = Domain.PaymentAccountType.Checking;
                else if (accountTypeName.ToUpper() == "SAVINGS")
                    accountType = Domain.PaymentAccountType.Savings;

                userAccount.Nickname = nickName;
                userAccount.NameOnAccount = _securityService.Encrypt(nameOnAccount);
                userAccount.RoutingNumber = _securityService.Encrypt(routingNumber);
                userAccount.AccountType = accountType;
                userAccount.LastUpdatedDate = System.DateTime.Now;

                _ctx.SaveChanges();

            }
        }
        public void DeletePaymentAccount(string userId, string paymentAccountId)
        {
            using (var _ctx = new Context())
            {
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
                DomainServices.UserService _userService = new DomainServices.UserService();

                Guid userGuid;
                Guid.TryParse(userId, out userGuid);

                var user = _ctx.Users
                    .Include("PaymentAccounts")
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", user.UserId));

                Guid paymentAccountGuid;
                Guid.TryParse(paymentAccountId, out paymentAccountGuid);

                if (paymentAccountGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment Account {0} Not Found"));

                var userAccount = user.PaymentAccounts.FirstOrDefault(p => p.Id == paymentAccountGuid);

                if (userAccount == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment Account {0} Not Found", paymentAccountId));

                userAccount.IsActive = false;
                userAccount.LastUpdatedDate = System.DateTime.Now;

                _ctx.SaveChanges();
            }
        }

    }
}
