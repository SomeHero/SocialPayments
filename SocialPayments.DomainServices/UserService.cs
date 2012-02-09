using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using NLog;

namespace SocialPayments.DomainServices
{
    public class UserService
    {
        private readonly Context _ctx = new Context();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public User AddUser(string userName, string password, string emailAddress, bool isLockedOut, string mobileNumber, string securityPin, 
            UserStatus userStatus, string accountNumber,
            PaymentAccountType accountType, string nameOnAccount, string routingNumber)
        {
            var user = _ctx.Users.Add(new User()
            {
                CreateDate = System.DateTime.Now,
                EmailAddress = emailAddress,
                //IsLockedOut = isLockedOut,
                //LastLoggedIn = System.DateTime.Now,
                MobileNumber = mobileNumber,
                Password = password,
                PaymentAccounts = new List<PaymentAccount> {
                    new PaymentAccount() { 
                        AccountNumber = accountNumber,
                        AccountType = accountType,
                        NameOnAccount =  nameOnAccount,
                        RoutingNumber = routingNumber
                    }
                },
                SecurityPin = securityPin,
                UserName = userName,
                UserStatus = userStatus,
            });

            _ctx.SaveChanges();

            return user;
        }
        public List<User> GetUsers()
        {
            return _ctx.Users
                .Include("PaymentAccounts")
                .Select(u => u).ToList<User>();
        }
        public User GetUser(Expression<Func<User, bool>> expression)
        {
            return _ctx.Users
                .Include("PaymentAccounts")
                .FirstOrDefault(expression);
        }
        public bool ConfirmUser(string accountConfirmationToken)
        {
            var confirmed = false;
            User user;

            logger.Log(LogLevel.Info, string.Format("Confirming User Registration with Email Token {0}. Starting.", accountConfirmationToken));

            if (string.IsNullOrEmpty(accountConfirmationToken))
            {
                logger.Log(LogLevel.Error, string.Format("Confirming User Registration with Email Token {0}. No confirmation token.", accountConfirmationToken));

                throw CreateArgumentNullOrEmptyException("accountConfirmationToken");
            }
            using (Context context = new Context())
            {
                user = _ctx.Users.FirstOrDefault(Usr => Usr.ConfirmationToken.Equals(accountConfirmationToken));

                if (user == null)
                {
                    logger.Log(LogLevel.Error, string.Format("Confirming User Registration with Email Token {0}. Unable to find user associated with account token.", accountConfirmationToken));

                    return false;
                }

                confirmed = true;
                user.IsConfirmed = true;
                _ctx.SaveChanges();
            }
            if (!confirmed)
                return false;

            logger.Log(LogLevel.Info, string.Format("Confirming User Registration with Email Token {0}. Calling Workflow.", accountConfirmationToken));


            try
            {
                logger.Log(LogLevel.Info, string.Format("Confirming User Registration with Email Token {0}. Calling Amazon SNS.", accountConfirmationToken));

                AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();

                client.Publish(new PublishRequest()
                {
                    Message = user.UserId.ToString(),
                    TopicArn = "arn:aws:sns:us-east-1:102476399870:UserNotifications",
                    Subject = "New User Registration"
                });
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Info, string.Format("Confirming User Registration with Email Token {0}. Exception {1}.", accountConfirmationToken, ex.Message));
                return false;
            }

            logger.Log(LogLevel.Info, string.Format("Confirming User Registration with Email Token {0}. Ending.", accountConfirmationToken));

