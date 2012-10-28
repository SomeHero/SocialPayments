using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using System.Net;
using SocialPayments.Domain;
using NLog;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Configuration;
using System.Collections.ObjectModel;
using System.Data.Entity;
using SocialPayments.DomainServices.Interfaces;
using System.Threading.Tasks;
using Amazon.S3.Model;
using System.IO;
using System.Text;
using SocialPayments.Domain.ExtensionMethods;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UsersController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private Guid ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174");

        // GET /api/user
        [HttpGet]
        public HttpResponseMessage Get(int take, int skip, int page, int pageSize)
        {
            var userServices = new DomainServices.UserService();
            List<Domain.User> users = null;
            int totalRecords = 0;

            try
            {
                users = userServices.GetPagedUsers(take, skip, page, pageSize, out totalRecords);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<UserModels.PagedResults>(HttpStatusCode.OK, new UserModels.PagedResults()
            {
                TotalRecords = totalRecords,
                Results = users.Select(u => new UserModels.UserResponse()
                {
                    createDate = (u.CreateDate != null ? u.CreateDate.Value.ToString("MM/dd/yyyy") : ""),
                    imageUrl = u.ImageUrl,
                    instantLimit = u.Limit,
                    isConfirmed = u.IsConfirmed,
                    isLockedOut = u.IsLockedOut,
                    lastLoggedIn = (u.LastLoggedIn != null ? u.LastLoggedIn.ToString("MM/dd/yyyy") : ""),
                    userId = u.UserId,
                    userName = u.UserName,
                    userStatus = u.UserStatus.ToString(),
                    canExpress = u.CanExpress,
                    expressDeliveryFeePercentage = u.ExpressDeliveryFeePercentage,
                    expressDeliveryThreshold = u.ExpressDeliveryFreeThreshold
                }).ToList()
            });
        }
        // GET /api/users/5
        [HttpGet]
        public HttpResponseMessage Get(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Getting User {0}", id));

            var securityService = new DomainServices.SecurityService();
            var formattingService = new DomainServices.FormattingServices();
            var validationServices = new DomainServices.ValidationService();
            var userService = new DomainServices.UserService();
            var messageServices = new DomainServices.MessageServices(); ;
            UserModels.UserResponse userResponse = null;

            User user = null;

            try
            {
                user = userService.GetUserById(id);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Unable to find user by id {0}. {1}", id, ex.Message));
            }

            if (user == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("User {0} Not Valid", id));
            }

            foreach (var payPoint in user.PayPoints)
            {
                if(payPoint.PayPointTypeId == 2 && validationServices.IsPhoneNumber(payPoint.URI))
                    payPoint.URI = formattingService.FormatMobileNumber(payPoint.URI);
            }

            double sentTotal = 0;
            double receivedTotal = 0;

            //var sentPayments = _ctx.Messages
            //        .Where(m => m.SenderId.Equals(user.UserId) && m.MessageTypeValue.Equals((int)MessageType.Payment));

            //if (sentPayments.Count() > 0)
            //    sentTotal = sentPayments.Sum(m => m.Amount);

            //var receivedPayments = _ctx.Messages
            //        .Where(m => m.RecipientId.Value.Equals(user.UserId) && m.MessageTypeValue.Equals((int)MessageType.Payment));

            //if (receivedPayments.Count() > 0)
            //    receivedTotal = receivedPayments.Sum(m => m.Amount);

            string userName = userService.GetSenderName(user);
            var numberOfPayStreamUpdates = messageServices.GetNumberOfPaystreamUpdates(user);
            var outstandingMessages = messageServices.GetOutstandingMessage(user);
            var newCount = messageServices.GetNewMessages(user);
            var pendingCount = messageServices.GetPendingMessages(user);

            var fbId = (user.FacebookUser == null ? @"" : user.FacebookUser.FBUserID);
            var fbToken = (user.FacebookUser == null ? @"" : user.FacebookUser.OAuthToken);


            try
            {
                userResponse = new UserModels.UserResponse()
                {
                    address = user.Address,
                    city = user.City,
                    createDate = user.CreateDate.Value.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                    culture = user.Culture,
                    deviceToken = user.DeviceToken,
                    emailAddress = user.EmailAddress,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    imageUrl = (user.ImageUrl != null ? user.ImageUrl : ""),
                    isConfirmed = user.IsConfirmed,
                    isLockedOut = user.IsLockedOut,
                    lastLoggedIn = user.LastLoggedIn.ToString("ddd MMM dd HH:mm:ss zzz yyyy"),
                    lastPasswordFailureDate = user.LastPasswordFailureDate,
                    mobileNumber = user.MobileNumber,
                    passwordFailuresSinceLastSuccess = user.PasswordFailuresSinceLastSuccess,
                    registrationId = user.RegistrationId,
                    senderName = userName,
                    state = user.State,
                    timeZone = user.TimeZone,
                    userId = user.UserId,
                    userName = user.UserName,
                    userStatus = user.UserStatus.ToString(),
                    zip = user.Zip,
                    userAttributes = user.UserAttributes.Select(a => new UserModels.UserAttribute()
                    {
                        AttributeId = a.UserAttributeId,
                        AttributeName = a.UserAttribute.AttributeName,
                        AttributeValue = (a.AttributeValue != null ? a.AttributeValue : "")
                    }).ToList(),
                    upperLimit = user.Limit,
                    instantLimit = userService.GetUserInstantLimit(user),
                    totalMoneyReceived = receivedTotal,
                    totalMoneySent = sentTotal,
                    preferredPaymentAccountId = user.PreferredSendAccountId.ToString(),
                    preferredReceiveAccountId = user.PreferredReceiveAccountId.ToString(),
                    setupSecurityPin = (String.IsNullOrEmpty(user.SecurityPin) ? false : true),
                    securityQuestion = (user.SecurityQuestion != null ? user.SecurityQuestion.Question : ""),
                    securityQuestionId = user.SecurityQuestionID,
                    pendingMessages = outstandingMessages.Select(m => new MessageModels.MessageResponse()
                    {
                        amount = m.Amount,
                        comments = m.Comments,
                        createDate = formattingService.FormatDateTimeForJSON(m.CreateDate),
                        lastUpdatedDate = formattingService.FormatDateTimeForJSON(m.LastUpdatedDate),
                        direction = m.Direction,
                        Id = m.Id,
                        latitude = m.Latitude,
                        longitutde = m.Longitude,
                        messageStatus = m.Status.ToString(),
                        messageType = m.MessageType.ToString(),
                        recipientName = m.RecipientName,
                        recipientUri = m.RecipientUri,
                        senderName = m.SenderName,
                        senderUri = m.SenderUri,
                        transactionImageUri = m.TransactionImageUrl
                    }).ToList(),
                    userPayPoints = (user.PayPoints != null ? user.PayPoints
                    .Where(a => a.IsActive)
                    .Select(p => new UserModels.UserPayPointResponse()
                    {
                        Id = p.Id.ToString(),
                        Type = p.Type.Name,
                        Uri = p.URI,
                        UserId = p.UserId.ToString(),
                        Verified = p.Verified,
                        VerifiedDate = "",
                        CreateDate = p.CreateDate.ToString("ddd MMM dd HH:mm:ss zzz yyyy")
                    }).ToList() : null),
                    bankAccounts = (user.PaymentAccounts != null ? user.PaymentAccounts
                    .Where(b => b.IsActive)
                    .Select(a => new AccountModels.AccountResponse()
                    {
                        AccountNumber = securityService.GetLastFour(securityService.Decrypt(a.AccountNumber)),
                        AccountType = a.AccountType.ToString(),
                        Id = a.Id.ToString(),
                        BankName = a.BankName,
                        BankIconUrl = a.BankIconURL,
                        NameOnAccount = securityService.Decrypt(a.NameOnAccount),
                        Nickname = a.Nickname == null ? String.Format("{0} {1}",a.AccountType,securityService.GetLastFour(securityService.Decrypt(a.AccountNumber))) : a.Nickname,
                        RoutingNumber = securityService.Decrypt(a.RoutingNumber),
                        UserId = a.UserId.ToString(),
                        Status = a.AccountStatus.GetDescription()
                    }).ToList() : null),
                    userConfigurationVariables = (user.UserConfigurations != null ? user.UserConfigurations.Select(c =>
                        new UserModels.UserConfigurationResponse()
                        {
                            Id = c.Id.ToString(),
                            UserId = c.UserId.ToString(),
                            ConfigurationKey = c.ConfigurationKey,
                            ConfigurationValue = c.ConfigurationValue,
                            ConfigurationType = c.ConfigurationType
                        }).ToList() : null),
                    userSocialNetworks = (user.UserSocialNetworks != null ? user.UserSocialNetworks.Select(s =>
                        new UserModels.UserSocialNetworkResponse() {
                            SocialNetwork = "Facebook",
                            SocialNetworkUserId = s.UserNetworkId,
                            SocialNetworkUserToken = s.UserAccessToken
                        }).ToList() : null),
                    numberOfPaystreamUpdates = numberOfPayStreamUpdates,
                    newMessageCount = 1,
                    pendingMessageCount = 1,
                    facebookId = fbId,
                    facebookToken = fbToken,
                    canExpress = user.CanExpress,
                    expressDeliveryFeePercentage = user.ExpressDeliveryFeePercentage,
                    expressDeliveryThreshold = user.ExpressDeliveryFreeThreshold
                };
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled exception formatting User Response {0}. Exception: {1}. Stack Trace: {2}", id, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<UserModels.UserResponse>(HttpStatusCode.OK, userResponse);
        }

        // POST /api/user
        [HttpPost]
        public HttpResponseMessage Post(UserModels.SubmitUserRequest request)
        {
            _logger.Log(LogLevel.Info, string.Format("Registering User  {0}", request.userName));

            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            DomainServices.UserService _userService = new DomainServices.UserService();

            //_logger.Log(LogLevel.Error, string.Format("Formatting Mobile Number"));

            //try
            //{
            //    if (!String.IsNullOrEmpty(request.mobileNumber))
            //    {

            //        formattingService.RemoveFormattingFromMobileNumber(request.mobileNumber);

            //        _logger.Log(LogLevel.Error, string.Format("Registering User Mobile Number {0}", mobileNumber));

            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.Log(LogLevel.Error, string.Format("Exception formatting mobile number. {0}", ex.Message));

            //}
            User user;

            //validate that email address is not already user
           
            //if(!String.IsNullOrEmpty(mobileNumber))
            //{
            //    user = _userService.FindUserByMobileNumber(mobileNumber);

            //    if (user != null)
            //    {
            //        var errorMessage = new HttpResponseMessage<UserModels.SubmitUserResponse>(HttpStatusCode.BadRequest);
            //        errorMessage.ReasonPhrase = String.Format("The mobile number {0} is already registered.", request.mobileNumber);

            //        return errorMessage;
            //    }
            //}

            try
            {
                _logger.Log(LogLevel.Info, String.Format("Adding user {0}", request.userName));

                user = _userService.FindUserByEmailAddress(request.userName);

                if (user != null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, String.Format("Sorry, {0} belongs to an existing account.", request.userName));
                }

                user = _userService.RegisterUser(Guid.Parse(request.apiKey), request.userName, request.password, request.emailAddress,
                    request.deviceToken, "", request.messageId);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, string.Format("Exception registering user {0}. Exception: {1} Stack Trace: {2}", request.emailAddress, ex.Message, ex.StackTrace));

                var innerException = ex.InnerException;

                while (innerException != null)
                {
                    _logger.Log(LogLevel.Error, string.Format("Inner Exception: {0}.", innerException.Message));

                    innerException = innerException.InnerException;
                }

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            
            }

            var responseMessage = new UserModels.SubmitUserResponse()
            {
                userId = user.UserId.ToString()
            };

            return Request.CreateResponse<UserModels.SubmitUserResponse>(HttpStatusCode.Created, responseMessage);
        }

        // POST /api/user/{id}/personalize_user
        public HttpResponseMessage PersonalizeUser(string id, UserModels.PersonalizeUserRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Start Personalize User"));

            DomainServices.UserService userService = new DomainServices.UserService();

            try
            {
                userService.PersonalizeUser(id, request.FirstName, request.LastName, request.ImageUrl);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Personalizing User {0}. Exception {1}", id, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Personalizing User {0}. Exception {1}", id, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Personalizing User {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST /api/users/{id}/upload_member_image
        public async Task<HttpResponseMessage> UploadMemberImage([FromUri] string id)
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(
                    Request.CreateResponse(HttpStatusCode.UnsupportedMediaType));
            }

            if(!Directory.Exists(String.Format(@"{0}\{1}", @"c:\memberImages", id)))
                Directory.CreateDirectory(String.Format(@"{0}\{1}", @"c:\memberImages", id));

            var provider = new RenamingMultipartFormDataStreamProvider(String.Format(@"{0}\{1}", @"c:\memberImages", id));

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach(MultipartFileData file in provider.FileData)
                {

                    var fileName = file.LocalFileName;

                    string bucketName = ConfigurationManager.AppSettings["MemberImagesBucketName"];

                    // _logger.Log(LogLevel.Info, String.Format("Uploading Batch File for batch {0} to bucket {1}", transactionBatch.Id, bucketName));

                    if (String.IsNullOrEmpty(bucketName))
                        throw new Exception("S3 bucket name for MemberImages not configured");

                    var fileContent = File.OpenRead(fileName);

                    Amazon.S3.AmazonS3Client s3Client = new Amazon.S3.AmazonS3Client();
                    PutObjectRequest putRequest = new PutObjectRequest()
                    {
                        BucketName = bucketName,
                        InputStream = fileContent,
                        Key = String.Format("{0}/image1.png", id)
                    };
                    try
                    {
                        s3Client.PutObject(putRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Unable to upload member image to S3. {0}", ex.Message));
                    }

                    return Request.CreateResponse<FileUploadResponse>(HttpStatusCode.OK,
                        new FileUploadResponse()
                        {
                            ImageUrl = String.Format("http://memberimages.paidthx.com/{0}/image1.png", id)
                        });
                };
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to upload image");
        }
        // PUT /api/user/5
        public void Put(int id, string value)
        {

        }

        public HttpResponseMessage ChangeSecurityPin(string id, UserModels.ChangeSecurityPinRequest request)
        {
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            DomainServices.UserService userService = new DomainServices.UserService();

            var user = userService.GetUserById(id);

            if (!securityService.Decrypt(user.SecurityPin).Equals(request.currentSecurityPin))
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Security Pin Don't Match");
            }
            if (request.newSecurityPin.Length < 4)
            {
                var error = @"Invalid Security Pin";

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Pin for {0}. {1}", id, error));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            
            }

            user.SecurityPin = securityService.Encrypt(request.newSecurityPin);
            userService.UpdateUser(user);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        //POST /api/users/{userId}/setup_securityquestion
        public HttpResponseMessage SetupSecurityQuestion(string id, UserModels.UpdateSecurityQuestion request)
        {
            _logger.Log(LogLevel.Info, String.Format("Setting up Security Question for {0}", id));

            DomainServices.UserService userService = new DomainServices.UserService();

            if (request.questionId < 0)
            {
                var error = @"Invalid Security Question Index";

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Question for {0}. {1}", id, error));

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            
            }
            if (request.questionAnswer.Length < 1)
            {
                var error = @"Invalid Security Question Length";

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Question for {0}. {1}", id, error));

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }

            try
            {
                userService.SetupSecurityQuestion(id, request.questionId, request.questionAnswer);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Question for {0}. {1}", id, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }


        //POST /api/users/validate_passwordreset
        public HttpResponseMessage ValidatePasswordResetAttempt(UserModels.ValidatePasswordResetAttemptRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Validating Password Reset Attempt {0}", request.ResetPasswordAttemptId));

            DomainServices.UserService userService = new DomainServices.UserService();
            Domain.PasswordResetAttempt passwordResetAttempt = null;

            try
            {
                passwordResetAttempt = userService.ValidatePasswordResetAttempt(request.ResetPasswordAttemptId);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Validating Password Reset Attempt {0}. Exception {1}", request.ResetPasswordAttemptId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Validating Password Reset Attempt {0}. Exception {1}", request.ResetPasswordAttemptId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Validating Password Reset Attempt {0}. Exception {1}. Stack Trace {2}", request.ResetPasswordAttemptId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<UserModels.ValidatePasswordResetAttemptResponse>(HttpStatusCode.OK, new UserModels.ValidatePasswordResetAttemptResponse() {
              HasSecurityQuestion = !(passwordResetAttempt.User.SecurityQuestion == null),
              SecurityQuestion = (passwordResetAttempt.User.SecurityQuestion != null ? passwordResetAttempt.User.SecurityQuestion.Question : ""),
              UserId = passwordResetAttempt.UserId.ToString()
            });
        }
        public HttpResponseMessage ChangePassword(string id, UserModels.ChangePasswordRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Changing password for: {0}", id));

            DomainServices.UserService userService = new DomainServices.UserService();
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();

            try
            {
                userService.ChangePassword(id, request.currentPassword, request.newPassword);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Changing Password for User {0}. Exception {1}", id, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception  Changing Password for User  {0}. Exception {1}", id, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Changing Password for User  {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        //POST api/users/reset_password
        public HttpResponseMessage ResetPassword(UserModels.ResetPasswordRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Reset Password for User {0}", request.userId));

            var userServices = new DomainServices.UserService();
            
            try
            {
                userServices.ResetPassword(request.userId, request.securityQuestionAnswer, request.newPassword);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Reseting Password for {0}. Exception {1}", request.userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Reseting Password for {0}. Exception {1}", request.userId, ex.Message));
                
                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Reseting Password for {0}. Exception {1}. Stack Trace {2}", request.userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
        //POST api/users/forgot_password
        public HttpResponseMessage ForgotPassword(UserModels.ForgotPasswordRequest request)
        {
            var validateService = new DomainServices.ValidationService();
            var userService = new DomainServices.UserService();

            try
            {
                if (!(validateService.IsEmailAddress(request.userName) || validateService.IsPhoneNumber(request.userName)))
                    throw new SocialPayments.DomainServices.CustomExceptions.BadRequestException("You must enter a valid email address or mobile #.");

                userService.SendResetPasswordLink(ApiKey.ToString(), request.userName);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Sending Forgot Password Email to {0}. Exception {1}", request.userName, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Sending Forgot Password Email to {0}. Exception {1}", request.userName, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Sending Forgot Password Email to {0}. Exception {1}. Stack Trace {2}", request.userName, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
           
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        //POST /api/users/{userId}/registerpushnotifications
        public HttpResponseMessage RegisterForPushNotifications(string id, UserModels.PushNotificationRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Registering push notifications for Android for: {0}", id));

            DomainServices.UserService userService = new DomainServices.UserService();

            var user = userService.GetUserById(id);

            if (String.IsNullOrEmpty(user.RegistrationId) || (!String.IsNullOrEmpty(user.RegistrationId) && !user.RegistrationId.Equals(request.registrationId)))
            {
                try
                {
                    userService.AddPushNotificationRegistrationId(id, request.deviceToken, request.registrationId);
                }
                catch (Exception ex)
                {
                    var error = ex.Message;

                    _logger.Log(LogLevel.Error, String.Format("Unable to register push notifications for {0}. {1}", id, error));

                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            
                }
            }
            else
            {
                var error = "Device Tokens match, no need to reregister for Android push.";

                _logger.Log(LogLevel.Error, String.Format("Unable to register push notifications for {0}. {1}", id, error));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // GET /api/users/{userId}/refresh_homepage
        [HttpGet]
        public HttpResponseMessage RefreshHomepageInformation(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Refreshing homepage for {0}", id));

            List<Domain.Message> recentPayments = null;
            var userService = new DomainServices.UserService();
            var messageService = new DomainServices.MessageServices();
            List<UserModels.QuickSendUserReponse> quickSends = new List<UserModels.QuickSendUserReponse>();
            int numberOfIncomingNotifications = 0;
            int numberOfOutgoingNotifications = 0;

            User user = null;

            try
            {
                recentPayments = messageService.GetQuickSendPayments(id);

                foreach (Message msg in recentPayments)
                {

                    string recipName;

                    int recipientType = -1;

                    _logger.Log(LogLevel.Debug, "RecipientURI: {0} isFacebookRecipient? {1}", msg.RecipientUri, msg.RecipientUri.Substring(0, 3).Equals("fb_") ? "YES" : "NO");

                    if (msg.RecipientUri.Substring(0, 3).Equals("fb_"))
                    {
                        _logger.Log(LogLevel.Debug, "First: {0} Last: {1} Name: {2}", msg.recipientFirstName, msg.recipientLastName, msg.RecipientName);

                        if (msg.Recipient != null)
                        {
                            quickSends.Add(new UserModels.QuickSendUserReponse()
                            {
                                userUri = msg.RecipientUri,
                                userName = msg.Recipient.SenderName,
                                userFirstName = msg.Recipient.FirstName,
                                userLastName = msg.Recipient.LastName,
                                userImage = String.Format("http://graph.facebook.com/{0}/picture", msg.RecipientUri.Substring(3)),
                                userType = 0 // Normal User
                            });
                        }
                        else
                        {
                            quickSends.Add(new UserModels.QuickSendUserReponse()
                            {
                                userUri = msg.RecipientUri,
                                userName = msg.RecipientName,
                                userFirstName = msg.recipientFirstName,
                                userLastName = msg.recipientLastName,
                                userImage = String.Format("http://graph.facebook.com/{0}/picture", msg.RecipientUri.Substring(3)),
                                userType = 0 // Normal User
                            });
                        }
                    }
                    else
                    {
                        if (msg.RecipientId == null)
                        {
                            _logger.Log(LogLevel.Debug, "RecipientURI: {0} isFacebookRecipient? {1}", msg.RecipientUri, msg.RecipientUri.Substring(0, 3).Equals("fb_") ? "YES" : "NO");

                            recipName = msg.RecipientUri;

                            quickSends.Add(new UserModels.QuickSendUserReponse()
                            {
                                userUri = msg.RecipientUri,
                                userName = recipName,
                                userFirstName = msg.recipientFirstName,
                                userLastName = msg.recipientLastName,
                                userImage = msg.recipientImageUri,
                                userType = 0 // Normal User
                            });
                        }
                        else if (msg.Recipient.Merchant != null)
                        {
                            _logger.Log(LogLevel.Info, "Found a merchant for {0}", msg.Recipient.Merchant.Id.ToString());

                            recipName = msg.RecipientUri;

                            recipientType = 1;

                            string imageUri = null;

                            if (msg.Recipient != null)
                                imageUri = msg.Recipient.ImageUrl;

                            quickSends.Add(new UserModels.QuickSendUserReponse()
                            {
                                userUri = msg.RecipientId.ToString(),
                                userName = recipName,
                                userFirstName = msg.Recipient.FirstName,
                                userLastName = msg.Recipient.LastName,
                                userImage = imageUri,
                                userType = recipientType
                            });
                        }
                        else
                        {
                            recipName = msg.Recipient.SenderName;

                            recipientType = 0;

                            string imageUri = null;

                            if (msg.Recipient != null)
                                imageUri = msg.Recipient.ImageUrl;

                            quickSends.Add(new UserModels.QuickSendUserReponse()
                            {
                                userUri = msg.RecipientUri,
                                userName = recipName,
                                userFirstName = msg.Recipient.FirstName,
                                userLastName = msg.Recipient.LastName,
                                userImage = imageUri,
                                userType = recipientType
                            });
                        }
                    }
                }
 
                numberOfOutgoingNotifications = messageService.GetNumberOfPendingMessages(id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Refreshing Home Page Information {0}. Exception {1}", id, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);       
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Refreshing Home Page Information {0}. Exception {1}", id, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Refreshing Home Page Information {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            var results = new UserModels.HomepageRefreshReponse()
            {
                userId = id,
                numberOfIncomingNotifications = numberOfIncomingNotifications,
                numberOfOutgoingNotifications = numberOfOutgoingNotifications,
                quickSendContacts = quickSends
            };

            return Request.CreateResponse<UserModels.HomepageRefreshReponse>(HttpStatusCode.OK, new UserModels.HomepageRefreshReponse()
            {
                userId = id,
                numberOfIncomingNotifications = numberOfIncomingNotifications,
                numberOfOutgoingNotifications = numberOfOutgoingNotifications,
                quickSendContacts = quickSends
            });
        }


        // GET /api/users/searchbymecode/{searchterm}?type={type}
        [HttpGet]
        public HttpResponseMessage GetMatchingMECodesWithSearchTerm(string searchTerm, string type)
        {
            _logger.Log(LogLevel.Info, String.Format("Finding ME Codes maching {0}", searchTerm));

            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
            DomainServices.UserService _userService = new DomainServices.UserService();

            List<UserModels.MeCodeListResponse> meCodesFound = new List<UserModels.MeCodeListResponse>();

            List<Domain.UserPayPoint> foundPaypoints;

            try
            {
                foundPaypoints = _userService.FindTopMatchingMeCodes(searchTerm, type);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Finding me codes for {0} failed. {1}",searchTerm,ex.StackTrace));
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            
            }

            foreach (Domain.UserPayPoint meCode in foundPaypoints)
            {
                meCodesFound.Add(new UserModels.MeCodeListResponse()
                {
                    userId = meCode.UserId.ToString(),
                    meCode = meCode.URI
                    
                });
            }

            return Request.CreateResponse<UserModels.FindMECodeResponse>(HttpStatusCode.OK, new UserModels.FindMECodeResponse()
            {
                searchTerm = searchTerm,
                foundUsers = meCodesFound
            });
        }



        //POST /api/users/{userId}/setup_securitypin
        [HttpPost]
        public HttpResponseMessage SetupSecurityPin(string id, UserModels.UpdateSecurityPin request)
        {
            _logger.Log(LogLevel.Info, String.Format("Setting up Security Pin for {0}", id));

            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            DomainServices.UserService _userService = new DomainServices.UserService();
            DomainServices.UserService userService = new DomainServices.UserService();

            if (request.securityPin.Length < 4)
            {
                var error = @"Invalid Security Pin";

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Pin for {0}. {1}", id, error));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Security Pin Too Short");
            }

            try
            {
                userService.SetupSecurityPin(id, request.securityPin);
            }
            catch (Exception ex)
            {
                var error = ex.Message;

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Pin for {0}. {1}", id, error));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
        //POST /api/users/validate_user
        [HttpPost]
        public HttpResponseMessage ValidateUser(UserModels.ValidateUserRequest request)
        {
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            DomainServices.UserService _userService = new DomainServices.UserService();

            User user;
            string securityQuestion = "";
            var isValid = false;
            _logger.Log(LogLevel.Debug, String.Format("Validating User {0}.", request.userName));

            try
            {
                isValid = _userService.ValidateUser(request.userName, request.password, out user);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Validating User {0}. Exception: {1}. Stack Trace {2}.", request.userName, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            if (isValid)
            {
                if (user.IsLockedOut)
                {
                    return Request.CreateResponse<UserModels.ValidateUserResponse>(HttpStatusCode.OK, new UserModels.ValidateUserResponse()
                    {
                        userId = user.UserId.ToString(),
                        isLockedOut = true,
                        securityQuestion = user.SecurityQuestion.Question
                    });
                }
                else
                {
                    bool hasACHAccount = false;
                    if (user.PaymentAccounts.Where(a => a.IsActive = true).Count() > 0)
                        hasACHAccount = true;

                    return Request.CreateResponse<UserModels.ValidateUserResponse>(HttpStatusCode.OK, new UserModels.ValidateUserResponse()
                    {
                        userId = user.UserId.ToString(),
                        mobileNumber = user.MobileNumber,
                        paymentAccountId = (user.PaymentAccounts != null && user.PaymentAccounts.Count() > 0 ? user.PaymentAccounts[0].Id.ToString() : ""),
                        setupSecurityPin = user.SetupSecurityPin,
                        upperLimit = Convert.ToInt32(user.Limit),
                        hasACHAccount = hasACHAccount,
                        hasSecurityPin = user.SetupSecurityPin,
                        setupSecurityQuestion = (user.SecurityQuestionID >= 0 ? true : false), // If SecurityQuestion setup (value not null > -1 ), return true.
                        isLockedOut = user.IsLockedOut,
                        securityQuestion = securityQuestion
                    });
                }
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Username and Password");
            
            }

        }

        //POST /api/users/{userId}/validate_security_pin
        [HttpPost]
        public HttpResponseMessage ValidateSecurityPin(string userId, UserModels.ValidateSecurityPinRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Validating Security Pin for User {0}", userId));

            var userService = new DomainServices.UserService();
            bool valid = false;
            try
            {
                valid = userService.ValidateSecurityPin(userId, request.SecurityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Validating Security Pin. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Validating Security Pin. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Validating Security Pin. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new UserModels.ValidateSecurityPinResponse()
            {
                IsValid = valid
            });

        }
        //POST /api/users/signin_withfacebook
        [HttpPost]
        public HttpResponseMessage SignInWithFacebook(UserModels.FacebookSignInRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Sign in with Facebook {0} {1}", request.deviceToken, request.oAuthToken));

            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            DomainServices.UserService _userService = new DomainServices.UserService();

            Domain.User user = null;

            bool isNewUser = false;

            //TODO: Add exception
            try
            {
                user = _userService.SignInWithFacebook(Guid.Parse(request.apiKey), request.accountId, request.emailAddress, request.firstName, request.lastName,
                    request.deviceToken, request.oAuthToken, System.DateTime.Now.AddDays(30), request.messageId, out isNewUser);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Signing in With Facebook Account {0}. Exception: {1} Stack Trace: {2}", request.accountId,
                    ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            bool hasACHAccount = false;

            var response = new UserModels.FacebookSignInResponse()
            {
                hasACHAccount = hasACHAccount,
                hasSecurityPin = user.SetupSecurityPin,
                userId = user.UserId.ToString(),
                mobileNumber = (!String.IsNullOrEmpty(user.MobileNumber) ? user.MobileNumber : ""),
                paymentAccountId = (user.PaymentAccounts != null && user.PaymentAccounts.Count() > 0 ? user.PaymentAccounts[0].Id.ToString() : ""),
                upperLimit = Convert.ToInt32(user.Limit),
                setupSecurityQuestion = (user.SecurityQuestionID >= 0 ? true : false),  // If SecurityQuestion setup (value not null > -1 ), return true.
                isLockedOut = user.IsLockedOut,
                facebookId = request.accountId
            };

            if (isNewUser)
                return Request.CreateResponse(HttpStatusCode.Created, response);
            else
                return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpPost]
        public HttpResponseMessage LinkFacebook(string id, UserModels.LinkFacebookRequest request)
        {
            //using (var ctx = new Context())
            //{
            //    DomainServices.UserService _userService = new DomainServices.UserService(ctx);
            //    DomainServices.SecurityService securityService = new DomainServices.SecurityService();

            //    Domain.User user = null;

            //    try
            //    {
            //        user = _userService.GetUserById(id);
            //    }
            //    catch (Exception ex)
            //    {
            //        string ErrorReason = String.Format("-LinkFB- Unable to find user for {0}", id);
            //        _logger.Log(LogLevel.Error, ErrorReason);

            //        var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            //        message.ReasonPhrase = ErrorReason;
            //        return message;
            //    }

            //    /*
            //     * Validate Facebook AccountId and Auth Token and Create ExpirationDate Token
            //     */
            //    if (request.apiKey == null || request.AccountId == null || request.oAuthToken == null)
            //    {
            //        string ErrorReason = String.Format("Link Facebook Failed -> Invalid Input Sent.");
            //        _logger.Log(LogLevel.Error, ErrorReason);

            //        var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            //        message.ReasonPhrase = ErrorReason;
            //        return message;
            //    }
            //    else if (ctx.Users.Select(u => u.FacebookUser).Where(f => f.FBUserID.Equals(request.AccountId)).Count() > 0)
            //    {
            //        // Account in use... return an error.
            //        string ErrorReason = String.Format("Link Facebook Failed -> FB Account [{0}] already linked to another account.", id);
            //        _logger.Log(LogLevel.Error, ErrorReason);

            //        var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            //        message.ReasonPhrase = ErrorReason;
            //        return message;
            //    }


            //    if (_userService.LinkFacebook(request.apiKey, id, request.AccountId, request.oAuthToken, System.DateTime.Now.AddDays(7.0)))
            //        return new HttpResponseMessage(HttpStatusCode.Created);
            //    else
            //    {
            //        string ErrorReason = String.Format("Link Facebook Failed -> User Service to add FB User Failed.");
            //        _logger.Log(LogLevel.Error, ErrorReason);

            //        var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
            //        message.ReasonPhrase = ErrorReason;
            //        return message;
            //    }
            //}

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        //api/users/verify_paypoint
        [HttpPost]
        public HttpResponseMessage VerifyPayPoint(UserModels.ValidatePayPointRequest request)
        {
            var userService = new DomainServices.UserService();
            HttpResponseMessage responseMessage;

            bool result = false;

            //TODO: Add exception handling
            try
            {
                result = userService.VerifyPayPoint(request.PayPointVerificationId);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
