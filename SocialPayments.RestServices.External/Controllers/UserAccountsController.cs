using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.DataLayer;
using System.Net;
using SocialPayments.RestServices.External.Models;
using SocialPayments.DomainServices;
using SocialPayments.Domain;

namespace SocialPayments.RestServices.External.Controllers
{
    public class UserAccountsController : ApiController
    {
        private Context _ctx = new Context();
        private SecurityService _securityService = new SecurityService();

        // GET /api/useraccounts
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        // GET /api/users/1/accounts/5
        public HttpResponseMessage<AccountModels.AccountResponse> Get(string id)
        {
            var paymentAccount = GetAccount(id);

            var accountResponse = new AccountModels.AccountResponse()
            {
                AccountNumber = paymentAccount.AccountNumber,
                AccountType = paymentAccount.AccountType.ToString(),
                Id = paymentAccount.Id.ToString(),
                NameOnAccount = paymentAccount.NameOnAccount,
                RoutingNumber = paymentAccount.RoutingNumber,
                UserId = paymentAccount.UserId.ToString()
            };

            return new HttpResponseMessage<AccountModels.AccountResponse>(accountResponse, HttpStatusCode.OK);
        }

        // POST /api/users/1/accounts
        public HttpResponseMessage Post(string id, AccountModels.SubmitAccountRequest request)
        {
            var user = GetUser(id);

            if (user == null)
            {
                var message = new HttpResponseMessage(HttpStatusCode.NotFound);

                message.ReasonPhrase = String.Format("The user id {0} specified in the request is not valid", id);
                return message;
            }
            //TODO: validate routing number

            PaymentAccountType accountType = PaymentAccountType.Checking;

            if(request.AccountType.ToUpper() == "CHECKING")
                accountType = PaymentAccountType.Checking;
            else if(request.AccountType.ToUpper() == "SAVINGS")
                accountType = PaymentAccountType.Savings;
            else
            {
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
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
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = String.Format("Internal Service Error. {0}", ex.Message);

                return message;
            }

            var response = new HttpResponseMessage(HttpStatusCode.Created);
            //TODO: add uri for created account to response header

            return response;
        }

        // PUT /api/users/1/accounts/5
        public HttpResponseMessage Put(string id, string accountId, AccountModels.UpdateAccountRequest request)
        {
            var user = GetUser(id);

            if (user == null)
            {
                var message = new HttpResponseMessage(HttpStatusCode.NotFound);

                message.ReasonPhrase = String.Format("The user {0} specified in the request is not valid", id);
                return message;
            }

            var account = GetAccount(accountId);

            if (account == null)
            {
                var message = new HttpResponseMessage(HttpStatusCode.NotFound);
                message.ReasonPhrase = String.Format("The account {0} specified in the request is not valid", accountId);

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
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.ReasonPhrase = String.Format("Account Type specified in the request is invalid.  Valid account types are {0} or {1}", "Savings", "Checking");

                return message;
            }

            try
            {
                account.AccountNumber = _securityService.Encrypt(request.AccountNumber);
                account.AccountType = accountType;
                //account.IsActive = true;
                account.LastUpdatedDate = System.DateTime.Now;
                account.NameOnAccount = _securityService.Encrypt(request.NameOnAccount);
                account.RoutingNumber = _securityService.Encrypt(request.RoutingNumber);

                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = String.Format("Internal Server Error. {0}", ex.Message);

                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // DELETE /api/users/1/accounts/5
        public HttpResponseMessage Delete(string id, string accountId)
        {
            var user = GetUser(id);

            if (user == null)
            {
                var message = new HttpResponseMessage(HttpStatusCode.NotFound);

                message.ReasonPhrase = String.Format("The user {0} specified in the request is not valid", id);
                return message;
            }

            var account = GetAccount(accountId);

            if (account == null)
            {
                var message = new HttpResponseMessage(HttpStatusCode.NotFound);
                message.ReasonPhrase = String.Format("The account {0} specified in the request is not valid", accountId);

                return message;
            }

            try
            {
                account.IsActive = false;
                account.LastUpdatedDate = System.DateTime.Now;

                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                var message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                message.ReasonPhrase = String.Format("Internal Server Error", ex.Message);

                return message;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);

        }
        private Domain.User GetUser(string id)
        {
            Guid userId;

            Guid.TryParse(id, out userId);

            if (userId == null)
                return null;

            var user = _ctx.Users
                .Include("UserAttributes")
                .Include("UserAttributes.UserAttribute")
                .FirstOrDefault(u => u.UserId.Equals(userId));

            if (user == null)
                return null;

            return user;
        }
        private Domain.PaymentAccount GetAccount(string id)
        {
            Guid paymentAccountId;

            Guid.TryParse(id, out paymentAccountId);

            if (paymentAccountId == null)
                return null;

            var paymentAccount = _ctx.PaymentAccounts.FirstOrDefault(a => a.Id.Equals(paymentAccountId));

            return paymentAccount;
        }
    }
}
