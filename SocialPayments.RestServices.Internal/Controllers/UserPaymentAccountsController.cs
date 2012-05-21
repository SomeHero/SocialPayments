using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using System.Net;
using SocialPayments.DataLayer;
using SocialPayments.Domain;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserPaymentAccountsController : ApiController
    {
        private Context _ctx = new Context();
        private DomainServices.SecurityService _securityService = new DomainServices.SecurityService();

        // GET /api/{userId}/paymentaccounts
        public IEnumerable<string> Get(string userId)
        {
            return new string[] { "value1", "value2" };
        }

        // GET /api/{userId}/paymentaccounts/{id}
        public string Get(string userId, string id)
        {
            return "value";
        }

        // POST /api/{userId}/paymentaccounts
        public HttpResponseMessage<AccountModels.SubmitAccountResponse> Post(string userId, AccountModels.SubmitAccountRequest request)
        {
            var user = GetUser(userId);

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

            var response = new AccountModels.SubmitAccountResponse()
            {
                paymentAccountId = account.Id.ToString()
            };

            var responseMessage = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(response, HttpStatusCode.Created);
            //TODO: add uri for created account to response header

            return responseMessage;
        }

        // PUT /api/{userId}/paymentaccounts/{id}
        public void Put(string userId, string id, AccountModels.UpdateAccountRequest request)
        {
        }

        // DELETE /api/{userId}/paymentaccounts/{id}
        public void Delete(string userId, string id)
        {
        }
        private User GetUser(string id)
        {
            Guid userId;

            Guid.TryParse(id, out userId);

            if (userId == null)
                return null;

            var user = _ctx.Users
                .FirstOrDefault(u => u.UserId.Equals(userId));

            if (user == null)
                return null;

            return user;
        }
    }
}
