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
using SocialPayments.DomainServices.UserProcessing;
using System.Threading.Tasks;

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
        private string _mobileVerificationBaseUrl = ConfigurationManager.AppSettings["MobileNumberVerificationBaseUrl"];

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public UserService() : this(new Context()) { }

        public UserService(IDbContext context)
        {
            _ctx = context;
        }
        public User AddUser(Guid apiKey, string userName, string password, string emailAddress, string deviceToken)
        {
            return AddUser(apiKey, userName, password, emailAddress, deviceToken, "", "");
        }
        public User AddUser(Guid apiKey, string userName, string password, string emailAddress, string deviceToken,
            string mobileNumber, string messageId)
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
                PayPoints = new Collection<UserPayPoint>(),
                RegistrationMethod = Domain.UserRegistrationMethod.MobilePhone,
                SetupPassword = true,
                SetupSecurityPin = false,
                Roles = new Collection<Role>()
                    {
                        memberRole
                    },
                DeviceToken = (!String.IsNullOrEmpty(deviceToken) ? deviceToken : null)
            });

            Domain.PayPointType payPointType;

            payPointType = _ctx.PayPointTypes.FirstOrDefault(p => p.Name == @"EmailAddress");

            if (payPointType == null)
                throw new Exception("Pay Point Type Email Address Not Found");

            if (!String.IsNullOrEmpty(emailAddress))
            {
                user.PayPoints.Add(new UserPayPoint()
                {
                    Id = Guid.NewGuid(),
                    CreateDate = System.DateTime.Now,
                    IsActive = true,
                    URI = emailAddress,
                    Type = payPointType,
                    Verified = false
                });
            }

            if (!String.IsNullOrEmpty(mobileNumber))
            {

                payPointType = _ctx.PayPointTypes.FirstOrDefault(p => p.Name == @"Phone");

                if (payPointType == null)
                    throw new Exception("Pay Point Type Phone Not Found");

                user.PayPoints.Add(new UserPayPoint()
                {
                    CreateDate = System.DateTime.Now,
                    Id = Guid.NewGuid(),
                    Type = payPointType,
                    URI = userName,
                    User = user,
                    IsActive = true,
                    Verified = false
                });
            }
            var messages = _ctx.Messages
                .Where(m => (m.RecipientUri == user.EmailAddress || m.RecipientUri == user.MobileNumber)
                    && (m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedPayment) || m.StatusValue.Equals((int)PaystreamMessageStatus.NotifiedRequest)))
                    .ToList();

            foreach (var message in messages)
            {
                switch (message.MessageType)
                {
                    case MessageType.Payment:
                        message.Recipient = user;
                        message.Status = PaystreamMessageStatus.ProcessingPayment;

                        break;
                    case MessageType.PaymentRequest:
                        message.Recipient = user;
                        message.Status = PaystreamMessageStatus.PendingRequest;

                        break;
                    default:
                        message.Recipient = user;

                        break;
                }
            }

            _ctx.SaveChanges();

            if (!String.IsNullOrEmpty(messageId))
            {
                Guid messageGuid;

                Guid.TryParse(messageId, out messageGuid);

                var message = _ctx.Messages
                    .FirstOrDefault(m => m.Id == messageGuid);

                if (message.Recipient == null)
                {
                    message.Recipient = user;
                }
                switch (message.MessageType)
                {
                    case MessageType.Payment:
                        message.Status = PaystreamMessageStatus.ProcessingPayment;
                        break;
                    case MessageType.PaymentRequest:
                        message.Status = PaystreamMessageStatus.NotifiedRequest;
                        break;
                    case MessageType.Donation:
                        message.Status = PaystreamMessageStatus.ProcessingPayment;
                        break;
                }

                UserPayPoint messagePayPoint;

                switch (GetURIType(message.RecipientUri))
                {
                    case URIType.EmailAddress:
                        if (message.RecipientUri != user.EmailAddress)
                        {
                            messagePayPoint = _ctx.UserPayPoints.Add(new UserPayPoint()
                            {
                                CreateDate = System.DateTime.Now,
                                Id = Guid.NewGuid(),
                                PayPointTypeId = 1,
                                URI = message.RecipientUri,
                                User = user
                            });

                        }

                        break;
                    case URIType.MobileNumber:

                        if (message.RecipientUri != user.MobileNumber)
                        {
                            messagePayPoint = _ctx.UserPayPoints.Add(new UserPayPoint()
                            {
                                CreateDate = System.DateTime.Now,
                                Id = Guid.NewGuid(),
                                PayPointTypeId = 2,
                                URI = message.RecipientUri,
                                User = user
                            });

                        }
                        break;
                }


                _ctx.SaveChanges();

            }

            Task.Factory.StartNew(() =>
            {
                _logger.Log(LogLevel.Info, String.Format("Started Summitted User Task. {0}", user.UserName));

                SubmittedUserTask userTask = new SubmittedUserTask();
                userTask.Execute(user.UserId);

            }).ContinueWith(task =>
            {
                _logger.Log(LogLevel.Info, String.Format("Completed Summitted User Task. {0}", user.UserName));
            });
            return user;
        }
        public void ChangePassword(string userId, string currentPassword, string newPassword)
        {
            using (var ctx = new Context())
            {
                Guid userGuid;

                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                if (!securityService.Encrypt(currentPassword).Equals(user.Password))
                {
                    user.PasswordFailuresSinceLastSuccess += 1;

                    if (user.PasswordFailuresSinceLastSuccess > 2)
                    {
                        user.IsLockedOut = true;
                        ctx.SaveChanges();

                        throw new CustomExceptions.BadRequestException(String.Format("Password Invalid. Userr {0} is Locked out", user.UserId), 1001);
                    }
                    ctx.SaveChanges();

                    throw new CustomExceptions.BadRequestException(String.Format("Current Password Does Not Match Our Records. Try Again."));
                }

                if (securityService.Decrypt(user.Password).Equals(newPassword))
                    throw new CustomExceptions.BadRequestException(String.Format("New Password Can't Match Your Current Password. Try Again."));

                user.Password = securityService.Encrypt(newPassword);

                ctx.SaveChanges();
            }
        }
        public List<User> GetUsers()
        {
            return _ctx.Users
                .Include("PaymentAccounts")
                .Select(u => u).ToList<User>();
        }
        public List<Domain.User> GetPagedUsers(int take, int skip, int page, int pageSize, out int totalRecords)
        {
            using (var ctx = new Context())
            {
                totalRecords = ctx.Messages.Count();

                var users = ctx.Users.Select(u => u)
                    .OrderBy(m => m.CreateDate)
                    .Skip(skip)
                    .Take(take)
                    .ToList();

                return users;
            }
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

            switch (uriType)
            {
                case URIType.FacebookAccount:
                    user = _ctx.Users
                        .FirstOrDefault(u => u.UserName.Equals(userUri));

                    break;

                case URIType.MECode:
                    var meCode = _ctx.UserPayPoints.FirstOrDefault(m => m.URI.Equals(userUri));

                    user = meCode.User;
                    break;
                case URIType.EmailAddress:
                    user = _ctx.Users
                        .FirstOrDefault(u => u.EmailAddress.Equals(userUri));
                    break;

                case URIType.MobileNumber:
                    var phoneNumber = formattingServices.RemoveFormattingFromMobileNumber(userUri);

                    user = _ctx.Users
                        .FirstOrDefault(u => u.MobileNumber == phoneNumber);
                    break;
            }

            return user;

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

        public List<Domain.UserPayPoint> FindTopMatchingMeCodes(string searchTerm, string type)
        {
            using (var ctx = new Context())
            {
                List<Domain.UserPayPoint> meCodesFound = new List<Domain.UserPayPoint>();

                int meCodeTypeInt = _ctx.PayPointTypes
                    .First(m => m.Name.Equals("MeCode")).Id;

                // Takes the top 20 found matches.
                if (!String.IsNullOrEmpty(type) && (!(type.ToLower() == "non-profits" || type.ToLower() == "organizations")))
                    throw new CustomExceptions.BadRequestException("If type is specified, type must be Non-Profits or Organizations");

                if (!String.IsNullOrEmpty(type) && type.ToLower() == "non-profits")
                {
                    meCodesFound = _ctx.UserPayPoints.Select(m => m)
                       .Where(m => m.PayPointTypeId == meCodeTypeInt && m.URI.Contains(searchTerm) && m.User.UserTypeId.Equals((int)Domain.UserType.NonProfit))
                       .OrderBy(m => m.URI).Take(20).ToList();
                }
                else if (!String.IsNullOrEmpty(type) && type.ToLower() == "organizations")
                {
                    meCodesFound = _ctx.UserPayPoints.Select(m => m)
                       .Where(m => m.PayPointTypeId == meCodeTypeInt && m.URI.Contains(searchTerm) && m.User.UserTypeId.Equals((int)Domain.UserType.Organization))
                       .OrderBy(m => m.URI).Take(20).ToList();
                }
                else
                {
                    meCodesFound = _ctx.UserPayPoints.Select(m => m)
                        .Where(m => m.PayPointTypeId == meCodeTypeInt && m.URI.Contains(searchTerm) && (m.User.UserTypeId.Equals((int)Domain.UserType.Individual) || m.User.UserTypeId.Equals((int)Domain.UserType.Organization)))
                        .OrderBy(m => m.URI).Take(20).ToList();
                }

                return meCodesFound;
            }
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
        public void PersonalizeUser(string userId, string firstName, string lastName, string imageUrl)
        {
            using (var ctx = new Context())
            {
                Guid userGuid;

                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                var user = ctx.Users
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                user.FirstName = firstName;
                user.LastName = lastName;
                user.ImageUrl = imageUrl;

                var firstNameAttribute = ctx.UserAttributes
                    .FirstOrDefault(a => a.AttributeName == "FirstName");

                if (firstNameAttribute != null)
                {
                    user.UserAttributes.Add(new UserAttributeValue()
                    {
                        id = Guid.NewGuid(),
                        UserId  = user.UserId,
                        UserAttributeId = firstNameAttribute.Id,
                        AttributeValue = firstName
                    });
                }

                var lastNameAttribute = ctx.UserAttributes
                    .FirstOrDefault(a => a.AttributeName == "LastName");

                if (lastNameAttribute != null)
                {
                    user.UserAttributes.Add(new UserAttributeValue()
                    {
                        id = Guid.NewGuid(),
                        UserId = user.UserId,
                        UserAttributeId = lastNameAttribute.Id,
                        AttributeValue = lastName
                    });
                }

                ctx.SaveChanges();
            }

        }
        public bool ValidateUser(string userNameOrEmail, string password, out User foundUser)
        {
            using (var ctx = new Context())
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

                user = ctx.Users
                    .Include("PaymentAccounts")
                    .Include("SecurityQuestion")
                    .FirstOrDefault(Usr => Usr.UserName == userNameOrEmail);

                if (user == null)
                {
                    logger.Log(LogLevel.Warn, "Unable to find user by user name. Check email address.");
                    user = ctx.Users
                        .Include("PaymentAccounts")
                        .Include("SecurityQuestion")
                        .FirstOrDefault(Usr => Usr.EmailAddress == userNameOrEmail);
                }
                if (user == null)
                {
                    logger.Log(LogLevel.Warn, "Unable to find user by email address. Check mobile number.");
                    user = ctx.Users
                        .Include("PaymentAccounts")
                        .Include("SecurityQuestion")
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

                    if (user.SecurityQuestion == null)
                    {

                        user.PasswordFailuresSinceLastSuccess = 0;
                        user.PinCodeFailuresSinceLastSuccess = 0;
                        user.IsLockedOut = false;

                        ctx.SaveChanges();
                    }


                    user.PasswordFailuresSinceLastSuccess = 0;
                    ctx.SaveChanges();

                    foundUser = user;

                    return true;

                }
                else
                {

                    int failures = user.PasswordFailuresSinceLastSuccess;
                    if (failures != -1)
                    {
                        user.PasswordFailuresSinceLastSuccess += 1;
                        user.LastPasswordFailureDate = DateTime.UtcNow;

                        if (failures > 3)
                            user.IsLockedOut = true;

                        ctx.SaveChanges();

                        foundUser = null;

                        return false;
                    }
                }

                foundUser = null;
                return false;
            }
        }
        public bool VerifyMobilePayPoint(string userId, string userPayPointId, string verificationCode)
        {
            _logger.Log(LogLevel.Info, String.Format("Verify Mobile Pay Point User: {0} UserPayPoint:{1} Verification Code: {2}", userId, userPayPointId, verificationCode));

            using (var ctx = new Context())
            {

                Guid userPayPointGuid;
                Guid userGuid;

                Guid.TryParse(userId, out userGuid);
                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

                Guid.TryParse(userPayPointId, out userPayPointGuid);
                if (userPayPointGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User Pay Point {0} Not Found", userPayPointId));

                var payPointVerification = ctx.UserPayPointVerifications.FirstOrDefault(p => p.UserPayPointId == userPayPointGuid && p.UserPayPoint.UserId == userGuid &&
                    p.Confirmed == false);

                if (payPointVerification == null)
                    throw new CustomExceptions.BadRequestException("Invalid Attempt");

                if (payPointVerification.VerificationCode == verificationCode)
                {
                    payPointVerification.Confirmed = true;
                    payPointVerification.ConfirmedDate = System.DateTime.UtcNow;
                    payPointVerification.UserPayPoint.Verified = true;
                    payPointVerification.UserPayPoint.VerifiedDate = System.DateTime.UtcNow;

                    ctx.SaveChanges();

                    return true;
                }

                return false;
            }

        }
        public bool VerifyPayPoint(Guid payPointVerificationId)
        {
            using (var ctx = new Context())
            {

                var payPointVerification = ctx.UserPayPointVerifications
                        .FirstOrDefault(p => p.Id == payPointVerificationId);

                if (payPointVerification == null)
                    throw new Exception("Unable to find verification record.");

                if (payPointVerification.Confirmed)
                    throw new Exception(String.Format("This pay point is already verified."));

                if (payPointVerification.ExpirationDate < System.DateTime.Now)
                {
                    throw new Exception(String.Format("Unable to verify this pay point. The verification link has expired.",
                        payPointVerification.UserPayPoint.URI));
                }

                payPointVerification.Confirmed = true;
                payPointVerification.ConfirmedDate = System.DateTime.UtcNow;
                payPointVerification.UserPayPoint.Verified = true;
                payPointVerification.UserPayPoint.VerifiedDate = System.DateTime.UtcNow;

                ctx.SaveChanges();

                return true;
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

            return GetUserById(userIdGuid);

        }
        public User GetUserById(Guid userId)
        {
            var user = _ctx.Users
                .FirstOrDefault(u => u.UserId.Equals(userId));

            return user;
        }
        public User SignInWithFacebook(Guid apiKey, string accountId, string emailAddress, string firstName, string lastName,
            string deviceToken, string oAuthToken, DateTime tokenExpiration, string messageId, out bool isNewUser)
        {
            _logger.Log(LogLevel.Info, String.Format("Sign in with Facebook {0}: emailAddress: {1} and token {2}", accountId, emailAddress, oAuthToken));
            _logger.Log(LogLevel.Info, String.Format("Sign in with Facebook {0}: firstname {1} lastname {2}", accountId, firstName, lastName));

            User user = null;
            SocialNetwork socialNetwork = null;

            var memberRole = _ctx.Roles.FirstOrDefault(r => r.RoleName == "Member");

            try
            {
                user = _ctx.Users
                    .FirstOrDefault(u => u.FacebookUser.FBUserID.Equals(accountId));

                socialNetwork = _ctx.SocialNetworks
                    .FirstOrDefault(u => u.Name == "Facebook");

                if (user == null)
                {
                    _logger.Log(LogLevel.Info, String.Format("Unable to find user with Facebook account {0}: Email Address {1}.  Create new user.", accountId, emailAddress));

                    isNewUser = true;

                    user = AddUser(apiKey, "fb_" + accountId, "tempPassword", emailAddress, deviceToken, "", messageId);

                    user.FacebookUser = new FBUser()
                    {
                        FBUserID = accountId,
                        Id = Guid.NewGuid(),
                        TokenExpiration = tokenExpiration,
                        OAuthToken = oAuthToken
                    };
                    user.UserSocialNetworks = new Collection<UserSocialNetwork>();

                    user.UserSocialNetworks.Add(new UserSocialNetwork()
                    {
                        EnableSharing = true,
                        SocialNetwork = socialNetwork,
                        UserNetworkId = accountId,
                        UserAccessToken = oAuthToken
                    });

                    user.FirstName = firstName;
                    user.LastName = lastName;
                    user.ImageUrl = String.Format(_fbImageUrlFormat, accountId);

                }
                else
                {
                    isNewUser = false;

                    user.DeviceToken = deviceToken;
                    user.ImageUrl = String.Format(_fbImageUrlFormat, accountId);
                    if (user.FacebookUser != null)
                    {
                        user.FacebookUser.OAuthToken = oAuthToken;
                        user.FacebookUser.TokenExpiration = tokenExpiration;
                    }
                    else
                    {
                        user.FacebookUser = new FBUser()
                        {
                            FBUserID = accountId,
                            Id = Guid.NewGuid(),
                            TokenExpiration = tokenExpiration,
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

        public bool LinkFacebook(string apiKey, string userId, string accountId, string oAuthToken, DateTime tokenExpiration)
        {
            _logger.Log(LogLevel.Info, String.Format("Linking Facebook {0}: and token {1}", accountId, oAuthToken));

            User user = null;

            var memberRole = _ctx.Roles.FirstOrDefault(r => r.RoleName == "Member");

            try
            {
                user = GetUserById(userId);

                if (user != null)
                {
                    user.FacebookUser = new FBUser()
                    {
                        FBUserID = accountId,
                        Id = Guid.NewGuid(),
                        OAuthToken = oAuthToken,
                        TokenExpiration = tokenExpiration
                    };
                }
                else
                {
                    _logger.Log(LogLevel.Error, String.Format("Error linking facebook account, No user found for {0}", userId));

                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, "Error linking facebook account:");
                _logger.Log(LogLevel.Error, ex.Message);

                return false;
            }

            _ctx.SaveChanges();

            return true;
        }

        public User ResetPassword(string userId, string newPassword)
        {
            var user = GetUserById(userId);

            if (user == null)
                throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Found", userId));

            user.Password = securityService.Encrypt(newPassword);

            UpdateUser(user);

            return user;
        }
        public Domain.PasswordResetAttempt ValidatePasswordResetAttempt(string id)
        {
            using (var ctx = new Context())
            {
                Guid passwordResetAttemptGuid;
                Guid.TryParse(id, out passwordResetAttemptGuid);

                if (passwordResetAttemptGuid == null)
                    throw new CustomExceptions.BadRequestException("Password reset link is invalid");

                var passwordResetDb = ctx.PasswordResetAttempts
                    .Include("User")
                    .Include("User.SecurityQuestion")
                        .FirstOrDefault(p => p.Id == passwordResetAttemptGuid);

                if (passwordResetDb == null)
                    throw new CustomExceptions.BadRequestException("Password reset link is invalid");

                if (passwordResetDb.ExpiresDate < System.DateTime.Now)
                    throw new CustomExceptions.BadRequestException("Password reset link has expired.");


                if (passwordResetDb.Clicked)
                    throw new CustomExceptions.BadRequestException("Password reset link is no longer valid. Please generate a new link.");

                ctx.SaveChanges();

                return passwordResetDb;
            }
        }
        public void ResetPassword(string userId, string securityQuestionAnswer, string newPassword)
        {
            using (var ctx = new Context())
            {
                Guid userGuid;

                Guid.TryParse(userId, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                var user = ctx.Users
                    .Include("SecurityQuestion")
                    .FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                if (user.SecurityQuestion != null)
                {
                    if (!securityQuestionAnswer.Equals(securityService.Decrypt(user.SecurityQuestionAnswer)))
                        throw new CustomExceptions.BadRequestException(String.Format("Unable to reset password. Security Question Answer Not Correct"));
                }

                user.Password = securityService.Encrypt(newPassword);

                ctx.SaveChanges();
            }
        }

        public void SendResetPasswordLink(string apiKey, string userName)
        {
            DomainServices.EmailService emailService = new DomainServices.EmailService(_ctx);
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();

            string name = "";
            PasswordResetAttempt passwordResetDb = null;
            Guid applicationId;

            Application application = null;
            User user = null;

            using (var ctx = new Context())
            {
                Guid.TryParse(apiKey, out applicationId);

                application = ctx.Applications.FirstOrDefault(a => a.ApiKey == applicationId);

                if (application == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Application {0} is Invalid", apiKey));

                user = ctx.Users.FirstOrDefault(u => u.UserName == userName || u.MobileNumber == userName);

                if (user == null)
                    throw new CustomExceptions.BadRequestException(String.Format("User {0} is Invalid", userName));

                name = formattingService.FormatUserName(user);

                passwordResetDb = ctx.PasswordResetAttempts.Add(new PasswordResetAttempt()
                {
                    Clicked = false,
                    User = user,
                    ExpiresDate = System.DateTime.Now.AddHours(3),
                    Id = Guid.NewGuid()
                });

                ctx.SaveChanges();
            }

            string link = String.Format("{0}reset_password/{1}", ConfigurationManager.AppSettings["MobileWebSetURL"], passwordResetDb.Id);

            StringBuilder body = new StringBuilder();
            body.AppendFormat("Dear {0}", name).AppendLine().AppendLine();
            body.Append("You asked us to reset your PaidThx password. ");
            body.Append("To complete the process, please click on the link below ");
            body.Append("or paste it into your browser:").AppendLine().AppendLine();
            body.AppendLine(link).AppendLine();
            body.AppendLine("This link will be active for 3 hours only.").AppendLine();
            body.AppendLine("Thank you,").AppendLine();
            body.Append("The PaidThx Team");

            emailService.SendEmail(application.ApiKey, ConfigurationManager.AppSettings["fromEmailAddress"], user.EmailAddress, "How to reset your PaidThx password", body.ToString());
        }
        public void SendMobileVerificationCode(UserPayPoint userPayPoint)
        {
            DomainServices.EmailService emailService = new DomainServices.EmailService(_ctx);
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            DomainServices.ValidationService validateService = new ValidationService();
            DomainServices.SMSService smsService = new DomainServices.SMSService(_ctx);
            DomainServices.CommunicationService communicationServices = new DomainServices.CommunicationService(_ctx);

            string name = formattingService.FormatUserName(userPayPoint.User);
            Guid passwordResetGuid = Guid.NewGuid();
            DateTime expiresDate = System.DateTime.Now.AddHours(3);

            var random = new Random();
            int first = random.Next(10);
            int second = random.Next(10);
            int third = random.Next(10);
            int forth = random.Next(10);

            string verificationCode = String.Format("{0}{1}{2}{3}", first, second, third, forth);

            var verification = _ctx.UserPayPointVerifications.Add(
                new UserPayPointVerification()
                {
                    Id = Guid.NewGuid(),
                    CreateDate = System.DateTime.Now,
                    UserPayPoint = userPayPoint,
                    VerificationCode = verificationCode,
                    ExpirationDate = System.DateTime.Now.AddDays(30)
                });
            _ctx.SaveChanges();

            try
            {
                var communicationTemplate = communicationServices.GetCommunicationTemplate("Phone_Added_SMS");
                var link = String.Format(_mobileVerificationBaseUrl, verification.Id);

                //{0} - Link to verification screen
                //{1} - Phone verification code
                smsService.SendSMS(verification.UserPayPoint.User.ApiKey, verification.UserPayPoint.URI, String.Format(communicationTemplate.Template,
                    link, verificationCode));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Sending Email Verification Link to {0}. {1}", userPayPoint.URI, ex.Message));
            }

        }
        public void SendEmailVerificationLink(string userId, string userPayPointId)
        {
            using (var ctx = new Context())
            {
                DomainServices.UserService userService = new DomainServices.UserService(_ctx);
                DomainServices.EmailService emailService = new DomainServices.EmailService(_ctx);
                DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
                DomainServices.ValidationService validateService = new ValidationService();
                DomainServices.CommunicationService communicationService = new CommunicationService();

                Guid userGuid;
                Guid userPayPointGuid;

                Guid.TryParse(userId, out userGuid);

                if(userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", userId));

                var user = ctx.Users
                    .FirstOrDefault(u => u.UserId == userGuid);

                Guid.TryParse(userPayPointId, out userPayPointGuid);

                if(userPayPointGuid == null)
                    throw new CustomExceptions.BadRequestException(String.Format("Pay Point {0} Not Valid", userPayPointId));

                var userPayPoint = ctx.UserPayPoints
                    .FirstOrDefault(p => p.UserId == userGuid && p.Id == userPayPointGuid);

                if (userPayPoint == null)
                    throw new SocialPayments.DomainServices.CustomExceptions.NotFoundException(String.Format("User Pay Point {0} Not Found", userPayPointId));

                string name = formattingService.FormatUserName(user);
                DateTime expiresDate = System.DateTime.Now.AddHours(3);

                var verification = ctx.UserPayPointVerifications.Add(
                    new UserPayPointVerification()
                    {
                        Id = Guid.NewGuid(),
                        CreateDate = System.DateTime.Now,
                        UserPayPoint = userPayPoint,
                        VerificationCode = "",
                        ExpirationDate = System.DateTime.Now.AddDays(30)
                    });
                ctx.SaveChanges();

                string link = String.Format("{0}verify_paypoint/{1}", ConfigurationManager.AppSettings["MobileWebSetURL"], verification.Id);

                try
                {
                    var communicationTemplate = communicationService.GetCommunicationTemplate("Email_Added_Email");

                    //FIRST_NAME, LAST_NAME, EMAIL_ADDED, LINK_EMAIL_VERIFY
                    emailService.SendEmail(userPayPoint.URI, "Your email has been added", communicationTemplate.Template, new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("FIRST_NAME", userService.GetSenderName(userPayPoint.User)),
                        new KeyValuePair<string, string>("LAST_NAME", ""),
                        new KeyValuePair<string, string>("EMAIL_ADDED",  userPayPoint.URI),
                        new KeyValuePair<string, string>("LINK_EMAIL_VERIFY",  link)                            
                    });
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Exception Sending Email Verification Link to {0}. {1}", userPayPoint.URI, ex.Message));
                }
            }
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

        public List<Domain.Message> RefreshHomeScreen(string id)
        {
            using (var ctx = new Context())
            {
                DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
                DomainServices.UserService _userService = new DomainServices.UserService();
                DomainServices.MessageServices _messageService = new DomainServices.MessageServices();

                Guid userGuid;

                Guid.TryParse(id, out userGuid);

                if (userGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", id));

                Domain.User user = ctx.Users.FirstOrDefault(u => u.UserId == userGuid);

                if (user == null)
                    throw new CustomExceptions.NotFoundException(String.Format("User {0} Not Valid", id));

                List<Domain.Message> recentPayments = ctx.Messages
                    .Include("Recipient")
                    .Include("Recipient.Merchant")
                    .Where
                    (m => m.SenderId == user.UserId && m.MessageTypeValue.Equals((int)MessageType.Payment))
                    .OrderByDescending(m => m.CreateDate).ToList();

                return recentPayments;
            }
        }
        public User FindUserByEmailAddress(string emailAddress)
        {
            return _ctx.Users
                .Include("PaymentAccounts")
                .FirstOrDefault(u => u.UserName == emailAddress || u.EmailAddress == emailAddress);
        }
        public double GetUserInstantLimit(User User)
        {
            using (var ctx = new Context())
            {
                var timeToCheck = System.DateTime.Now.AddHours(-24);

                var verifiedPaymentAmounts = ctx.Messages
                    .Where(m => m.SenderId.Equals(User.UserId) && m.MessageTypeValue.Equals((int)MessageType.Payment) && m.CreateDate > timeToCheck && m.Payment.PaymentVerificationLevelValue.Equals((int)PaymentVerificationLevel.Verified));

                double amountSent = 0;

                if (verifiedPaymentAmounts.Count() > 0)
                    amountSent = verifiedPaymentAmounts.Sum(m => m.Amount);

                if (100 - amountSent > 0)
                    return 100 - amountSent;
                else
                    return 0;
            }
        }

        public string GetSenderName(User sender)
        {
            _logger.Log(LogLevel.Info, String.Format("Getting UserName {0}", sender.UserId));

            if (sender.Merchant != null)
                return sender.Merchant.Name;

            if (!String.IsNullOrEmpty(sender.FirstName) && !String.IsNullOrEmpty(sender.LastName))
                return sender.FirstName + " " + sender.LastName;
            else if (!String.IsNullOrEmpty(sender.FirstName))
                return sender.FirstName;
            else if (!String.IsNullOrEmpty(sender.LastName))
                return sender.LastName;
            else
            {
                if (!String.IsNullOrEmpty(sender.SenderName))
                    return sender.SenderName;

                if (!String.IsNullOrEmpty(sender.MobileNumber))
                    return formattingServices.FormatMobileNumber(sender.MobileNumber);

                if (!String.IsNullOrEmpty(sender.EmailAddress))
                    return sender.EmailAddress;
            }

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
