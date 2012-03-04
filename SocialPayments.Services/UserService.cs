using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SocialPayments.DataLayer;
using SocialPayments.Services.DataContracts.Users;
using SocialPayments.Services.ServiceContracts;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using SocialPayments.DomainServices;
using NLog;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using SocialPayments.Domain;

namespace SocialPayments.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class UserService : IUserService
    {
        private DomainServices.UserService userDomainService = new DomainServices.UserService();
        private EmailService emailService = new EmailService();
        private SecurityService securityService = new SecurityService();
        private Context _ctx = new Context();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public UserAcknowledgementResponse AcknowledgeUser(UserAckowledgementRequest request)
        {
            logger.Log(LogLevel.Info, string.Format("Acknowledging Device {0} at Mobile # {1}.", request.DeviceId, request.MobileNumber));

            var users = _ctx.Users
                .Include("PaymentAccounts")
                .Where(u => u.MobileNumber == request.MobileNumber);

            if (users.Count() == 0)
            {
                logger.Log(LogLevel.Info, string.Format("No user found with mobile number {0}",  request.MobileNumber));
                
                return new UserAcknowledgementResponse()
                {
                    UserId = "",
                    DoesDeviceIdMatch = false,
                    IsMobilieNumberRegistered = false,
                    SetupPassword = false,
                    SetupSecurityPin = false,
                    PaymentAccountId = ""
                };
            }

            logger.Log(LogLevel.Info, string.Format("User found with mobile number {0}", request.MobileNumber));
                
            var user = users.FirstOrDefault();
            String paymentAccountId = "";
             
            if(user.PaymentAccounts.Count > 0)
                paymentAccountId = user.PaymentAccounts[0].Id.ToString();

            return new UserAcknowledgementResponse()
            {
                UserId = user.UserId.ToString(),
                DoesDeviceIdMatch = true,
                IsMobilieNumberRegistered = true,
                SetupPassword = user.SetupPassword,
                SetupSecurityPin = user.SetupSecurityPin,
                PaymentAccountId =paymentAccountId
            };

        }
        public UserRegistrationResponse RegisterUser(UserRegistrationRequest request)
        {
            logger.Log(LogLevel.Error, string.Format("Registering User  {0}", request.UserName));

            string password = "pdthxnew";

            int defaultNumPasswordFailures = 0;

            var memberRole = _ctx.Roles.FirstOrDefault(r => r.RoleName == "Member");


            Domain.User user;

            try
            {
                user = _ctx.Users.Add(new Domain.User()
                {
                    UserId = Guid.NewGuid(),
                    ApiKey = new Guid(request.ApiKey),
                    CreateDate = System.DateTime.Now,
                    PasswordChangedDate = DateTime.UtcNow,
                    PasswordFailuresSinceLastSuccess = defaultNumPasswordFailures,
                    LastPasswordFailureDate = DateTime.UtcNow,
                    EmailAddress = "james@pdthx.me",
                    //IsLockedOut = isLockedOut,
                    //LastLoggedIn = System.DateTime.Now,
                    MobileNumber = request.MobileNumber,
                    Password = securityService.Encrypt(password), //hash
                    SecurityPin = securityService.Encrypt("1111"),
                    UserName = request.UserName,
                    UserStatus = Domain.UserStatus.Submitted,
                    IsConfirmed = false,
                    LastLoggedIn = System.DateTime.Now,
                    Limit = 0,
                    RegistrationMethod = Domain.UserRegistrationMethod.MobilePhone,
                    SetupPassword = false,
                    SetupSecurityPin = false,
                    Roles = new List<Role>()
                    {
                        memberRole
                    }
                });

                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, string.Format("Exception registering user {0}. Exception {1}.", request.MobileNumber, ex.Message));

                return new UserRegistrationResponse()
                {
                    Message = ex.Message,
                    Success = false
                };
            }

            if (user == null || user.UserId == null)
            {

                logger.Log(LogLevel.Error, string.Format("Exception registering user {0}. Exception {1}.", request.MobileNumber, "User or UserId is null"));

                return new UserRegistrationResponse()
                {
                    Message = "Failed Registering New User",
                    Success = false
                };
            }



            try
            {
                logger.Log(LogLevel.Info, string.Format("Calling Amazon SNS."));

                AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();

                client.Publish(new PublishRequest()
                {
                    Message = user.UserId.ToString(),
                    TopicArn = System.Configuration.ConfigurationManager.AppSettings["UserPostedTopicARN"],
                    Subject = "New User Registration"
                });
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Info, string.Format("Call Amazon SNS. Exception {1}.", ex.Message));
            }

            return new UserRegistrationResponse()
            {
                Success = true,
                Message = "Success",
                UserId = user.UserId.ToString()
            };
        }
        public UserSetupPasswordResponse SetupPassword(UserSetupPasswordRequest request)
        {
            logger.Log(LogLevel.Info, string.Format("Setting Password for user {0} from device {1}.", request.UserName, request.DeviceId));

            var user = _ctx.Users.FirstOrDefault(u => u.UserId == new Guid(request.UserId));

            if (user == null)
            {
                logger.Log(LogLevel.Error, string.Format("Setting Password. Unable to find user {0}.", request.UserName));

                return new UserSetupPasswordResponse()
                {
                    Success = false,
                    Message = "Unable to setup password"
                };
            }

            if (user.UserName != request.UserName)
            {
                logger.Log(LogLevel.Error, string.Format("Setting Password. Invalid userName {0} submitted in request.", request.UserName));

                return new UserSetupPasswordResponse()
                {
                    Success = false,
                    Message = "Unable to setup password"
                };
            }

            user.Password = securityService.Encrypt(request.Password);
            user.IsConfirmed = true;
            user.SetupPassword = true;

            try
            {
                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, string.Format("Setting Password. Exception saving context {1}.",  ex.Message));

                return new UserSetupPasswordResponse()
                {
                    Success = false,
                    Message = "Unable to setup password"
                };
            }

            logger.Log(LogLevel.Error, string.Format("Successfully set password for user {0} from {1}.", request.UserName, request.Password));

            return new UserSetupPasswordResponse()
            {
                Success = true,
                Message = "Password setup done."
            };
        }

        public UserSetupSecurityPinResonse SetupSecurityPin(UserSetupSecurityPinRequest request)
        {

            logger.Log(LogLevel.Info, string.Format("Setting Security Pin for user {0} to {1} from device {2}.", request.UserId, request.SecurityPin, request.DeviceId));

            var user = _ctx.Users.FirstOrDefault(u => u.UserId == new Guid(request.UserId));

            if (user == null)
            {
                logger.Log(LogLevel.Error, string.Format("Setting Security Pin. Unable to find user {0}.", request.UserId));

                return new UserSetupSecurityPinResonse()
                {
                    Success = false,
                    Message = "Unable to setup security pin."
                };
            }

            user.SecurityPin = securityService.Encrypt(request.SecurityPin);
            user.SetupSecurityPin = true;
            try
            {
                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, string.Format("Setting Security Pin. Exception saving context {1}.", ex.Message));

                return new UserSetupSecurityPinResonse()
                {
                    Success = false,
                    Message = "Unable to setup security pin"
                };
            }

            logger.Log(LogLevel.Error, string.Format("Successfully setup security pin for user {0}.", request.UserId));

            return new UserSetupSecurityPinResonse()
            {
                Success = true,
                Message = "Security Pin setup done."
            };
        }
        public bool ValidateUser(string userName, string password)
        {
            var userService = new DomainServices.UserService();

            Domain.User user;
            var isValid = userService.ValidateUser(userName, password, out user);

            return isValid;
        }
        public UserMobileVerificationResponse VerifyMobileDevice(UserMobileVerificationRequest request)
        {
            var user = _ctx.Users.FirstOrDefault(u => u.UserId == new Guid(request.UserId));

            if (user == null)
            {
                return new UserMobileVerificationResponse()
                {
                    Success = false,
                    Message = "Verification failed."
                };
            }
            if (user.MobileNumber != request.MobileNumber)
            {
                return new UserMobileVerificationResponse()
               {
                   Success = false,
                   Message = "Verification failed.  Mobile number mismatch."
               };
            }
            if (user.MobileVerificationCode1 != request.VerificationCode1 || user.MobileVerificationCode2 != request.VerificationCode2)
            {
                return new UserMobileVerificationResponse()
                {
                    Success = false,
                    Message = "Verification failed.  Invalid Verification Codes Submitted."
                };
            }

            return new UserMobileVerificationResponse()
            {
                Success = true,
                Message = "Verified Mobile Device Succeeded"
            };
        }
       public UserChangeSecurityPinResponse ChangeSecurityPin(UserChangeSecurityPinRequest request)
       {
           var results = new UserChangeSecurityPinResponse()
                             {

                             };

           results.Success = false;
           results.Message = "Unable to change security pin.";

           Guid userId;
           var parseValid = Guid.TryParse(request.UserId, out userId);

           if(!parseValid)
               return new UserChangeSecurityPinResponse()
                          {
                            Success = false,
                            Message = "UserId is invalid."
                          };

           var user = _ctx.Users.FirstOrDefault(u => u.UserId == userId);

           if (user == null)
               return new UserChangeSecurityPinResponse()
                          {
                              Success = false,
                              Message = "User not found."
                          };

           string securityPin;

           try
           {
               securityPin = securityService.Encrypt(request.SecurityPin);
           }
           catch (Exception ex)
           {
               throw ex;
           }

           user.SecurityPin = securityPin;

           _ctx.SaveChanges();

           return new UserChangeSecurityPinResponse()
                      {
                          Success = true,
                          Message = "Security was successfully changed."
                      };
       }
       public UserForgotPasswordResponse ForgotPassword(UserForgotPasswordRequest request)
       {
           var user = _ctx.Users.FirstOrDefault(u => u.EmailAddress == request.EmailAddress && u.MobileNumber == request.MobileNumber);
           var fromAddress = "support@pdthx.me";

           if(user == null)
               return new UserForgotPasswordResponse() {
                   Errors = true,
                   Message = "Unable to find user account. Try again."
               };
           emailService.SendEmail(new DataContracts.Email.EmailRequest()
                        {
                           // ApiKey = "",
                            Subject = "Your password request for PdThx",
                            Body = String.Format("The password for your account is {0}", user.Password),
                            FromAddress = fromAddress,
                            ToAddress = user.EmailAddress
                        });
           return new UserForgotPasswordResponse()
           {
               Errors = false,
               Message = String.Format("You're password was sent to {0}.  Please check your email.", request.EmailAddress)
           };
       }

        public DataContracts.Users.UserSignInResponse SignInUser(DataContracts.Users.UserSignInRequest request)
        {
            Domain.User user = null;

            var paymentAccountId = "";

            var isValid = userDomainService.ValidateUser(request.UserName, securityService.Encrypt(request.Password), out user);

            if (user != null && user.PaymentAccounts.Count > 0)
            {
                paymentAccountId = user.PaymentAccounts[0].Id.ToString();
            }

            if (isValid && user != null)
            {
                return new UserSignInResponse()
                {
                    IsValid = true,
                    UserId = user.UserId.ToString(),
                    MobileNumber = user.MobileNumber,
                    PaymentAccountId = paymentAccountId,
                    SetupPassword = user.SetupPassword,
                    SetupSecurityPin = user.SetupSecurityPin
                };
            }
            else
            {
                return new UserSignInResponse()
                {
                    IsValid = false,
                    UserId = "",
                    MobileNumber = "",
                    PaymentAccountId = "",
                    SetupPassword = false,
                    SetupSecurityPin = false
                };
            }
        }

        public DataContracts.Users.UserResponse AddUser(DataContracts.Users.UserRequest newUser)
        {
            var user = userDomainService.AddUser(newUser.UserName, newUser.Password, newUser.EmailAddress, true, newUser.MobileNumber,
                newUser.SecurityPin, Domain.UserStatus.Submitted, newUser.AccountNumber, Domain.PaymentAccountType.Checking,
                newUser.NameOnAccount, newUser.RoutingNumber);

            return new DataContracts.Users.UserResponse()
            {
                UserId = user.UserId,
                UserName = user.UserName,
                EmailAddress = user.EmailAddress,
                IsLockedOut = user.IsLockedOut,
                MobileNumber = user.MobileNumber,
                UserStatus = user.UserStatus.ToString()
            };
        }

        public List<DataContracts.Users.UserResponse> GetUsers()
        {
            var users = userDomainService.GetUsers();

            return users.Select(u => new DataContracts.Users.UserResponse() { UserId = u.UserId, MobileNumber = u.MobileNumber, EmailAddress = u.EmailAddress, UserName = u.UserName, UserStatus = u.UserStatus.ToString() 
            }).ToList(); 
        }

        public DataContracts.Users.UserResponse GetUser(string userId)
        {
            var user = userDomainService.GetUser(u => u.UserId.Equals(new Guid(userId)));

            return new DataContracts.Users.UserResponse()
            {
                UserId = user.UserId,
                UserName = user.UserName,
                MobileNumber = user.MobileNumber,
                EmailAddress = user.EmailAddress,
               // UserStatus = user.UserStatus.ToString(),
               // IsLockedOut = user.IsLockedOut
            };
        }

        public void UpdateUser(DataContracts.Users.UserRequest userRequest)
        {
            var userStatus = Domain.UserStatus.Verified; //(Domain.UserStatus)userRequest.UserStatus;

            userDomainService.UpdateUser(userRequest.UserId, userRequest.MobileNumber, userRequest.EmailAddress, userRequest.Password,
                userRequest.SecurityPin, userRequest.IsLockedOut, userStatus);
        }

        public void DeleteUser(DataContracts.Users.UserRequest userRequest)
        {
            userDomainService.DeleteUser(userRequest.UserId);
        }
    }
}