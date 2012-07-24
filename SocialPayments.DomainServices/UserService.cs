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
using System.Configuration;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using SocialPayments.DataLayer.Interfaces;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace SocialPayments.DomainServices
{
    public class UserService
    {
        private IDbContext _ctx;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private SecurityService securityService = new SecurityService();
        private DomainServices.FormattingServices formattingServices = new DomainServices.FormattingServices();
        private DomainServices.ValidationService _validationService = new DomainServices.ValidationService();

        int defaultNumPasswordFailures = 3;
        int defaultUpperLimit = Convert.ToInt32(ConfigurationManager.AppSettings["InitialPaymentLimit"]);
        private string _fbImageUrlFormat = "http://graph.facebook.com/{0}/picture";
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public UserService() { }

        public UserService(IDbContext context)
        {
            _ctx = context;
        }
        public User AddUser(Guid apiKey, string userName, string password, string emailAddress, string deviceToken)
        {
            return AddUser(apiKey, userName, password, emailAddress, deviceToken, "");
        }
        public User AddUser(Guid apiKey, string userName, string password, string emailAddress, string deviceToken,
            string mobileNumber)
        {
            var memberRole = _ctx.Roles.FirstOrDefault(r => r.RoleName == "Member");

            mobileNumber = formattingServices.RemoveFormattingFromMobileNumber(mobileNumber);

            var user = _ctx.Users.Add(new Domain.User()
            {
                UserId = Guid.NewGuid(),
                ApiKey = apiKey,
                CreateDate = System.DateTime.Now,
                PasswordChangedDate = DateTime.UtcNow,
                PasswordFailuresSinceLastSuccess = 0,
                EmailAddress = emailAddress,
                //IsLockedOut = isLockedOut,
                //LastLoggedIn = System.DateTime.Now,
                MobileNumber = (!String.IsNullOrEmpty(mobileNumber) ? mobileNumber : null),
                Password = securityService.Encrypt(password), //hash
                UserName = userName,
                UserStatus = Domain.UserStatus.Submitted,
                IsConfirmed = false,
                LastLoggedIn = System.DateTime.Now,
                Limit = Convert.ToDouble(defaultUpperLimit),
                RegistrationMethod = Domain.UserRegistrationMethod.MobilePhone,
                SetupPassword = true,
                SetupSecurityPin = false,
                Roles = new Collection<Role>()
                    {
                        memberRole
                    },
                DeviceToken = (!String.IsNullOrEmpty(deviceToken) ? deviceToken : null)
            });

            var messages = _ctx.Messages
                .Where(m => (m.RecipientUri == user.EmailAddress || m.RecipientUri == user.MobileNumber)
                    && m.StatusValue.Equals((int)PaystreamMessageStatus.Processing))
                    .ToList();

            foreach (var message in messages)
            {
                message.Recipient = user;
            }

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
        public User GetUser(string userUri)
        {
            _logger.Log(LogLevel.Debug, String.Format("Find User {0}", userUri));

            var uriType = GetURIType(userUri);

            _logger.Log(LogLevel.Debug, String.Format("Find User {0} with UriType {1}", userUri, uriType));

            User user = null;

            try
            {
                switch (uriType)
                {
                    case URIType.FacebookAccount:
                        user = _ctx.Users
                            .FirstOrDefault(u => u.UserName.Equals(userUri));

                        return user;

                    case URIType.MECode:
                        var meCode = _ctx.MECodes
                            .Include("User")
                            .FirstOrDefault(m => m.MeCode.Equals(userUri));

                        if (meCode == null)
                            return null;

                        user = meCode.User;

                        return user;

                    case URIType.EmailAddress:
                        user = _ctx.Users
                            .FirstOrDefault(u => u.EmailAddress == userUri);

                        return user;

                    case URIType.MobileNumber:
                        var phoneNumber = formattingServices.RemoveFormattingFromMobileNumber(userUri);

                        user = _ctx.Users
                            .FirstOrDefault(u => u.MobileNumber == phoneNumber);

                        return user;




                }

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Getting User {0}", userUri));
            }

            return null;
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
            using (Context ctx = new Context())
            {
                user = ctx.Users.FirstOrDefault(Usr => Usr.ConfirmationToken.Equals(accountConfirmationToken));

                if (user == null)
                {
                    logger.Log(LogLevel.Error, string.Format("Confirming User Registration with Email Token {0}. Unable to find user associated with account token.", accountConfirmationToken));

                    return false;
                }

                confirmed = true;
                user.IsConfirmed = true;
                ctx.SaveChanges();
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
                    TopicArn = ConfigurationManager.AppSettings["UserPostedTopicARN"],
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

        public void UpdateUser(User user)
        {
            if (!String.IsNullOrEmpty(user.MobileNumber))
                user.MobileNumber = formattingServices.RemoveFormattingFromMobileNumber(user.MobileNumber);

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

            User user = null;

            user = _ctx.Users
                .Include("PaymentAccounts")
                .FirstOrDefault(Usr => Usr.UserName == userNameOrEmail);

            if (user == null)
            {
                logger.Log(LogLevel.Warn, "Unable to find user by user name. Check email address.");
                user = _ctx.Users
                    .Include("PaymentAccounts")
                    .FirstOrDefault(Usr => Usr.EmailAddress == userNameOrEmail);
            }
            if (user == null)
            {
                logger.Log(LogLevel.Warn, "Unable to find user by email address. Check mobile number.");
                user = _ctx.Users
                    .Include("PaymentAccounts")
                    .FirstOrDefault(Usr => Usr.MobileNumber == userNameOrEmail);
            }
            if (user == null)
            {
                logger.Log(LogLevel.Warn, "Unable to find user by user name.");
                foundUser = null;
                return false;
            }
            //if (!user.IsConfirmed)
            //{
            //    foundUser = null;
            //    return false;
            //}
            var hashedPassword = securityService.Encrypt(password);
            logger.Log(LogLevel.Info, "Verifying Hashed Passwords");

            bool verificationSucceeded = false;

            try
            {
                logger.Log(LogLevel.Info, string.Format("Passwords {0} {1}", user.Password, hashedPassword));
                verificationSucceeded = (hashedPassword != null && hashedPassword.Equals(user.Password));

            }
            catch (Exception ex)
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
            _ctx.SaveChanges();

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
        public User SetupSecurityPin(string userId, string securityPin)
        {
            var user = GetUserById(userId);

            if (user == null)
            {
                var error = @"User Not Found";

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Pin for {0}. {1}", userId, error));

                throw new ArgumentException(String.Format("User {0} Not Found", userId), "userId");
            }

            user.SecurityPin = securityService.Encrypt(securityPin);
            user.SetupSecurityPin = true;

            _ctx.SaveChanges();

            return user;
        }

        public User SetupSecurityQuestion(string userId, int questionId, string questionAnswer)
        {
            var user = GetUserById(userId);

            if (user == null)
            {
                var error = @"User Not Found";

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Question for {0}. {1}", userId, error));

                throw new ArgumentException(String.Format("User {0} Not Found", userId), "userId");
            }

            user.SecurityQuestionID = questionId;
            user.SecurityQuestionAnswer = securityService.Encrypt(questionAnswer);

            _ctx.SaveChanges();

            return user;
        }

        public User AddPushNotificationRegistrationId(string userId, string newDeviceToken, string registrationId)
        {
            var user = GetUserById(userId);

            if (user == null)
            {
                var error = @"User Not Found";

                _logger.Log(LogLevel.Error, String.Format("Unable to add Android Push Notification Registration Id for {0}. {1}", userId, error));

                throw new ArgumentException(String.Format("User {0} Not Found", userId), "userId");
            }

            user.RegistrationId = registrationId;

            _ctx.SaveChanges();

            return user;
        }

        public User GetUserById(string userId)
        {
            Guid userIdGuid;

            Guid.TryParse(userId, out userIdGuid);

            if (userIdGuid == null)
                throw new ArgumentException(String.Format("UserId {0} Not Valid.", userId));

            var user = _ctx.Users
                .FirstOrDefault(u => u.UserId.Equals(userIdGuid));

            return user;
        }
        public User SignInWithFacebook(Guid apiKey, string accountId, string emailAddress, string firstName, string lastName,
            string deviceToken, string oAuthToken, DateTime tokenExpiration, out bool isNewUser)
        {
            _logger.Log(LogLevel.Info, String.Format("Sign in with Facebook {0}: emailAddress: {1} and token {2}", accountId, emailAddress, oAuthToken));

            User user = null;

            var memberRole = _ctx.Roles.FirstOrDefault(r => r.RoleName == "Member");

            try
            {
                user = _ctx.Users
                    .FirstOrDefault(u => u.FacebookUser.FBUserID.Equals(accountId));

                if (user == null)
                {
                    _logger.Log(LogLevel.Info, String.Format("Unable to find user with Facebook account {0}: Email Address {1}.  Create new user.", accountId, emailAddress));

                    isNewUser = true;

                    user = _ctx.Users.Add(new Domain.User()
                    {
                        UserId = Guid.NewGuid(),
                        ApiKey = apiKey,
                        CreateDate = System.DateTime.Now,
                        PasswordChangedDate = DateTime.UtcNow,
                        PasswordFailuresSinceLastSuccess = 3,
                        LastPasswordFailureDate = DateTime.UtcNow,
                        EmailAddress = emailAddress,
                        //IsLockedOut = isLockedOut,
                        //LastLoggedIn = System.DateTime.Now,
                        UserName = "fb_" + accountId,
                        UserStatus = Domain.UserStatus.Submitted,
                        IsConfirmed = false,
                        LastLoggedIn = System.DateTime.Now,
                        Limit = Convert.ToDouble(ConfigurationManager.AppSettings["InitialPaymentLimit"]),
                        RegistrationMethod = Domain.UserRegistrationMethod.MobilePhone,
                        Password = "tempPassword",
                        SetupPassword = false,
                        SetupSecurityPin = false,
                        Roles = new Collection<Role>()
                        {
                            memberRole
                        },
                        DeviceToken = deviceToken,
                        FirstName = firstName,
                        LastName = lastName,
                        PaymentAccounts = new Collection<PaymentAccount>(),
                        FacebookUser = new FBUser()
                        {
                            FBUserID = accountId,
                            Id = Guid.NewGuid(),
                            TokenExpiration = tokenExpiration,
                            OAuthToken = oAuthToken
                        },
                        ImageUrl = String.Format(_fbImageUrlFormat, accountId),
                    });

                }
                else
                {
                    isNewUser = false;

                    user.DeviceToken = deviceToken;
                    user.ImageUrl = String.Format(_fbImageUrlFormat, accountId);
                    if (user.FacebookUser != null)
                    {
                        user.FacebookUser.OAuthToken = oAuthToken;
                        //user.FacebookUser.TokenExpiration = tokenExpiration;
                    }
                    else
                    {
                        user.FacebookUser = new FBUser()
                        {
                            FBUserID = accountId,
                            Id = Guid.NewGuid(),
                            // TokenExpiration = tokenExpiration,
                            OAuthToken = oAuthToken
                        };
                    }
                }

                _ctx.SaveChanges();

            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                    }
                }

                throw dbEx;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Fatal, String.Format("Exception Signing in with Facebook. {0}", ex.Message));

                throw ex;
            }

            return user;
        }

        public User ResetPassword(string userId, string newPassword)
        {
            var user = GetUserById(userId);

            if (user == null)
            {
                var error = @"User Not Found";

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Pin for {0}. {1}", userId, error));

                throw new ArgumentException(String.Format("User {0} Not Found", userId), "userId");
            }

            user.Password = securityService.Encrypt(newPassword);
            UpdateUser(user);

            return user;
        }

        public User ResetPassword(string userId, string securityQuestionAnswer, string newPassword)
        {
            var user = GetUserById(userId);

            if (user == null)
            {
                var error = @"User Not Found";

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Pin for {0}. {1}", userId, error));

                throw new ArgumentException(String.Format("User {0} Not Found", userId), "userId");
            }

            if (!securityQuestionAnswer.Equals(securityService.Decrypt(user.SecurityQuestionAnswer)))
            {
                var error = @"Security Question Incorrect";

                _logger.Log(LogLevel.Error, String.Format("Unable to reset password for {0}. {1}", userId, error));

                throw new ArgumentException("Incorrect SecurityQuestion", "securityQuestionAnswer");
            }

            user.Password = securityService.Encrypt(newPassword);
            UpdateUser(user);

            return user;
        }

        public void SendResetPasswordLink(User user)
        {
            SendResetPasswordLink(user, new Guid("bda11d91-7ade-4da1-855d-24adfe39d174"));
        }

        public void SendResetPasswordLink(User user, Guid ApiKey)
        {
            DomainServices.EmailService emailService = new DomainServices.EmailService(_ctx);
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            DomainServices.ValidationService validateService = new ValidationService();

            if (!validateService.IsEmailAddress(user.UserName))
            {
                var message = "Facebook accounts cannot reset their password. Sign in with Facebook to continue";
                
                throw new ArgumentException(message);
            }

            string name = formattingService.FormatUserName(user);
            Guid passwordResetGuid = Guid.NewGuid();
            DateTime expiresDate = System.DateTime.Now.AddHours(3);

            PasswordResetAttempt passwordResetDb = _ctx.PasswordResetAttempts.Add(new PasswordResetAttempt()
            {
                Clicked = false,
                User = user,
                ExpiresDate = expiresDate,
                Id = passwordResetGuid
            });

            _ctx.SaveChanges();

            string link = String.Format("{0}reset_password/{1}", ConfigurationManager.AppSettings["MobileWebSetURL"], passwordResetGuid);

            StringBuilder body = new StringBuilder();
            body.AppendFormat("Dear {0}", name).AppendLine().AppendLine();
            body.Append("You asked us to reset your PaidThx password. ");
            body.Append("To complete the process, please click on the link below ");
            body.Append("or paste it into your browser:").AppendLine().AppendLine();
            body.AppendLine(link).AppendLine();
            body.AppendLine("This link will be active for 3 hours only.").AppendLine();
            body.AppendLine("Thank you,").AppendLine();
            body.Append("The PaidThx Team");

            emailService.SendEmail(ApiKey, ConfigurationManager.AppSettings["fromEmailAddress"], user.EmailAddress, "How to reset your PaidThx password", body.ToString());
        }

        private ArgumentException CreateArgumentNullOrEmptyException(string paramName)
        {
            return new ArgumentException(string.Format("Argument cannot be null or empty: {0}", paramName));
        }

        public User FindUserByMobileNumber(string mobileNumber)
        {
            var tempMobileNumber = formattingServices.RemoveFormattingFromMobileNumber(mobileNumber);

            return _ctx.Users
                .Include("PaymentAccounts")
                .FirstOrDefault(u => u.MobileNumber == tempMobileNumber);
        }


        public User FindUserByEmailAddress(string emailAddress)
        {
            return _ctx.Users
                .Include("PaymentAccounts")
                .FirstOrDefault(u => u.UserName == emailAddress || u.EmailAddress == emailAddress);
        }
        public double GetUserInstantLimit(User User)
        {
            var timeToCheck = System.DateTime.Now.AddHours(-24);

            var verifiedPaymentAmounts = _ctx.Messages
                .Where(m => m.SenderId.Equals(User.UserId) && m.MessageTypeValue.Equals((int)MessageType.Payment) && m.CreateDate > timeToCheck && m.Payment.PaymentVerificationLevelValue.Equals((int)PaymentVerificationLevel.Verified));

            double amountSent = 0;

            _logger.Log(LogLevel.Debug, String.Format("Getting Verified Limi {0}", verifiedPaymentAmounts.Count()));

            if (verifiedPaymentAmounts.Count() > 0)
                amountSent = verifiedPaymentAmounts.Sum(m => m.Amount);

            if (100 - amountSent > 0)
                return 100 - amountSent;
            else
                return 0;
        }
        public string GetSenderName(User sender)
        {
            _logger.Log(LogLevel.Debug, String.Format("Getting UserName {0}", sender.UserId));

            if (!String.IsNullOrEmpty(sender.FirstName) || !String.IsNullOrEmpty(sender.LastName))
                return sender.FirstName + " " + sender.LastName;

            if (!String.IsNullOrEmpty(sender.SenderName))
                return sender.SenderName;

            if (!String.IsNullOrEmpty(sender.MobileNumber))
                return formattingServices.FormatMobileNumber(sender.MobileNumber);

            if (!String.IsNullOrEmpty(sender.EmailAddress))
                return sender.EmailAddress;

            return "PaidThx User";


        }
        public URIType GetURIType(string uri)
        {
            var uriType = URIType.MobileNumber;

            if (_validationService.IsEmailAddress(uri))
                uriType = URIType.EmailAddress;
            else if (_validationService.IsMECode(uri))
                uriType = URIType.MECode;
            else if (_validationService.IsFacebookAccount(uri))
                uriType = URIType.FacebookAccount;

            return uriType;
        }
    }
}
