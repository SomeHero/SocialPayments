using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using System.Net;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using NLog;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Configuration;
using System.Collections.ObjectModel;
using System.Data.Entity;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UsersController : ApiController
    {
        private Context _ctx = new Context();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private DomainServices.UserService _userService = new DomainServices.UserService();
        private DomainServices.SecurityService securityService = new DomainServices.SecurityService();
        private DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();

        // GET /api/user
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/users/5
        public HttpResponseMessage<UserModels.UserResponse> Get(string id)
        {
            var user = GetUser(id);

            //TODO: check to make sure user exists
            if (user == null)
            {
                var message = new HttpResponseMessage<UserModels.UserResponse>(HttpStatusCode.NotFound);
                message.ReasonPhrase = "User Not Found";

                return message;
            }


            double sentTotal = 0;
            double receivedTotal = 0;

            var sentPayments = _ctx.Messages
                    .Where(m => m.SenderId.Equals(user.UserId) && m.MessageTypeValue.Equals((int)MessageType.Payment));

            if (sentPayments.Count() > 0)
                sentTotal = sentPayments.Sum(m => m.Amount);

            var receivedPayments = _ctx.Messages
                    .Where(m => m.RecipientId.Value.Equals(user.UserId) && m.MessageTypeValue.Equals((int)MessageType.Payment));

            if (receivedPayments.Count() > 0)
                receivedTotal = receivedPayments.Sum(m => m.Amount);

            var userResponse = new UserModels.UserResponse()
            {
                address = user.Address,
                city = user.City,
                createDate = user.CreateDate.Value.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                culture = user.Culture,
                emailAddress = user.EmailAddress,
                firstName = user.FirstName,
                isConfirmed = user.IsConfirmed,
                isLockedOut = user.IsLockedOut,
                lastLoggedIn = user.LastLoggedIn.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                lastName = user.LastName,
                lastPasswordFailureDate = user.LastPasswordFailureDate,
                mobileNumber = user.MobileNumber,
                passwordFailuresSinceLastSuccess = user.PasswordFailuresSinceLastSuccess,
                senderName = user.SenderName,
                state = user.State,
                timeZone = user.TimeZone,
                userId = user.UserId,
                userName = user.UserName,
                userStatus = user.UserStatus.ToString(),
                zip = user.Zip,
                userAttributes = user.UserAttributes.Select(a => new UserModels.UserAttribute()
                {
                    AttributeName = a.UserAttribute.AttributeName,
                    AttributeValue = a.AttributeValue
                }).ToList(),
                upperLimit = user.Limit,
                totalMoneyReceived = receivedTotal,
                totalMoneySent = sentTotal
            };

            return new HttpResponseMessage<UserModels.UserResponse>(userResponse, HttpStatusCode.OK);
        }

        // POST /api/user
        public HttpResponseMessage<UserModels.SubmitUserResponse> Post(UserModels.SubmitUserRequest request)
        {
            _logger.Log(LogLevel.Error, string.Format("Registering User  {0}", request.userName));

            int defaultNumPasswordFailures = 0;

            var memberRole = _ctx.Roles.FirstOrDefault(r => r.RoleName == "Member");

            String mobileNumber = formattingService.FormatMobileNumber(request.mobileNumber);

            _logger.Log(LogLevel.Error, string.Format("I got here 1"));

            User user;

            //validate that email address is not already user
            user = _userService.FindUserByEmailAddress(request.userName);


            if (user != null)
            {
                var errorMessage = new HttpResponseMessage<UserModels.SubmitUserResponse>(HttpStatusCode.BadRequest);
                errorMessage.ReasonPhrase = String.Format("The email address {0} is already registered.", request.emailAddress);

                return errorMessage;
            }
            //ValidateUser the phone number is not already registered
            user = _userService.FindUserByMobileNumber(mobileNumber);

            if (user != null)
            {
                var errorMessage = new HttpResponseMessage<UserModels.SubmitUserResponse>(HttpStatusCode.BadRequest);
                errorMessage.ReasonPhrase = String.Format("The mobile number {0} is already registered.", request.mobileNumber);

                return errorMessage;
            }

            _logger.Log(LogLevel.Error, string.Format("I got here 2"));

            try
            {
                user = _ctx.Users.Add(new Domain.User()
                {
                    UserId = Guid.NewGuid(),
                    ApiKey = new Guid(request.apiKey),
                    CreateDate = System.DateTime.Now,
                    PasswordChangedDate = DateTime.UtcNow,
                    PasswordFailuresSinceLastSuccess = defaultNumPasswordFailures,
                    LastPasswordFailureDate = DateTime.UtcNow,
                    EmailAddress = request.emailAddress,
                    //IsLockedOut = isLockedOut,
                    //LastLoggedIn = System.DateTime.Now,
                    MobileNumber = mobileNumber,
                    Password = securityService.Encrypt(request.password), //hash
                    SecurityPin = securityService.Encrypt(request.securityPin),
                    UserName = request.userName,
                    UserStatus = Domain.UserStatus.Submitted,
                    IsConfirmed = false,
                    LastLoggedIn = System.DateTime.Now,
                    Limit = Convert.ToDouble(ConfigurationManager.AppSettings["InitialPaymentLimit"]),
                    RegistrationMethod = Domain.UserRegistrationMethod.MobilePhone,
                    SetupPassword = false,
                    SetupSecurityPin = false,
                    Roles = new Collection<Role>()
                    {
                        memberRole
                    },
                    DeviceToken = request.deviceToken
                });

                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, string.Format("Exception registering user {0}. Exception {1}.", request.mobileNumber, ex.Message));

                var message = new HttpResponseMessage<UserModels.SubmitUserResponse>(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = String.Format("Unable to register user. {0}", ex.Message);

                return message;
            }

            try
            {
                //_logger.Log(LogLevel.Error, string.Format("Calling Amazon SNS."));

                //AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();

                //client.Publish(new PublishRequest()
                //{
                //    Message = user.UserId.ToString(),
                //    TopicArn = System.Configuration.ConfigurationManager.AppSettings["UserPostedTopicARN"],
                //    Subject = "New User Registration"
                //});
            }
            catch (Exception ex)
            {
                //_logger.Log(LogLevel.Info, string.Format("Call Amazon SNS. Exception {1}.", ex.Message));
            }

            var responseMessage = new UserModels.SubmitUserResponse()
            {
                userId = user.UserId.ToString(),
                mobileNumber = user.MobileNumber
            };

            return new HttpResponseMessage<UserModels.SubmitUserResponse>(responseMessage, HttpStatusCode.Created);
        }
        
        // PUT /api/user/5
        public void Put(int id, string value)
        {
        }

        //POST /api/users/validate_user
        public HttpResponseMessage<UserModels.ValidateUserResponse> ValidateUser(UserModels.ValidateUserRequest request)
        {
            var userService = new DomainServices.UserService();

            User user;
            var isValid = userService.ValidateUser(request.userName, request.password, out user);

            if (isValid){
                var message = new UserModels.ValidateUserResponse()
                {
                    userId = user.UserId.ToString(),
                    mobileNumber = user.MobileNumber,
                    paymentAccountId = (user.PaymentAccounts != null && user.PaymentAccounts.Count() > 0 ? user.PaymentAccounts[0].Id.ToString() : "")
                };

                return new HttpResponseMessage<UserModels.ValidateUserResponse>(message, HttpStatusCode.OK);
            }
            else
                return new HttpResponseMessage<UserModels.ValidateUserResponse>(HttpStatusCode.Forbidden);
        }

        // DELETE /api/user/5
        public void Delete(int id)
        {
        }


        private User GetUser(string id)
        {
            Guid userId;

            Guid.TryParse(id, out userId);

            if (userId == null)
                return null;

            var user = _ctx.Users
                .Include("PaymentAccounts")
                .Include("UserAttributes")
                .Include("UserAttributes.UserAttribute")
                .FirstOrDefault(u => u.UserId.Equals(userId));

            if (user == null)
                return null;

            return user;
        }
    }
}
