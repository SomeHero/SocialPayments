﻿using System;
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

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UsersController : ApiController
    {
        private Context _ctx = new Context();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private DomainServices.SecurityService securityService = new DomainServices.SecurityService();
        private DomainServices.FormattingServices formattingService = new DomainServices.FormattingServices();
        private static IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
       
        // GET /api/user
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/users/5
        public HttpResponseMessage<UserModels.UserResponse> Get(string id)
        {
             _logger.Log(LogLevel.Info, String.Format("Getting User {0}", id));

             DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

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

            UserModels.UserResponse userResponse = null;

            try
            {
                userResponse = new UserModels.UserResponse()
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
            } 
            catch(Exception ex)
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
                    request.deviceToken);

                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, string.Format("Exception registering user {0}. Exception {1}.", request.emailAddress, ex.Message));

                var message = new HttpResponseMessage<UserModels.SubmitUserResponse>(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = String.Format("Unable to register user. {0}", ex.Message);

                return message;
            }

            _amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["UserPostedTopicARN"], "New User Account Created", user.UserId.ToString());

            var responseMessage = new UserModels.SubmitUserResponse()
            {
                userId = user.UserId.ToString()
            };

            return new HttpResponseMessage<UserModels.SubmitUserResponse>(responseMessage, HttpStatusCode.Created);
        }
        
        // PUT /api/user/5
        public void Put(int id, string value)
        {

        }
        [HttpPost]
        public HttpResponseMessage ChangeSecurityPin(string id, UserModels.ChangeSecurityPinRequest request)
        {
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

        //POST /api/users/{userId}/setup_securitypin
        public HttpResponseMessage SetupSecurityPin(string id, UserModels.UpdateSecurityPin request)
        {
            _logger.Log(LogLevel.Info, String.Format("Setting up Security Pin for {0}", id));

            DomainServices.UserService userService = new DomainServices.UserService(_ctx);

            if(request.securityPin.Length < 4)
            {
                var error = @"Invalid Security Pin";

                _logger.Log(LogLevel.Error, String.Format("Unable to Setup Security Pin for {0}. {1}", id, error));

                var message =  new HttpResponseMessage(HttpStatusCode.BadRequest);
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
            var userService = new DomainServices.UserService(_ctx);

            User user;
            var isValid = userService.ValidateUser(request.userName, request.password, out user);

            bool hasACHAccount = false;
            if (user.PaymentAccounts.Where(a => a.IsActive = true).Count() > 0)
                hasACHAccount = true;

            if (isValid){
                var message = new UserModels.ValidateUserResponse()
                {
                    userId = user.UserId.ToString(),
                    mobileNumber = user.MobileNumber,
                    paymentAccountId = (user.PaymentAccounts != null && user.PaymentAccounts.Count() > 0 ? user.PaymentAccounts[0].Id.ToString() : ""),
                    setupSecurityPin = user.SetupSecurityPin,
                    upperLimit = Convert.ToInt32(user.Limit),
                    hasACHAccount = hasACHAccount,
                    hasSecurityPin = user.SetupSecurityPin
                };

                return new HttpResponseMessage<UserModels.ValidateUserResponse>(message, HttpStatusCode.OK);
            }
            else
                return new HttpResponseMessage<UserModels.ValidateUserResponse>(HttpStatusCode.Forbidden);
        }

        //POST /api/users/signin_withfacebook
        public HttpResponseMessage<UserModels.FacebookSignInResponse> SignInWithFacebook(UserModels.FacebookSignInRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Sign in with Facebook {0}", request.deviceToken));

            DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
       
            Domain.User user = null;

            try
            {
                user = _userService.SignInWithFacebook(Guid.Parse(request.apiKey), request.accountId, request.emailAddress, request.firstName, request.lastName,
                    request.deviceToken);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Fatal, String.Format("Exception Signing in With Facebook. Account {0}", request.accountId));

                var message = new HttpResponseMessage<UserModels.FacebookSignInResponse>(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = ex.Message;

                return message;
            }

            bool hasACHAccount = false;

            if (user.PaymentAccounts.Where(a => a.IsActive = true).Count() > 0)
                hasACHAccount = true;

            var response = new UserModels.FacebookSignInResponse() {
                hasACHAccount = hasACHAccount,
                hasSecurityPin = user.SetupSecurityPin,
                userId = user.UserId.ToString(),
                mobileNumber = (!String.IsNullOrEmpty(user.MobileNumber) ? user.MobileNumber : ""),
                paymentAccountId = (user.PaymentAccounts != null && user.PaymentAccounts.Count() > 0 ? user.PaymentAccounts[0].Id.ToString() : ""),
                upperLimit = Convert.ToInt32(user.Limit)
            };

            return new HttpResponseMessage<UserModels.FacebookSignInResponse>(response, HttpStatusCode.OK);
        }

        // DELETE /api/user/5
        public void Delete(int id)
        {
        }
    }
}