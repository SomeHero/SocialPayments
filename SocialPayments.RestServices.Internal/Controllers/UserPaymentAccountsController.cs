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
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using SocialPayments.Domain.ExtensionMethods;

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
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

                var user = _userService.GetUserById(userId);

                if (user == null)
                {
                    var message = new HttpResponseMessage<List<AccountModels.AccountResponse>>(HttpStatusCode.NotFound);

                    message.ReasonPhrase = String.Format("The user id {0} specified in the request is not valid", userId);
                    return message;
                }

                var accounts = _ctx.PaymentAccounts
                    .Where(a => a.UserId == user.UserId  && a.IsActive).ToList();

                var accountResponse = accounts.Select(a => new AccountModels.AccountResponse()
                {
                    AccountNumber = _securityService.GetLastFour(_securityService.Decrypt(a.AccountNumber)),
                    AccountType = a.AccountType.ToString(),
                    BankName = a.BankName,
                    BankIconUrl = a.BankIconURL,
                    NameOnAccount = _securityService.Decrypt(a.NameOnAccount),
                    Nickname = a.Nickname,
                    Id = a.Id.ToString(),
                    RoutingNumber = _securityService.Decrypt(a.RoutingNumber),
                    UserId = a.UserId.ToString(),
                    Status = a.AccountStatus.GetDescription()
                }).ToList<AccountModels.AccountResponse>();

                

                return new HttpResponseMessage<List<AccountModels.AccountResponse>>(accountResponse, HttpStatusCode.OK);
            }
         }

        // GET /api/{userId}/paymentaccounts/{id}
        public HttpResponseMessage<AccountModels.AccountResponse> Get(string userId, string id)
        {
            using (var _ctx = new Context())
            {
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);

                var user = _userService.GetUserById(userId);

                if (user == null)
                {
                    var message = new HttpResponseMessage<AccountModels.AccountResponse>(HttpStatusCode.NotFound);

                    message.ReasonPhrase = String.Format("The user id {0} specified in the request is not valid", userId);
                    return message;
                }

                Guid paymentAccountId;

                Guid.TryParse(id, out paymentAccountId);

                var account = _ctx.PaymentAccounts
                    .FirstOrDefault(a => a.Id == paymentAccountId && a.UserId == user.UserId && a.IsActive);

                var accountResponse = new AccountModels.AccountResponse()
                {
                    AccountNumber = _securityService.GetLastFour(_securityService.Decrypt(account.AccountNumber)),
                    AccountType = account.AccountType.ToString(),
                    BankName = account.BankName,
                    BankIconUrl = account.BankIconURL,
                    NameOnAccount = _securityService.Decrypt(account.NameOnAccount),
                    Nickname = account.Nickname,
                    Id = account.Id.ToString(),
                    RoutingNumber = _securityService.Decrypt(account.RoutingNumber),
                    UserId = account.UserId.ToString(),
                    Status = account.AccountStatus.GetDescription()
                };


                return new HttpResponseMessage<AccountModels.AccountResponse>(accountResponse, HttpStatusCode.OK);
            }

        }

        // POST /api/{userid}/paymentaccounts/set_preferred_send_account
        public HttpResponseMessage SetPreferredSendAccount(string userId, AccountModels.ChangePreferredAccountRequest request)
        {
            using (var _ctx = new Context())
            {
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService paymentAccountService = new DomainServices.PaymentAccountService(_ctx);

                var user = _userService.GetUserById(userId);

                if (user == null)
                {
                    var message = new HttpResponseMessage(HttpStatusCode.NotFound);

                    message.ReasonPhrase = String.Format("The user id {0} specified in the request is not valid", userId);
                    return message;
                }

                var paymentAccount = paymentAccountService.GetPaymentAccount(request.PaymentAccountId);

                if (paymentAccount == null)
                {
                    var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    message.ReasonPhrase = String.Format("Invalid account id {0} specified in the request", request.PaymentAccountId);

                    return message;
                }
                user.PreferredSendAccount = paymentAccount;

                _ctx.SaveChanges();

            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // POST /api/{userId}/paymentaccounts/set_preferred_receive_account
        public HttpResponseMessage SetPreferredReceiveAccount(string userId, AccountModels.ChangePreferredAccountRequest request)
        {
            using(var _ctx = new Context())
            {
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService paymentAccountService = new DomainServices.PaymentAccountService(_ctx);

                var user = _userService.GetUserById(userId);

                 if (user == null)
                {
                    var message = new HttpResponseMessage(HttpStatusCode.NotFound);

                    message.ReasonPhrase = String.Format("The user id {0} specified in the request is not valid", userId);
                    return message;
                }

                var paymentAccount = paymentAccountService.GetPaymentAccount(request.PaymentAccountId);

                if(paymentAccount == null)
                {
                    var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    message.ReasonPhrase = String.Format("Invalid account id {0} specified in the request", request.PaymentAccountId);

                    return message;
                }
                user.PreferredReceiveAccount = paymentAccount;

                _ctx.SaveChanges();

            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // POST /api/{userId}/paymentaccounts
        public HttpResponseMessage<AccountModels.SubmitAccountResponse> Post(string userId, AccountModels.SubmitAccountRequest request)
        {
            using (var ctx = new Context())
            {
                var securityService = new DomainServices.SecurityService();
                var userService = new DomainServices.UserService(ctx);
                var paymentAccountService = new DomainServices.PaymentAccountService(ctx);

                var user = userService.GetUserById(userId);

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
                    account = paymentAccountService.AddPaymentAccount(user, request.Nickname, request.NameOnAccount, request.RoutingNumber, 
                        request.AccountNumber, request.AccountType, request.SecurityPin, request.SecurityQuestionID,
                        request.SecurityQuestionAnswer);
                }
                catch (Exception ex)
                {
                    var message = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.InternalServerError);
                    message.ReasonPhrase = String.Format("Internal Service Error. {0}", ex.Message);

                    return message;
                }

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
            using (var ctx = new Context())
            {
                var securityService = new DomainServices.SecurityService();
                var userService = new DomainServices.UserService(ctx);
                var emailService = new DomainServices.EmailService(ctx);
                var paymentAccountService = new DomainServices.PaymentAccountService(ctx);

                var user = userService.GetUserById(userId);

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
                    account = paymentAccountService.AddPaymentAccount(user, request.Nickname, request.NameOnAccount, request.RoutingNumber, request.AccountNumber,
                        request.AccountType);
                }
                catch (Exception ex)
                {
                    var message = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.InternalServerError);
                    message.ReasonPhrase = String.Format("Internal Service Error. {0}", ex.Message);

                    return message;
                }
        
                var response = new AccountModels.SubmitAccountResponse()
                {
                    paymentAccountId = account.Id.ToString()
                };

                var responseMessage = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(response, HttpStatusCode.Created);
                //TODO: add uri for created account to response header

                return responseMessage;
            }
        }

        // POST /api/users/{id}/PaymentAccounts/upload_check_image
        public Task<HttpResponseMessage> UploadCheckImage([FromUri] string id)
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(
                    Request.CreateResponse(HttpStatusCode.UnsupportedMediaType));
            }

            var provider = new RenamingMultipartFormDataStreamProvider(String.Format(@"{0}\{1}", @"c:\checkImages", id));

            // Read the form data and return an async task.
            var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(readTask =>
                {
                    if (readTask.IsFaulted || readTask.IsCanceled)
                    {
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    }

                    // This illustrates how to get the file names.
                    foreach (var file in provider.BodyPartFileNames)
                    {
                        _logger.Log(LogLevel.Info, "Client file name: " + file.Key);
                        _logger.Log(LogLevel.Info, "Server file path: " + file.Value);
                    }
                    return new HttpResponseMessage(HttpStatusCode.Created);
                });

            return task;
        }
        //POST /api/{userId}/paymentaccounts/{id}/verify_account
        public HttpResponseMessage VerifyAccount(string userId, string id, AccountVerificationRequest request)
        {
            using (var _ctx = new Context())
            {
                DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
                IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
                DomainServices.UserService _userService = new DomainServices.UserService(_ctx);
                DomainServices.PaymentAccountService verificationService =
                    new DomainServices.PaymentAccountService(_ctx);

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

                var result = false; ///// verificationService.VerifyAccount(userId, id, depositAmount1, depositAmount2);

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

                PaymentAccountType accountType = PaymentAccountType.Checking;

                if (request.AccountType.ToUpper() == "CHECKING")
                    accountType = PaymentAccountType.Checking;
                else if (request.AccountType.ToUpper() == "SAVINGS")
                    accountType = PaymentAccountType.Savings;

                userAccount.Nickname = request.Nickname;
                userAccount.NameOnAccount = _securityService.Encrypt(request.NameOnAccount);
                userAccount.RoutingNumber = _securityService.Encrypt(request.RoutingNumber);
                userAccount.AccountType = accountType;
                userAccount.LastUpdatedDate = System.DateTime.Now;

                _ctx.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        // DELETE /api/{userId}/paymentaccounts/{id}
        public HttpResponseMessage Delete(string userId, string id)
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

                userAccount.IsActive = false;
                userAccount.LastUpdatedDate = System.DateTime.Now;

                _ctx.SaveChanges();

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }
    }
}
