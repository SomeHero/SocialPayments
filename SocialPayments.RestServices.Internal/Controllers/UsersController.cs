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
using SocialPayments.DomainServices.Interfaces;
using System.Threading.Tasks;
using Amazon.S3.Model;
using System.IO;
using System.Text;
using SocialPayments.Domain.ExtensionMethods;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UsersController : ApiController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private Guid ApiKey = new Guid("bda11d91-7ade-4da1-855d-24adfe39d174");

        // GET /api/user
        public UserModels.PagedResults Get(int take, int skip, int page, int pageSize)
        {
            using (var ctx = new Context())
            {
                var totalRecords = ctx.Messages.Count();

                var users = ctx.Users.Select(u => u)
                    .OrderBy(m => m.CreateDate)
                    .Skip(skip)
                    .Take(take)
                    .ToList();

                return new UserModels.PagedResults()
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
                    })
                };
            }
        }

        // GET /api/users/5
        public HttpResponseMessage<UserModels.UserResponse> Get(string id)
        {
            _logger.Log(LogLevel.Info, String.Format("Getting User {0}", id));

            Context _ctx = new Context();
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
            DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
            DomainServices.MessageServices messageServices = new DomainServices.MessageServices(_ctx); ;

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

            var sentPayments = _ctx.Messages
                    .Where(m => m.SenderId.Equals(user.UserId) && m.MessageTypeValue.Equals((int)MessageType.Payment));

            if (sentPayments.Count() > 0)
                sentTotal = sentPayments.Sum(m => m.Amount);

            var receivedPayments = _ctx.Messages
                    .Where(m => m.RecipientId.Value.Equals(user.UserId) && m.MessageTypeValue.Equals((int)MessageType.Payment));

            if (receivedPayments.Count() > 0)
                receivedTotal = receivedPayments.Sum(m => m.Amount);

            _logger.Log(LogLevel.Info, String.Format("User Mobile Number {0}", user.MobileNumber));
            _logger.Log(LogLevel.Info, String.Format("# of paypoints {0}", user.PayPoints.Count));

            string userName = _userService.GetSenderName(user);
            UserModels.UserResponse userResponse = null;

            var numberOfPayStreamUpdates = messageServices.GetNumberOfPaystreamUpdates(user);
            var outstandingMessages = messageServices.GetOutstandingMessage(user);

            var newCount = messageServices.GetNewMessages(user);
            var pendingCount = messageServices.GetPendingMessages(user);

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
                    pendingMessageCount = 1
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

            Context _ctx = new Context();
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
            DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

            var memberRole = _ctx.Roles.FirstOrDefault(r => r.RoleName == "Member");

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
                _logger.Log(LogLevel.Error, string.Format("Exception registering user {0}. Exception {1}.", request.emailAddress, ex.Message));

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

            Context _ctx = new Context();
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
            DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

            var user = _userService.GetUserById(id);

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.ImageUrl = request.ImageUrl;

            var firstNameAttribute = _ctx.UserAttributes
                .FirstOrDefault(a => a.AttributeName == "FirstName");

            if (firstNameAttribute != null)
            {
                user.UserAttributes.Add(new UserAttributeValue()
                {
                    id = Guid.NewGuid(),
                    UserAttributeId = firstNameAttribute.Id,
                    AttributeValue = request.FirstName
                });
            }
            var lastNameAttribute = _ctx.UserAttributes
                .FirstOrDefault(a => a.AttributeName == "LastName");

            if (firstNameAttribute != null)
            {
                user.UserAttributes.Add(new UserAttributeValue()
                {
                    id = Guid.NewGuid(),
                    UserAttributeId = lastNameAttribute.Id,
                    AttributeValue = request.LastName
                });
            }


            try
            {
                _userService.UpdateUser(user);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Info, String.Format("Unhandled Expression Updating User {0}. {1}", id, ex.Message));

                var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                responseMessage.ReasonPhrase = "Unable to update user";

                return responseMessage;
            }

            _logger.Log(LogLevel.Info, String.Format("Completed Personalize User"));

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
            Context _ctx = new Context();
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
            DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
            DomainServices.UserService userService = new DomainServices.UserService(_ctx);

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

            Context _ctx = new Context();
            DomainServices.UserService userService = new DomainServices.UserService(_ctx);

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


        public HttpResponseMessage ChangePassword(string id, UserModels.ChangePasswordRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Changing password for: {0}", id));

            Context _ctx = new Context();
            DomainServices.UserService userService = new DomainServices.UserService(_ctx);
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();

            var user = userService.GetUserById(id);

            if (!securityService.Decrypt(user.Password).Equals(request.currentPassword))
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = "Password doesn't match";
                return message;
            }

            user.Password = securityService.Encrypt(request.newPassword);
            userService.UpdateUser(user);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        //POST api/users/reset_password
        public HttpResponseMessage ResetPassword(UserModels.ResetPasswordRequest request)
        {
            using (var _ctx = new Context())
            {
                DomainServices.UserService userService = new DomainServices.UserService(_ctx);
                DomainServices.ValidationService validateService = new DomainServices.ValidationService();

                if (String.IsNullOrEmpty(request.emailAddress) || !validateService.IsEmailAddress(request.emailAddress))
                {

                    var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    message.ReasonPhrase = "Invalid Email Address";

                    return message;
                }

                var user = userService.FindUserByEmailAddress(request.emailAddress);

                if (user == null)
                {
                    var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    message.ReasonPhrase = "Invalid User";

                    return message;
                }
                else if (!validateService.IsEmailAddress(user.UserName))
                {
                    var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    message.ReasonPhrase = "Facebook accounts cannot reset their password. Sign in with Facebook to continue";

                    return message;
                }
                else
                {
                    userService.SendResetPasswordLink(user, ApiKey);
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }
        public HttpResponseMessage SendEmail(string id, string apiKey, UserModels.SendEmailRequest request)
        {
            using (var ctx = new Context())
            {
                var emailService = new DomainServices.EmailService(ctx);
                var userService = new DomainServices.UserService(ctx);

                var user = userService.GetUserById(id);

                emailService.SendEmail(user.EmailAddress, request.Subject, request.TemplateName, request.ReplacementElements);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }
        //POST /api/users/{userId}/registerpushnotifications
        public HttpResponseMessage RegisterForPushNotifications(string id, UserModels.PushNotificationRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Registering push notifications for Android for: {0}", id));

            Context _ctx = new Context();
            DomainServices.UserService userService = new DomainServices.UserService(_ctx);

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

            Context _ctx = new Context();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
            DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
            DomainServices.MessageServices _messageService = new DomainServices.MessageServices(_ctx);

            User user = null;
            try
            {
                user = _userService.GetUserById(id);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unable to find user [RefreshHomepage] by [{0}]", id));
                return new HttpResponseMessage<UserModels.HomepageRefreshReponse>(HttpStatusCode.MethodNotAllowed); // 405
            }


            //try
            //{
            List<Domain.Message> recentPayments = _messageService.GetQuickSendPayments(user);
            List<UserModels.QuickSendUserReponse> quickSends = new List<UserModels.QuickSendUserReponse>();

            foreach (Message msg in recentPayments)
            {

                string recipName;
                var recipient = _userService.GetUser(msg.RecipientUri);
                int recipientType = -1;

                if (recipient == null)
                {
                    _logger.Log(LogLevel.Error, "RecipientURI: {0} isFacebookRecipient? {1}", msg.RecipientUri, msg.RecipientUri.Substring(0, 3).Equals("fb_") ? "YES" : "NO" );

                    if (msg.RecipientUri.Substring(0, 3).Equals("fb_"))
                    {
                        _logger.Log(LogLevel.Error,"First: {0} Last: {1} Name: {2}",msg.recipientFirstName, msg.recipientLastName, msg.RecipientName);
                        recipName = msg.RecipientName;
                    }
                    else
                    {
                        recipName = msg.RecipientUri;
                    }

                    recipientType = 0;
                }
                else if ( recipient.Merchant != null )
                {
                    recipName = _userService.GetSenderName(recipient);
                    recipientType = recipient.UserTypeId;

                    if (recipient.Merchant.MerchantTypeValue == 2)
                        recipientType = 2;
                    else if (recipient.Merchant.MerchantTypeValue == 1)
                        recipientType = 1;
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
                }
                string imageUri = null;

                if (msg.Recipient != null)
                    imageUri = msg.Recipient.ImageUrl;

                quickSends.Add(new UserModels.QuickSendUserReponse()
                {
                    userUri = msg.RecipientUri,
                    userName = recipName,
                    userImage = imageUri,
                    userType = recipientType
                });
            }

            var response = new UserModels.HomepageRefreshReponse()
            {
                userId = user.UserId.ToString(),
                numberOfIncomingNotifications = _messageService.GetNewMessages(user).Count,
                numberOfOutgoingNotifications = _messageService.GetPendingMessages(user).Count,
                quickSendContacts = quickSends
            };

            return new HttpResponseMessage<UserModels.HomepageRefreshReponse>(response, HttpStatusCode.OK);
            /*
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, String.Format("Something went wrong getting quicksend for {0}, {1}", id, ex.Message));
            var message = new HttpResponseMessage<UserModels.HomepageRefreshReponse>(HttpStatusCode.BadRequest);
            message.ReasonPhrase = String.Format("Something went wrong getting quicksend for {0}", id);
            return message;
        }
             * */
        }


        //POST /api/users/{userId}/setup_securitypin
        public HttpResponseMessage SetupSecurityPin(string id, UserModels.UpdateSecurityPin request)
        {
            _logger.Log(LogLevel.Info, String.Format("Setting up Security Pin for {0}", id));

            Context _ctx = new Context();
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
            DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
            DomainServices.UserService userService = new DomainServices.UserService(_ctx);

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
            Context _ctx = new Context();
            
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

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

            Context _ctx = new Context();
            DomainServices.SecurityService securityService = new DomainServices.SecurityService();
            DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
            IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
            DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

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
                isLockedOut = user.IsLockedOut
            };

            if (isNewUser)
                return new HttpResponseMessage<UserModels.FacebookSignInResponse>(response, HttpStatusCode.Created);
            else
                return new HttpResponseMessage<UserModels.FacebookSignInResponse>(response, HttpStatusCode.OK);
        }

        public HttpResponseMessage LinkFacebook(string id, UserModels.LinkFacebookRequest request)
        {
            using (var ctx = new Context())
            {
                DomainServices.UserService _userService = new DomainServices.UserService(ctx);

                Domain.User user = null;

                try
                {
                    user = _userService.GetUserById(id);
                }
                catch ( Exception ex )
                {
                    string ErrorReason = String.Format("-LinkFB- Unable to find user for {0}", id);
                    _logger.Log(LogLevel.Error, ErrorReason);

                    var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    message.ReasonPhrase = ErrorReason;
                    return message;
                }

                /*
                 * Validate Facebook AccountId and Auth Token and Create ExpirationDate Token
                 */
                if (request.apiKey == null || request.AccountId == null || request.oAuthToken == null)
                {
                    string ErrorReason = String.Format("Link Facebook Failed -> Invalid Input Sent.");
                    _logger.Log(LogLevel.Error, ErrorReason);

                    var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    message.ReasonPhrase = ErrorReason;
                    return message;
                }
                else if (ctx.Users.Select(u => u.FacebookUser).Where(f => f.FBUserID.Equals(request.AccountId)).Count() > 0)
                {
                    // Account in use... return an error.
                    string ErrorReason = String.Format("Link Facebook Failed -> FB Account [{0}] already linked to another account.", id);
                    _logger.Log(LogLevel.Error, ErrorReason);

                    var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    message.ReasonPhrase = ErrorReason;
                    return message;
                }
                
                
                if ( _userService.LinkFacebook(request.apiKey, id, request.AccountId, request.oAuthToken, System.DateTime.Now.AddDays(7.0)) )
                    return new HttpResponseMessage(HttpStatusCode.Created);
                else
                {
                    string ErrorReason = String.Format("Link Facebook Failed -> User Service to add FB User Failed.");
                    _logger.Log(LogLevel.Error, ErrorReason);

                    var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    message.ReasonPhrase = ErrorReason;
                    return message;
                }
            }
        }

        // DELETE /api/user/5
        public void Delete(int id)
        {
        }
    }
}
