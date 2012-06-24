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
using System.Configuration;
using SocialPayments.DomainServices.Interfaces;
using SocialPayments.DataLayer.Interfaces;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserPaymentAccountsController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        
        // GET /api/{userId}/paymentaccounts
        public HttpResponseMessage<List<AccountModels.AccountResponse>> Get(string userId)
        {
            using (var _ctx = new Context())
            {
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
                IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

                var user = _userService.GetUserById(userId);

                if (user == null)
                {
                    var message = new HttpResponseMessage<List<AccountModels.AccountResponse>>(HttpStatusCode.NotFound);

                    message.ReasonPhrase = String.Format("The user id {0} specified in the request is not valid", userId);
                    return message;
                }

                var accounts = _ctx.PaymentAccounts
                    .Where(a => a.UserId == user.UserId).ToList();

                var accountResponse = accounts.Select(a => new AccountModels.AccountResponse()
                {
                    AccountNumber = GetLastFour(_securityService.Decrypt(a.AccountNumber)),
                    AccountType = a.AccountType.ToString(),
                    NameOnAccount = _securityService.Decrypt(a.NameOnAccount),
                    Id = a.Id.ToString(),
                    RoutingNumber = _securityService.Decrypt(a.RoutingNumber),
                    UserId = a.UserId.ToString()
                }).ToList<AccountModels.AccountResponse>();

                

                return new HttpResponseMessage<List<AccountModels.AccountResponse>>(accountResponse, HttpStatusCode.OK);
            }
         }

        // GET /api/{userId}/paymentaccounts/{id}
        //public HttpResponseMessage<AccountModels.AccountResponse> Get(string userId, string id)
        //{
        //    return new HttpResponseMessage<AccountModels.AccountResponse>(

        //}

        // POST /api/{userId}/paymentaccounts
        public HttpResponseMessage<AccountModels.SubmitAccountResponse> Post(string userId, AccountModels.SubmitAccountRequest request)
        {
            using (var _ctx = new Context())
            {
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
                IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

                var user = _userService.GetUserById(userId);

                if (user == null)
                {
                    var message = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.NotFound);

                    message.ReasonPhrase = String.Format("The user id {0} specified in the request is not valid", userId);
                    return message;
                }

                //TODO: validate routing number

                PaymentAccountType accountType = PaymentAccountType.Checking;

                if (request.AccountType.ToUpper() == "CHECKING")
                    accountType = PaymentAccountType.Checking;
                else if (request.AccountType.ToUpper() == "SAVINGS")
                    accountType = PaymentAccountType.Savings;
                else
                {
                    var message = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.BadRequest);
                    message.ReasonPhrase = String.Format("Account Type specified in the request is invalid.  Valid account types are {0} or {1}", "Savings", "Checking");

                    return message;
                }


                PaymentAccount account;

                try
                {
                    account = _ctx.PaymentAccounts.Add(new Domain.PaymentAccount()
                    {
                        Id = Guid.NewGuid(),
                        AccountNumber = _securityService.Encrypt(request.AccountNumber),
                        RoutingNumber = _securityService.Encrypt(request.RoutingNumber),
                        NameOnAccount = _securityService.Encrypt(request.NameOnAccount),
                        AccountType = accountType,
                        UserId = user.UserId,
                        IsActive = true,
                        CreateDate = System.DateTime.Now
                    });

                    _ctx.SaveChanges();

                    user.SecurityPin = _securityService.Encrypt(request.SecurityPin);

                    user.SecurityQuestionID = request.SecurityQuestionID;
                    user.SecurityQuestionAnswer = _securityService.Encrypt(request.SecurityQuestionAnswer);

                    _userService.UpdateUser(user);
                }
                catch (Exception ex)
                {
                    var message = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.InternalServerError);
                    message.ReasonPhrase = String.Format("Internal Service Error. {0}", ex.Message);

                    return message;
                }

                _amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["PaymentAccountPostedTopicARN"], "New ACH Account Submitted", account.Id.ToString());

                var response = new AccountModels.SubmitAccountResponse()
                {
                    paymentAccountId = account.Id.ToString()
                };

                var responseMessage = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(response, HttpStatusCode.Created);
                //TODO: add uri for created account to response header

                return responseMessage;
            }
        }
        // POST /api/{userId}/paymentaccounts/add_account
        public HttpResponseMessage<AccountModels.SubmitAccountResponse> AddAccount(string userId, AccountModels.AddAccountRequest request)
        {
            using (var _ctx = new Context())
            {
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
                IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

                var user = _userService.GetUserById(userId);

                if (user == null)
                {
                    var message = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.NotFound);

                    message.ReasonPhrase = String.Format("The user id {0} specified in the request is not valid", userId);
                    return message;
                }

                //TODO: validate routing number

                PaymentAccountType accountType = PaymentAccountType.Checking;

                if (request.AccountType.ToUpper() == "CHECKING")
                    accountType = PaymentAccountType.Checking;
                else if (request.AccountType.ToUpper() == "SAVINGS")
                    accountType = PaymentAccountType.Savings;
                else
                {
                    var message = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.BadRequest);
                    message.ReasonPhrase = String.Format("Account Type specified in the request is invalid.  Valid account types are {0} or {1}", "Savings", "Checking");

                    return message;
                }


                PaymentAccount account;

                try
                {
                    account = _ctx.PaymentAccounts.Add(new Domain.PaymentAccount()
                    {
                        Id = Guid.NewGuid(),
                        AccountNumber = _securityService.Encrypt(request.AccountNumber),
                        RoutingNumber = _securityService.Encrypt(request.RoutingNumber),
                        NameOnAccount = _securityService.Encrypt(request.NameOnAccount),
                        AccountType = accountType,
                        UserId = user.UserId,
                        IsActive = true,
                        CreateDate = System.DateTime.Now
                    });

                    _ctx.SaveChanges();

                }
                catch (Exception ex)
                {
                    var message = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.InternalServerError);
                    message.ReasonPhrase = String.Format("Internal Service Error. {0}", ex.Message);

                    return message;
                }

                _amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["PaymentAccountPostedTopicARN"], "New ACH Account Submitted", account.Id.ToString());

                var response = new AccountModels.SubmitAccountResponse()
                {
                    paymentAccountId = account.Id.ToString()
                };

                var responseMessage = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(response, HttpStatusCode.Created);
                //TODO: add uri for created account to response header

                return responseMessage;
            }
        }

        //POST /api/{userId}/paymentaccounts/{id}/verify_account
        public HttpResponseMessage VerifyAccount(string userId, string id, AccountVerificationRequest request)
        {
            using (var _ctx = new Context())
            {
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
                IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountVerificationService verificationService =
                    new DomainServices.PaymentAccountVerificationService(_ctx, _logger);

                double depositAmount1 = 0;
                double depositAmount2 = 0;

                Double.TryParse(request.depositAmount1.ToString(), out depositAmount1);
                Double.TryParse(request.depositAmount2.ToString(), out depositAmount2);

                if (depositAmount1 == 0 || depositAmount2 == 0)
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    responseMessage.ReasonPhrase = "Invalid deposit amount specified";

                    return responseMessage;
                }

                var result = verificationService.VerifyAccount(userId, id, depositAmount1, depositAmount2);

                if (!result)
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    responseMessage.ReasonPhrase = "Not verified";

                    return responseMessage;
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        // PUT /api/{userId}/paymentaccounts/{id}
        public HttpResponseMessage Put(string userId, string id, AccountModels.UpdateAccountRequest request)
        {
            using (var _ctx = new Context())
            {
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
                IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

                var user = _userService.GetUserById(userId);

                if (user == null)
                {
                    var message = new HttpResponseMessage(HttpStatusCode.NotFound);

                    message.ReasonPhrase = String.Format("The user id {0} specified in the request is not valid", userId);
                    return message;
                }

                Guid accountId;

                Guid.TryParse(id, out accountId);

                if (accountId == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }

                var userAccount = user.PaymentAccounts.FirstOrDefault(p => p.Id == accountId);

                if (userAccount == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }

                userAccount.NameOnAccount = request.NameOnAccount;
                userAccount.RoutingNumber = request.RoutingNumber;
                userAccount.LastUpdatedDate = System.DateTime.Now;

                _ctx.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        // DELETE /api/{userId}/paymentaccounts/{id}
        //public HttpResponseMessage Delete(string userId, string id)
        //{
        //    var user = GetUser(userId);

        //    if (user == null)
        //    {
        //        var message = new HttpResponseMessage(HttpStatusCode.NotFound);

        //        message.ReasonPhrase = String.Format("The user id {0} specified in the request is not valid", userId);
        //        return message;
        //    }

        //    Guid accountId;

        //    Guid.TryParse(id, out accountId);

        //    if (accountId == null)
        //    {
        //        return new HttpResponseMessage(HttpStatusCode.BadRequest);
        //    }

        //    var userAccount = user.PaymentAccounts.FirstOrDefault(p => p.Id == accountId);

        //    if (userAccount == null)
        //    {
        //        return new HttpResponseMessage(HttpStatusCode.BadRequest);
        //    }

        //    //userAccount.Deleted = true;
        //    userAccount.LastUpdatedDate = System.DateTime.Now;

        //    _ctx.SaveChanges();
        //}
        private string GetLastFour(string p)
        {
            if (p.Length <= 4)
                return p;
            else
                return p.Substring(0, 4);

        }
    }
}
