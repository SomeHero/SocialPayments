using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using System.Net;
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
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();
            var securityServices = new DomainServices.SecurityService();
            HttpResponseMessage<List<AccountModels.AccountResponse>> response = null;
            List<Domain.PaymentAccount> paymentAccounts = null;

            try
            {
                paymentAccounts = userPaymentAccountServices.GetPaymentAccounts(userId);
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage<List<AccountModels.AccountResponse>>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<List<AccountModels.AccountResponse>>(paymentAccounts.Select(a => new AccountModels.AccountResponse()
                {
                    AccountNumber = securityServices.GetLastFour(securityServices.Decrypt(a.AccountNumber)),
                    AccountType = a.AccountType.ToString(),
                    BankName = a.BankName,
                    BankIconUrl = a.BankIconURL,
                    NameOnAccount = securityServices.Decrypt(a.NameOnAccount),
                    Nickname = a.Nickname,
                    Id = a.Id.ToString(),
                    RoutingNumber = securityServices.Decrypt(a.RoutingNumber),
                    UserId = a.UserId.ToString(),
                    Status = a.AccountStatus.GetDescription()
                }).ToList<AccountModels.AccountResponse>(), HttpStatusCode.OK);

            return response;
        }

        // GET /api/{userId}/paymentaccounts/{id}
        public HttpResponseMessage<AccountModels.AccountResponse> Get(string userId, string id)
        {
            DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
            DomainServices.UserPaymentAccountServices userPaymentAccountServices
                    = new DomainServices.UserPaymentAccountServices();
            Domain.PaymentAccount paymentAccount = null;
            HttpResponseMessage<AccountModels.AccountResponse> response = null;

            try
            {
                paymentAccount = userPaymentAccountServices.GetPaymentAccount(userId, id);
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage<AccountModels.AccountResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            var accountResponse = new AccountModels.AccountResponse()
            {
                AccountNumber = _securityService.GetLastFour(_securityService.Decrypt(paymentAccount.AccountNumber)),
                AccountType = paymentAccount.AccountType.ToString(),
                BankName = paymentAccount.BankName,
                BankIconUrl = paymentAccount.BankIconURL,
                NameOnAccount = _securityService.Decrypt(paymentAccount.NameOnAccount),
                Nickname = paymentAccount.Nickname,
                Id = paymentAccount.Id.ToString(),
                RoutingNumber = _securityService.Decrypt(paymentAccount.RoutingNumber),
                UserId = paymentAccount.UserId.ToString(),
                Status = paymentAccount.AccountStatus.GetDescription()
            };

            return new HttpResponseMessage<AccountModels.AccountResponse>(accountResponse, HttpStatusCode.OK);

        }

        // POST /api/{userid}/paymentaccounts/set_preferred_send_account
        public HttpResponseMessage SetPreferredSendAccount(string userId, AccountModels.ChangePreferredAccountRequest request)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();
            HttpResponseMessage response = null;

            try
            {
                userPaymentAccountServices.SetPreferredSendAccount(userId, request.PaymentAccountId);
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }

        // POST /api/{userId}/paymentaccounts/set_preferred_receive_account
        public HttpResponseMessage SetPreferredReceiveAccount(string userId, AccountModels.ChangePreferredAccountRequest request)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();
            HttpResponseMessage response = null;

            try
            {
                userPaymentAccountServices.SetPreferredReceiveAccount(userId, request.PaymentAccountId);
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }

        // POST /api/{userId}/paymentaccounts
        public HttpResponseMessage<AccountModels.SubmitAccountResponse> Post(string userId, AccountModels.SubmitAccountRequest request)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();
            HttpResponseMessage<AccountModels.SubmitAccountResponse> response = null;
            Domain.PaymentAccount paymentAccount = null;

            try
            {
                paymentAccount = userPaymentAccountServices.AddPaymentAccount(userId, request.Nickname, "", request.NameOnAccount, request.RoutingNumber, request.AccountNumber, request.AccountType,
                    request.SecurityPin, request.SecurityQuestionID, request.SecurityQuestionAnswer);
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.Created);
            //response.Content
            return response;
        }
        // POST /api/{userId}/paymentaccounts/add_account
        public HttpResponseMessage<AccountModels.SubmitAccountResponse> AddAccount(string userId, AccountModels.AddAccountRequest request)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();
            HttpResponseMessage<AccountModels.SubmitAccountResponse> response = null;
            Domain.PaymentAccount paymentAccount = null;

            try
            {
                paymentAccount = userPaymentAccountServices.AddPaymentAccount(userId, request.Nickname, "", request.NameOnAccount, request.RoutingNumber, request.AccountNumber, request.AccountType,
                    request.SecurityPin);
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(HttpStatusCode.Created);
            //response.Content
            return response;
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
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();

            HttpResponseMessage response = null;
            try
            {
                userPaymentAccountServices.VerifyAccount(userId, id, request.depositAmount1, request.depositAmount2);
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }

        // PUT /api/{userId}/paymentaccounts/{id}
        public HttpResponseMessage Put(string userId, string id, AccountModels.UpdateAccountRequest request)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();

            HttpResponseMessage response = null;
            try
            {
                userPaymentAccountServices.UpdatePaymentAccount(userId, id, request.Nickname, request.NameOnAccount, request.RoutingNumber, request.AccountType);
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;

        }

        // DELETE /api/{userId}/paymentaccounts/{id}
        public HttpResponseMessage Delete(string userId, string id)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();

            HttpResponseMessage response = null;
            try
            {
                userPaymentAccountServices.DeletePaymentAccount(userId, id);
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }

            response = new HttpResponseMessage(HttpStatusCode.OK);

            return response;
        }
    }
}