            return true;
        }
        public void UpdateUser(Guid userId, string mobileNumber, string emailAddress, string password, string securityPin, bool isLockedOut, UserStatus userStatus)
        {
            var user = _ctx.Users.FirstOrDefault(u => u.UserId == userId);

            user.EmailAddress = emailAddress;
           // user.IsLockedOut = isLockedOut;
           // user.LastUpdatedDate = System.DateTime.Now;
            user.MobileNumber = mobileNumber;
            user.Password = password;
            //user.SecurityPin = securityPin;
           // user.UserStatus = userStatus;

            _ctx.SaveChanges();
        }
        public void DeleteUser(Guid userId)
        {
            var user = GetUser(u => u.UserId.Equals(userId));

            _ctx.Users.Remove(user);

            _ctx.SaveChanges();
        }
        public bool ValidateUser(string userNameOrEmail, string password, out User foundUser)
        {
            logger.Log(LogLevel.Info, "Validating User");

            CryptoService cryptoService = new CryptoService();
            if (string.IsNullOrEmpty(userNameOrEmail))
            {
                throw CreateArgumentNullOrEmptyException("userNameOrEmail");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw CreateArgumentNullOrEmptyException("password");
            }
            
            using (Context context = new Context())
            {
                User user = null;
                user = context.Users
                    .Include("PaymentAccounts")
                    .FirstOrDefault(Usr => Usr.UserName == userNameOrEmail);
                if (user == null)
                {
                    logger.Log(LogLevel.Warn, "Unable to find user by user name.");
                    user = context.Users
                        .Include("PaymentAccounts")
                        .FirstOrDefault(Usr => Usr.EmailAddress == userNameOrEmail);
                }
                if (user == null)
                {
                    logger.Log(LogLevel.Warn, "Unable to find user by user name.");
                    foundUser = null;
                    return false;
                }
                if (!user.IsConfirmed)
                {
                    foundUser = null;
                    return false;
                }
                dynamic hashedPassword = user.Password;
                logger.Log(LogLevel.Info, "Verifying Hashed Passwords");

                bool verificationSucceeded = false;
                
                try
                {
                    logger.Log(LogLevel.Info, string.Format("Passwords {0} {1}", hashedPassword, password));
                    verificationSucceeded = (hashedPassword != null && hashedPassword.Equals(password));
                        
                        //cryptoService.VerifyHashedPassword(hashedPassword, password));
                }
                catch(Exception ex)
                {
                    logger.Log(LogLevel.Info, String.Format("Exception Verifying Password Hash {0}", ex.Message));
                }
                
                logger.Log(LogLevel.Info, String.Format("Verifying Results {0}", verificationSucceeded.ToString()));

                if (verificationSucceeded)
                {
                    user.PasswordFailuresSinceLastSuccess = 0;
                }
                else
                {
                    int failures = user.PasswordFailuresSinceLastSuccess;
                    if (failures != -1)
                    {
                        user.PasswordFailuresSinceLastSuccess += 1;
                        user.LastPasswordFailureDate = DateTime.UtcNow;
                    }
                }
                context.SaveChanges();
                if (verificationSucceeded)
                {
                    foundUser = user;
                    return true;
                }
                else
                {
                    foundUser = null;
                    return false;
                }
            }
        }
        private ArgumentException CreateArgumentNullOrEmptyException(string paramName)
        {
            return new ArgumentException(string.Format("Argument cannot be null or empty: {0}", paramName));
        }
        public void ProcessUser(Guid userId)
        {
            AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();

            var user = _ctx.Users.FirstOrDefault(u => u.UserId == userId);

            var payments = _ctx.Payments.Select(p => p.ToMobileNumber.Equals(user.MobileNumber)) as IQueryable<Payment>;

            foreach (var payment in payments)
            {
                payment.FromAccount = user.PaymentAccounts[0];

                _ctx.SaveChanges();

                client.Publish(new PublishRequest()
                {
                    Message = payment.Id.ToString(),
                    TopicArn = "arn:aws:sns:us-east-1:102476399870:SocialPaymentNotifications",
                    Subject = "New Payment Receivied"
                });
            }


        }

        public User FindUserByMobileNumber(string mobileNumber)
        {
            return _ctx.Users
                .Include("PaymentAccounts")
                .FirstOrDefault(u => u.MobileNumber == mobileNumber);
        }


    }
}
