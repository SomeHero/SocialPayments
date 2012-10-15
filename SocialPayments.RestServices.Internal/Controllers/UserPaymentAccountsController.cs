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
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using SocialPayments.Domain.ExtensionMethods;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserPaymentAccountsController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/{userId}/paymentaccounts
        [HttpGet]
        public HttpResponseMessage Get(string userId)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();
            var securityServices = new DomainServices.SecurityService();
            List<Domain.PaymentAccount> paymentAccounts = null;

            try
            {
                paymentAccounts = userPaymentAccountServices.GetPaymentAccounts(userId);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Payment Accounts for User {0}.  Exception {1}.", userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Payment Accounts for User {0}.  Exception {1}.", userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Payment Accounts for User {0}.  Exception {1}. Stack Trace {2}", userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<List<AccountModels.AccountResponse>>(HttpStatusCode.OK, paymentAccounts.Select(a => new AccountModels.AccountResponse()
                {
                    AccountNumber = securityServices.GetLastFour(securityServices.Decrypt(a.AccountNumber)),
                    AccountType = a.AccountType.ToString(),
                    BankName = a.BankName,
                    BankIconUrl = a.BankIconURL,
                    NameOnAccount = securityServices.Decrypt(a.NameOnAccount),
                    Nickname = a.Nickname == null ? String.Format("{0} {1}", a.AccountType, securityServices.GetLastFour(securityServices.Decrypt(a.AccountNumber))) : a.Nickname,
                    Id = a.Id.ToString(),
                    RoutingNumber = securityServices.Decrypt(a.RoutingNumber),
                    UserId = a.UserId.ToString(),
                    Status = a.AccountStatus.GetDescription()
                }).ToList<AccountModels.AccountResponse>());

        }

        // GET /api/{userId}/paymentaccounts/{id}
        [HttpGet]
        public HttpResponseMessage Get(string userId, string id)
        {
            DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
            DomainServices.UserPaymentAccountServices userPaymentAccountServices
                    = new DomainServices.UserPaymentAccountServices();
            Domain.PaymentAccount paymentAccount = null;

            try
            {
                paymentAccount = userPaymentAccountServices.GetPaymentAccount(userId, id);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Payment Account {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Payment Account {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Payment Account {0} for User {1}.  Exception {2}. Stack Trace {3}", id, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse<AccountModels.AccountResponse>(HttpStatusCode.OK, new AccountModels.AccountResponse()
            {
                AccountNumber = _securityService.GetLastFour(_securityService.Decrypt(paymentAccount.AccountNumber)),
                AccountType = paymentAccount.AccountType.ToString(),
                BankName = paymentAccount.BankName,
                BankIconUrl = paymentAccount.BankIconURL,
                NameOnAccount = _securityService.Decrypt(paymentAccount.NameOnAccount),
                Nickname = paymentAccount.Nickname == null ? String.Format("{0} {1}", paymentAccount.AccountType, _securityService.GetLastFour(_securityService.Decrypt(paymentAccount.AccountNumber))) : paymentAccount.Nickname,
                Id = paymentAccount.Id.ToString(),
                RoutingNumber = _securityService.Decrypt(paymentAccount.RoutingNumber),
                UserId = paymentAccount.UserId.ToString(),
                Status = paymentAccount.AccountStatus.GetDescription()
            });
        }

        // POST /api/{userid}/paymentaccounts/set_preferred_send_account
        [HttpPost]
        public HttpResponseMessage SetPreferredSendAccount(string userId, AccountModels.ChangePreferredAccountRequest request)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();

            try
            {
                userPaymentAccountServices.SetPreferredSendAccount(userId, request.PaymentAccountId, request.SecurityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Setting Preferred Send Account {0} for User {1}.  Exception {2}.", request.PaymentAccountId, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Setting Preferred Send Account {0} for User {1}.  Exception {2}.", request.PaymentAccountId, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Setting Preferred Send Account {0} for User {1}.  Exception {2}. Stack Trace {3}", request.PaymentAccountId, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST /api/{userId}/paymentaccounts/set_preferred_receive_account
        [HttpPost]
        public HttpResponseMessage SetPreferredReceiveAccount(string userId, AccountModels.ChangePreferredAccountRequest request)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();

            try
            {
                userPaymentAccountServices.SetPreferredReceiveAccount(userId, request.PaymentAccountId, request.SecurityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Setting Preferred Receive Account {0} for User {1}.  Exception {2}.", request.PaymentAccountId, userId, ex.Message));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Setting Preferred Receive Account {0} for User {1}.  Exception {2}.", request.PaymentAccountId, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Setting Preferred Receive Account {0} for User {1}.  Exception {2}. Stack Trace {3}", request.PaymentAccountId, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST /api/{userId}/paymentaccounts
        [HttpPost]
        public HttpResponseMessage Post(string userId, AccountModels.SubmitAccountRequest request)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();
            Domain.PaymentAccount paymentAccount = null;

            try
            {
                paymentAccount = userPaymentAccountServices.AddPaymentAccount(userId, request.Nickname, "", request.NameOnAccount, request.RoutingNumber, request.AccountNumber, request.AccountType,
                    request.SecurityPin, request.SecurityQuestionID, request.SecurityQuestionAnswer);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Account w/Security Settings for User {0}.  Exception {1}.", userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Account w/Security Settings for User {0}.  Exception {1}.", userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Account w/Security Settings for User {0}.  Exception {2}. Stack Trace {3}", userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }
        // POST /api/{userId}/paymentaccounts/add_account
        [HttpPost]
        public HttpResponseMessage AddAccount(string userId, AccountModels.AddAccountRequest request)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();
            HttpResponseMessage response = null;
            Domain.PaymentAccount paymentAccount = null;

            try
            {
                paymentAccount = userPaymentAccountServices.AddPaymentAccount(userId, request.Nickname, "", request.NameOnAccount, request.RoutingNumber, request.AccountNumber, request.AccountType,
                    request.SecurityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Adding Payment Account for User {0}.  Exception {1}.", userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Adding Payment Account for User {0}.  Exception {1}.", userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment Account for User {0}.  Exception {1}. Stack Trace {2}", userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.Created);
        }
        /*
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
        }*/
        //POST /api/{userId}/paymentaccounts/{id}/verify_account
        [HttpPost]
        public HttpResponseMessage VerifyAccount(string userId, string id, AccountVerificationRequest request)
        {
            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();

            try
            {
                userPaymentAccountServices.VerifyAccount(userId, id, request.depositAmount1, request.depositAmount2);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Verifying Payment Account {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Verifying Payment Account {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Verifying Payment Account {0} for User {1}.  Exception {2}. Stack Trace {3}", id, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // PUT /api/{userId}/paymentaccounts/{id}
        [HttpPut]
        public HttpResponseMessage Put(string userId, string id, AccountModels.UpdateAccountRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Updating Payment Account {0} for User {1}", id, userId));

            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();

            try
            {
                userPaymentAccountServices.UpdatePaymentAccount(userId, id, request.Nickname, request.NameOnAccount, request.RoutingNumber, request.AccountType,
                    request.SecurityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Updating Payment Account {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Updating Payment Account {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Updating Payment Account {0} for User {1}.  Exception {2}. Stack Trace {3}", id, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);

        }

        // DELETE /api/{userId}/paymentaccounts/{id}
        [HttpDelete]
        public HttpResponseMessage Delete(string userId, string id, AccountModels.DeletePaymentAccountRequest request)
        {
            _logger.Log(LogLevel.Info, String.Format("Deleting Payment Account {0} for User {1}..", id, userId));

            var userPaymentAccountServices = new DomainServices.UserPaymentAccountServices();

            try
            {
                userPaymentAccountServices.DeletePaymentAccount(userId, id, request.SecurityPin);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Deleting Payment Account {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Deleting Payment Account {0} for User {1}.  Exception {2}.", id, userId, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting Payment Account {0} for User {1}.  Exception {2}. Stack Trace {3}", id, userId, ex.Message, ex.StackTrace));

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
