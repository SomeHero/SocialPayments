using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Net;
using System.IO;
using System.Configuration;
using System.Web.Script.Serialization;
using SocialPayments.Domain;
using SocialPayments.DataLayer;

namespace SocialPayments.Services.PaymentAccounts
{
    public class SubmittedPaymentAccountProcessor
    {
        private Context _ctx;
        private Logger _logger;
        private string _emailSubject = "You've added a new bank account";
        private string _templateName = "ACH Account Verification";

        public SubmittedPaymentAccountProcessor()
        {
            _ctx = new Context();
            _logger = LogManager.GetCurrentClassLogger();
        }
        public bool Process(PaymentAccount paymentAccount)
        {
            var paymentAccountService = new DomainServices.PaymentAccountService(_ctx);
            var emailService = new DomainServices.EmailService(_ctx);

            //get random numbers between 10 and 49 that are not equal
            Random rand1 = new Random(10);
            Random rand2 = new Random(49);
            var depositAmount1 = (double)rand1.Next(10, 49) / 100;
            var depositAmount2 = (double)rand2.Next(10, 49) / 100;

            while (depositAmount1.Equals(depositAmount2))
            {
                depositAmount2 = (double)rand2.Next(10, 49) / 100;
            }

            var withdrawalAmount = depositAmount1 + depositAmount2;

            var sentDate = System.DateTime.Now;
            var estimatedSettlementDate = System.DateTime.Now.AddDays(5);

            var verification = paymentAccountService.AddVerification(paymentAccount,
                depositAmount1, depositAmount2, withdrawalAmount, sentDate, estimatedSettlementDate);

            emailService.SendEmail(paymentAccount.User.EmailAddress, _emailSubject, _templateName,
                new List<KeyValuePair<string, string>>());

            return true;
        }
    }
}
