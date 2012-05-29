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

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class UserPaymentAccountsController : ApiController
    {
        private Context _ctx = new Context();
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private DomainServices.SecurityService _securityService = new DomainServices.SecurityService();
        private static IAmazonNotificationService _amazonNotificationService = new DomainServices.AmazonNotificationService();
       
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

            _amazonNotificationService.PushSNSNotification(ConfigurationManager.AppSettings["PaymentAccountPostedTopicARN"], "New ACH Account Submitted", account.Id.ToString());
          
            var response = new AccountModels.SubmitAccountResponse()
            {
                paymentAccountId = account.Id.ToString()
            };

            var responseMessage = new HttpResponseMessage<AccountModels.SubmitAccountResponse>(response, HttpStatusCode.Created);
            //TODO: add uri for created account to response header

            return responseMessage;
        }

        //POST /api/{userId}/paymentaccounts/{id}/verify_account
        public HttpResponseMessage VerifyAccount(string userId, string id, AccountVerificationRequest request)
        {
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
