﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using System.Data.Entity;
using System.Threading.Tasks;
using NLog;
using System.Configuration;

namespace SocialPayments.DomainServices
{
    public class UserPaymentAccountServices
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        
        public Domain.PaymentAccount AddPaymentAccount(string userId, string nickName, string bankIconUrl, string nameOnAccount, string routingNumber,
            string accountNumber, string accountTypeName, string securityPin, int securityQuestionId, string securityQuestionAnswer)
        {
            _logger.Log(LogLevel.Info, String.Format("Start Add Payment Account for User {0}", userId));

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

                bool isRoutingNumberValid = paymentAccountService.VerifyRoutingNumber(routingNumber);

                if (!isRoutingNumberValid)
                    throw new CustomExceptions.BadRequestException(String.Format("Invalid Routing Number.  Please Check the Number and Try Again"));

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

                Task.Factory.StartNew(() =>
                {
                    _logger.Log(LogLevel.Info, String.Format("Started Summitted Payment Account Task. {0} to {1}", user.UserName, paymentAccount.Id));

                    DomainServices.PaymentAccountProcessing.SubmittedPaymentAccountTask task = new DomainServices.PaymentAccountProcessing.SubmittedPaymentAccountTask();
                    task.Excecute(paymentAccount.Id);

                }).ContinueWith(task =>
                {
                    _logger.Log(LogLevel.Info, String.Format("Completed Summitted Payment AccountTask. {0} to {1}", user.UserName, paymentAccount.Id));
                });

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

                Domain.PaymentAccountType accountType = Domain.PaymentAccountType.Checking;

                if (accountTypeName.ToUpper() == "CHECKING")
                    accountType = Domain.PaymentAccountType.Checking;
                else if (accountTypeName.ToUpper() == "SAVINGS")
                    accountType = Domain.PaymentAccountType.Savings;

                var user = userService.GetUserById(userId);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                var encryptedRoutingNumber = securityServices.Encrypt(routingNumber);
                var encryptedAccountNumber =  securityServices.Encrypt(accountNumber);

                //check to see if user has already added an payment account with matching routing #, account # and account type
                var duplicateAccount = ctx.PaymentAccounts
                    .FirstOrDefault(p => p.RoutingNumber == encryptedRoutingNumber &&
                        p.AccountNumber == encryptedAccountNumber
                        && p.PaymentAccountTypeId.Equals((int)accountType)
                        && p.UserId == user.UserId
                        && p.IsActive);

                if (duplicateAccount != null)
                    throw new CustomExceptions.BadRequestException("You have already created this payment account");

                if (!securityServices.Encrypt(securityPin).Equals(user.SecurityPin))
                {
                    user.PinCodeFailuresSinceLastSuccess += 1;

                    if (user.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;
                        ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid. Sender {0} is Locked out", user.UserId), 1001);
                    }

                    ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid."));
                }

                bool isRoutingNumberValid = paymentAccountService.VerifyRoutingNumber(routingNumber);

                if (!isRoutingNumberValid)
                    throw new CustomExceptions.BadRequestException(String.Format("Invalid Routing Number.  Please Check the Number and Try Again"));

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

                Task.Factory.StartNew(() =>
                {
                    _logger.Log(LogLevel.Info, String.Format("Started Summitted Payment Account Task. {0} to {1}", user.UserName, paymentAccount.Id));

                    DomainServices.PaymentAccountProcessing.SubmittedPaymentAccountTask task = new DomainServices.PaymentAccountProcessing.SubmittedPaymentAccountTask();
                    task.Excecute(paymentAccount.Id);

                }).ContinueWith(task =>
                {
                    _logger.Log(LogLevel.Info, String.Format("Completed Summitted Payment AccountTask. {0} to {1}", user.UserName, paymentAccount.Id));
                });

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

                if (!securityService.Encrypt(securityPin).Equals(user.SecurityPin))
                {
                    user.PinCodeFailuresSinceLastSuccess += 1;

                    if (user.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;
                        _ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid. Sender {0} is Locked out", user.UserId), 1001);
                    }

                    _ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid."));
                }

                user.PreferredSendAccount = paymentAccount;

                _ctx.SaveChanges();

            }
        }
        public void  SetPreferredReceiveAccount(string userId, string paymentAccountId, string securityPin)
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

                if (!securityService.Encrypt(securityPin).Equals(user.SecurityPin))
                {
                    user.PinCodeFailuresSinceLastSuccess += 1;

                    if (user.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;
                        _ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid. Sender {0} is Locked out", user.UserId), 1001);
                    }

                    _ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid."));
                }

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
        public void UpdatePaymentAccount(string userId, string paymentAccountId, string nickName, string nameOnAccount, string routingNumber, string accountTypeName,
            string securityPin)
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

                var userAccount = _ctx.PaymentAccounts.FirstOrDefault(p => p.Id == paymentAccountGuid && p.UserId == user.UserId);

                if (userAccount == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Payment Account {0} Not Found", paymentAccountId));


                if (!_securityService.Encrypt(securityPin).Equals(user.SecurityPin))
                {
                    user.PinCodeFailuresSinceLastSuccess += 1;

                    if (user.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;
                        _ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid. Sender {0} is Locked out", user.UserId), 1001);
                    }

                    _ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid."));
                }
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
        public void DeletePaymentAccount(string userId, string paymentAccountId, string securityPin)
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


                if (!_securityService.Encrypt(securityPin).Equals(user.SecurityPin))
                {
                    user.PinCodeFailuresSinceLastSuccess += 1;

                    if (user.PinCodeFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;
                        _ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid. Sender {0} is Locked out", user.UserId), 1001);
                    }

                    _ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Security Pin Invalid."));
                }
                var nextOldestBankAccount = user.PaymentAccounts
                    .Where(p => p.Id != userAccount.Id && p.IsActive)
                    .OrderByDescending(p => p.CreateDate);

                if (userAccount.Id == user.PreferredSendAccount.Id)
                {
                    if(nextOldestBankAccount == null)
                        user.PreferredSendAccount = null;
                    else
                        user.PreferredSendAccount = nextOldestBankAccount.FirstOrDefault();
                }
                if (userAccount.Id == user.PreferredReceiveAccount.Id)
                {
                    if (nextOldestBankAccount == null)
                        user.PreferredReceiveAccount = null;
                    else 
                        user.PreferredReceiveAccount = nextOldestBankAccount.FirstOrDefault();
                }
                userAccount.IsActive = false;
                userAccount.LastUpdatedDate = System.DateTime.Now;

                _ctx.SaveChanges();
            }
        }

    }
}
