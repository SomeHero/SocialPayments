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
        public HttpResponseMessage<UserModels.PagedResults> Get(int take, int skip, int page, int pageSize)
        {
            HttpResponseMessage<UserModels.PagedResults> response = null;
            var userServices = new DomainServices.UserService();
            List<Domain.User> users = null;
            int totalRecords = 0;

            try
            {
                users = userServices.GetPagedUsers(take, skip, page, pageSize, out totalRecords);
            }
            catch (Exception ex)
            {

            }

            response = new HttpResponseMessage<UserModels.PagedResults>(new UserModels.PagedResults()
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
                    userStatus = u.UserStatus.ToString()
                }).ToList()
            }, HttpStatusCode.OK);

            return response;
        }
        // GET /api/users/5
        public HttpResponseMessage<UserModels.UserResponse> Get(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Getting User {0}", id));

            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            DomainServices.UserService _userService = new DomainServices.UserService();
            DomainServices.MessageServices messageServices = new DomainServices.MessageServices(); ;
            UserModels.UserResponse userResponse = null;

            User user = null;

            try
            {
                user = _userService.GetUserById(id);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Info, String.Format("Unable to find user by id {0}. {1}", id, ex.Message));
            }

            if (user == null)
            {
                var message = new HttpResponseMessage<UserModels.UserResponse>(HttpStatusCode.NotFound);
                message.ReasonPhrase = "User Not Found";

                return message;
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

            string userName = _userService.GetSenderName(user);
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
                    instantLimit = _userService.GetUserInstantLimit(user),
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
                        Nickname = a.Nickname,
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
                    numberOfPaystreamUpdates = numberOfPayStreamUpdates,
                    newMessageCount = 1,
                    pendingMessageCount = 1,
                    facebookId = fbId,
                    facebookToken = fbToken
                };
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                _logger.ErrorException(String.Format("Unhandled exception formatting User Response {0}. {1}", id, errorMessage), ex);

                throw new HttpResponseException(errorMessage, HttpStatusCode.InternalServerError);
            }

            return new HttpResponseMessage<UserModels.UserResponse>(userResponse, HttpStatusCode.OK);
        }

        // POST /api/user
        public HttpResponseMessage<UserModels.SubmitUserResponse> Post(UserModels.SubmitUserRequest request)
        {
            _logger.Log(LogLevel.Error, string.Format("Registering User  {0}", request.userName));

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
            user = _userService.FindUserByEmailAddress(request.userName);

            if (user != null)
            {
                var errorMessage = new HttpResponseMessage<UserModels.SubmitUserResponse>(HttpStatusCode.BadRequest);
                errorMessage.ReasonPhrase = String.Format("The email address {0} is already registered.", request.emailAddress);

                return errorMessage;
            }

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

                user = _userService.AddUser(Guid.Parse(request.apiKey), request.userName, request.password, request.emailAddress,
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
                var message = new HttpResponseMessage<UserModels.SubmitUserResponse>(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = String.Format("Unable to register user. {0}", ex.Message);

                return message;
            }

            var responseMessage = new UserModels.SubmitUserResponse()
            {
                userId = user.UserId.ToString()
            };

            return new HttpResponseMessage<UserModels.SubmitUserResponse>(responseMessage, HttpStatusCode.Created);
        }

        // POST /api/user/{id}/personalize_user
        public HttpResponseMessage PersonalizeUser(string id, UserModels.PersonalizeUserRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Start Personalize User"));

            DomainServices.UserService userService = new DomainServices.UserService();
            HttpResponseMessage response = null;
            try
            {
                userService.PersonalizeUser(id, request.FirstName, request.LastName, request.ImageUrl);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Personalizing User {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage<UserModels.ValidatePasswordResetAttemptResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Personalizing User {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage<UserModels.ValidatePasswordResetAttemptResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Personalizing User {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.ValidatePasswordResetAttemptResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        // POST /api/users/{id}/upload_member_image
        public Task<HttpResponseMessage<FileUploadResponse>> UploadMemberImage([FromUri] string id)
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(
                    Request.CreateResponse(HttpStatusCode.UnsupportedMediaType));
            }

            var provider = new RenamingMultipartFormDataStreamProvider(String.Format(@"{0}\{1}", @"c:\memberImages", id));

            // Read the form data and return an async task.
            var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage<FileUploadResponse>>(readTask =>
                {
                    if (readTask.IsFaulted || readTask.IsCanceled)
                    {
                        return new HttpResponseMessage<FileUploadResponse>(HttpStatusCode.InternalServerError);
                    }

                    var fileName = "";
                    // This illustrates how to get the file names.
                    foreach (var file in provider.BodyPartFileNames)
                    {
                        _logger.Log(LogLevel.Info, "Client file name: " + file.Key);
                        _logger.Log(LogLevel.Info, "Server file path: " + file.Value);

                        fileName = file.Value;
                    }

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

                    return new HttpResponseMessage<FileUploadResponse>(
                        new FileUploadResponse()
                        {
                            ImageUrl = String.Format("http://memberimages.paidthx.com/{0}/image1.png", id)
                        }, HttpStatusCode.Created);
                });

            return task;
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
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = "Security Pin doesn't match";
                return message;
            }
            if (request.newSecurityPin.Length < 4)
            {
                var error = @"Invalid Security Pin";

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Pin for {0}. {1}", id, error));

                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = error;

                return message;
            }

            user.SecurityPin = securityService.Encrypt(request.newSecurityPin);
            userService.UpdateUser(user);

            return new HttpResponseMessage(HttpStatusCode.OK);
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

                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = error;

                return message;
            }
            if (request.questionAnswer.Length < 1)
            {
                var error = @"Invalid Security Question Length";

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Question for {0}. {1}", id, error));

                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = error;

                return message;
            }

            try
            {
                userService.SetupSecurityQuestion(id, request.questionId, request.questionAnswer);
            }
            catch (Exception ex)
            {
                var error = ex.Message;

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Question for {0}. {1}", id, error));

                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = error;

                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        //POST /api/users/validate_passwordreset
        public HttpResponseMessage<UserModels.ValidatePasswordResetAttemptResponse> ValidatePasswordResetAttempt(UserModels.ValidatePasswordResetAttemptRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Validating Password Reset Attempt {0}", request.ResetPasswordAttemptId));

            DomainServices.UserService userService = new DomainServices.UserService();
            Domain.PasswordResetAttempt passwordResetAttempt = null;
            HttpResponseMessage<UserModels.ValidatePasswordResetAttemptResponse> response = null;

            try
            {
                passwordResetAttempt = userService.ValidatePasswordResetAttempt(request.ResetPasswordAttemptId);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Validating Password Reset Attempt {0}. Exception {1}", request.ResetPasswordAttemptId, ex.Message));

                response = new HttpResponseMessage<UserModels.ValidatePasswordResetAttemptResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Validating Password Reset Attempt {0}. Exception {1}", request.ResetPasswordAttemptId, ex.Message));

                response = new HttpResponseMessage<UserModels.ValidatePasswordResetAttemptResponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Validating Password Reset Attempt {0}. Exception {1}. Stack Trace {2}", request.ResetPasswordAttemptId, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.ValidatePasswordResetAttemptResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<UserModels.ValidatePasswordResetAttemptResponse>(new UserModels.ValidatePasswordResetAttemptResponse() {
              HasSecurityQuestion = !(passwordResetAttempt.User.SecurityQuestion == null),
              SecurityQuestion = passwordResetAttempt.User.SecurityQuestion.Question,
              UserId = passwordResetAttempt.UserId.ToString()
            }, HttpStatusCode.OK);

            return response;
        }
        public HttpResponseMessage ChangePassword(string id, UserModels.ChangePasswordRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Changing password for: {0}", id));

            DomainServices.UserService userService = new DomainServices.UserService();
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            HttpResponseMessage response = null;
            try
            {
                userService.ChangePassword(id, request.currentPassword, request.newPassword);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Changing Password for User {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception  Changing Password for User  {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Changing Password for User  {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }

        //POST api/users/reset_password
        public HttpResponseMessage ResetPassword(UserModels.ResetPasswordRequest request)
        {
            HttpResponseMessage response = null;
            try
            {
                //userServices.ResetPassword(request.userId, request.securityQuestion, request.newPassword);
            }
            catch (Exception ex)
            {

            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }
        //POST api/users/forgot_password
        public HttpResponseMessage ForgotPassword(UserModels.ForgotPasswordRequest request)
        {
            var validateService = new DomainServices.ValidationService();

            HttpResponseMessage response = null;
            var userService = new DomainServices.UserService();

            try
            {
                if (!validateService.IsEmailAddress(request.userName))
                    throw new SocialPayments.DomainServices.CustomExceptions.BadRequestException("Facebook accounts cannot reset their password. Sign in with Facebook to continue");

                userService.SendResetPasswordLink(request.apiKey, request.userName);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Sending Forgot Password Email to {0}. Exception {1}", request.userName, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Sending Forgot Password Email to {0}. Exception {1}", request.userName, ex.Message));

                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Sending Forgot Password Email to {0}. Exception {1}. Stack Trace {2}", request.userName, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
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
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    var error = ex.Message;

                    _logger.Log(LogLevel.Error, String.Format("Unable to register push notifications for {0}. {1}", id, error));

                    var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    message.ReasonPhrase = error;

                    return message;
                }
            }
            else
            {
                var error = "DeviceTokens match, no need to reregister for Android push.";

                _logger.Log(LogLevel.Error, String.Format("Unable to register push notifications for {0}. {1}", id, error));

                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = error;

                return message;
            }
        }

        // GET /api/users/{userId}/refresh_homepage
        public HttpResponseMessage<UserModels.HomepageRefreshReponse> RefreshHomepageInformation(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Refreshing homepage for {0}", id));

            List<Domain.Message> recentPayments = null;
            var userService = new DomainServices.UserService();
            var messageService = new DomainServices.MessageServices();
            HttpResponseMessage<UserModels.HomepageRefreshReponse> response = null;
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

                    if (msg.RecipientId == null)
                    {
                        _logger.Log(LogLevel.Error, "RecipientURI: {0} isFacebookRecipient? {1}", msg.RecipientUri, msg.RecipientUri.Substring(0, 3).Equals("fb_") ? "YES" : "NO");

                        if (msg.RecipientUri.Substring(0, 3).Equals("fb_"))
                        {
                            _logger.Log(LogLevel.Error, "First: {0} Last: {1} Name: {2}", msg.recipientFirstName, msg.recipientLastName, msg.RecipientName);


                            if (msg.RecipientName != null && msg.RecipientName.Length > 0)
                                recipName = msg.RecipientName;
                            else
                                recipName = msg.recipientFirstName + " " + msg.recipientLastName;

                            quickSends.Add(new UserModels.QuickSendUserReponse()
                            {
                                userUri = msg.RecipientUri,
                                userName = recipName,
                                userFirstName = msg.recipientFirstName,
                                userLastName = msg.recipientLastName,
                                userImage = String.Format("http://graph.facebook.com/{0}/picture",msg.RecipientUri.Substring(3)),
                                userType = 0 // Normal User
                            });
                        }
                        else
                        {
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
                    }
                    else if (msg.Recipient.Merchant != null)
                    {
                        _logger.Log(LogLevel.Error, "Found a merchant for {0}", msg.Recipient.Merchant.Id.ToString());

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
                        if (msg.RecipientUri.Substring(0, 3).Equals("fb_"))
                        {
                            _logger.Log(LogLevel.Error, "First: {0} Last: {1} Name: {2}", msg.recipientFirstName, msg.recipientLastName, msg.RecipientName);

                            recipName = String.Format("{0} {1}", msg.recipientFirstName, msg.recipientLastName);
                        }
                        else
                        {
                            recipName = msg.Recipient.SenderName;
                        }

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
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Refreshing Home Page Information {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage<UserModels.HomepageRefreshReponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Refreshing Home Page Information {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage<UserModels.HomepageRefreshReponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Refreshing Home Page Information {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.HomepageRefreshReponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            try
            {
                numberOfIncomingNotifications = messageService.GetNumberOfNewMessages(id);

            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Refreshing Home Page Information {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage<UserModels.HomepageRefreshReponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Refreshing Home Page Information {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage<UserModels.HomepageRefreshReponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Refreshing Home Page Information {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.HomepageRefreshReponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            try
            {
                numberOfOutgoingNotifications = messageService.GetNumberOfPendingMessages(id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Refreshing Home Page Information {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage<UserModels.HomepageRefreshReponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Refreshing Home Page Information {0}. Exception {1}", id, ex.Message));

                response = new HttpResponseMessage<UserModels.HomepageRefreshReponse>(HttpStatusCode.BadRequest);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Refreshing Home Page Information {0}. Exception {1}. Stack Trace {2}", id, ex.Message, ex.StackTrace));

                response = new HttpResponseMessage<UserModels.HomepageRefreshReponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            var results = new UserModels.HomepageRefreshReponse()
            {
                userId = id,
                numberOfIncomingNotifications = numberOfIncomingNotifications,
                numberOfOutgoingNotifications = numberOfOutgoingNotifications,
                quickSendContacts = quickSends
            };

            response = new HttpResponseMessage<UserModels.HomepageRefreshReponse>(results, HttpStatusCode.OK);
            return response;
        }


        // GET /api/users/{userId}/find_mecodes
        public HttpResponseMessage<UserModels.FindMECodeResponse> GetMatchingMECodesWithSearchTerm (string searchTerm)
        {
            _logger.Log(LogLevel.Info, String.Format("Finding ME Codes maching {0}", searchTerm));

            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
            DomainServices.UserService _userService = new DomainServices.UserService();

            List<UserModels.MeCodeListResponse> meCodesFound = new List<UserModels.MeCodeListResponse>();

            List<Domain.UserPayPoint> foundPaypoints;

            try
            {
                foundPaypoints = _userService.FindTopMatchingMeCodes(searchTerm);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Finding me codes for {0} failed. {1}",searchTerm,ex.StackTrace));
                return new HttpResponseMessage<UserModels.FindMECodeResponse>(HttpStatusCode.InternalServerError); // 500
            }

            foreach (Domain.UserPayPoint meCode in foundPaypoints)
            {
                meCodesFound.Add(new UserModels.MeCodeListResponse()
                {
                    userId = meCode.UserId.ToString(),
                    meCode = meCode.URI
                    
                });
            }

            if ( meCodesFound.Count() == 0 )
                return new HttpResponseMessage<UserModels.FindMECodeResponse>(HttpStatusCode.InternalServerError); // 500

            var response = new UserModels.FindMECodeResponse()
            {
                searchTerm = searchTerm,
                foundUsers = meCodesFound
            };

            return new HttpResponseMessage<UserModels.FindMECodeResponse>(response, HttpStatusCode.OK);
        }



        //POST /api/users/{userId}/setup_securitypin
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

                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = error;

                return message;
            }

            try
            {
                userService.SetupSecurityPin(id, request.securityPin);
            }
            catch (Exception ex)
            {
                var error = ex.Message;

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Pin for {0}. {1}", id, error));

                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = error;

                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        //POST /api/users/validate_user
        public HttpResponseMessage<UserModels.ValidateUserResponse> ValidateUser(UserModels.ValidateUserRequest request)
        {
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            DomainServices.UserService _userService = new DomainServices.UserService();

            HttpResponseMessage<UserModels.ValidateUserResponse> responseMessage;

            User user;
            var isValid = false;

            try
            {
                isValid = _userService.ValidateUser(request.userName, request.password, out user);
            }
            catch (Exception ex)
            {
                responseMessage = new HttpResponseMessage<UserModels.ValidateUserResponse>(HttpStatusCode.InternalServerError);
                responseMessage.ReasonPhrase = ex.Message;

                return responseMessage;
            }

            if (isValid)
            {
                bool hasACHAccount = false;
                if (user.PaymentAccounts.Where(a => a.IsActive = true).Count() > 0)
                    hasACHAccount = true;

                var message = new UserModels.ValidateUserResponse()
                {
                    userId = user.UserId.ToString(),
                    mobileNumber = user.MobileNumber,
                    paymentAccountId = (user.PaymentAccounts != null && user.PaymentAccounts.Count() > 0 ? user.PaymentAccounts[0].Id.ToString() : ""),
                    setupSecurityPin = user.SetupSecurityPin,
                    upperLimit = Convert.ToInt32(user.Limit),
                    hasACHAccount = hasACHAccount,
                    hasSecurityPin = user.SetupSecurityPin,
                    setupSecurityQuestion = (user.SecurityQuestionID >= 0 ? true : false), // If SecurityQuestion setup (value not null > -1 ), return true.
                    isLockedOut = user.IsLockedOut
                };

                responseMessage = new HttpResponseMessage<UserModels.ValidateUserResponse>(message, HttpStatusCode.OK);

                return responseMessage;
            }
            else
            {
                responseMessage = new HttpResponseMessage<UserModels.ValidateUserResponse>(HttpStatusCode.Forbidden);
                responseMessage.ReasonPhrase = "Invalid Username and Password";

                return responseMessage;
            }

        }

        //POST /api/users/signin_withfacebook
        public HttpResponseMessage<UserModels.FacebookSignInResponse> SignInWithFacebook(UserModels.FacebookSignInRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Sign in with Facebook {0} {1}", request.deviceToken, request.oAuthToken));

            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            DomainServices.UserService _userService = new DomainServices.UserService();

            Domain.User user = null;

            bool isNewUser = false;

            try
            {
                user = _userService.SignInWithFacebook(Guid.Parse(request.apiKey), request.accountId, request.emailAddress, request.firstName, request.lastName,
                    request.deviceToken, request.oAuthToken, System.DateTime.Now.AddDays(30), request.messageId, out isNewUser);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Fatal, String.Format("Exception Signing in With Facebook. Account {0}", request.accountId));

                var message = new HttpResponseMessage<UserModels.FacebookSignInResponse>(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = ex.Message;

                return message;
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
                return new HttpResponseMessage<UserModels.FacebookSignInResponse>(response, HttpStatusCode.Created);
            else
                return new HttpResponseMessage<UserModels.FacebookSignInResponse>(response, HttpStatusCode.OK);
        }

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

        // DELETE /api/user/5
        public void Delete(int id)
        {
        }
        //api/users/verify_paypoint
        public HttpResponseMessage VerifyPayPoint(UserModels.ValidatePayPointRequest request)
        {
            var userService = new DomainServices.UserService();
            HttpResponseMessage responseMessage;

            bool result = false;
            try
            {
                result = userService.VerifyPayPoint(request.PayPointVerificationId);
            }
            catch (Exception ex)
            {
                responseMessage = Request.CreateResponse(HttpStatusCode.BadRequest);
                responseMessage.ReasonPhrase = ex.Message;

                return responseMessage;
            }

            responseMessage = Request.CreateResponse(HttpStatusCode.OK);
            responseMessage.ReasonPhrase = String.Format("Thanks. You have completed verification.  You can now begin to accept payments using this pay point.");

            return responseMessage;
        }
    }
}
