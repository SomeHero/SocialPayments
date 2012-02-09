using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using System.Net.Mail;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using NLog;

namespace SocialPayments.DomainServices
{
    public class PaymentService
    {
        private readonly Context _ctx = new Context();
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public Domain.Payment AddPayment(Domain.Application application, string comments, Domain.PaymentAccount fromAccount, string fromMobileNumber,
            double paymentAmount, PaymentChannelType paymentChannelType, PaymentStatus paymentStatus, StandardEntryClass standardEntryClass,
            string toMobileNumber)
        {
            logger.Log(LogLevel.Info, String.Format("Adding new payment"));


            var payment = _ctx.Payments.Add(new Domain.Payment()
            {
                Id = Guid.NewGuid(),
                ApiKey = application.ApiKey,
                Comments = comments,
                CreateDate = System.DateTime.Now,
                FromAccountId = fromAccount.Id,
                FromMobileNumber = fromMobileNumber,
                PaymentAmount = paymentAmount,
                PaymentChannelType = paymentChannelType,
                PaymentDate = System.DateTime.Now,
                PaymentStatus = paymentStatus,
                StandardEntryClass = standardEntryClass,
                ToMobileNumber = toMobileNumber
            });

            try
            {
                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, String.Format("Unable to add payment. Exception {0}", ex.ToString()));
                throw ex;
            }

            return payment;
        }
        public List<Payment> GetPayments(Expression<Func<Payment, bool>> expression)
        {
            var payments = _ctx.Payments
                .Include("Application")
                .Include("FromAccount")
                .Include("FromAccount.User")
                .Include("ToAccount")
                .Where(expression);

            return payments.ToList<Payment>();
        }
        public List<Payment> GetPayments(Expression<Func<Payment, Payment>> selector)
        {
            var payments = _ctx.Payments
                .Include("Application")
                .Include("FromAccount")
                .Include("FromAccount.User")
                .Include("ToAccount")
                .Select(selector);

            return payments.ToList<Payment>();
        }

        public Payment GetPayment(Guid id)
        {

            var payment = _ctx.Payments
                .Include("Application")
                .Include("FromAccount")
                .Include("FromAccount.User")
                .Include("ToAccount")
                .FirstOrDefault(p => p.Id == id);

            return payment;
        }
        public Payment MakePayment(Guid userId, string fromMobileNumber, string toMobileNumber, double amount, string comment, int fromAccount)
        {

             var application = _ctx.Applications.FirstOrDefault();
             var user = _ctx.Users
                 .Include("PaymentAccounts")
                 .FirstOrDefault(u => u.UserId == userId);
             var paymentAccount = user.PaymentAccounts[0];

             var payee = _ctx.Users
                 .Include("PaymentAccounts")
                 .FirstOrDefault(u => u.MobileNumber.Equals(toMobileNumber));
             
             PaymentAccount toAccount = null;
             if(payee != null)
                 toAccount = payee.PaymentAccounts[0];

             var payment = _ctx.Payments.Add(new Payment()
             {
                 Application = application,
                 FromAccount = paymentAccount,
                 FromMobileNumber = fromMobileNumber,
                 PaymentAmount = amount,
                 PaymentDate = System.DateTime.Now,
                 PaymentStatus = PaymentStatus.Submitted,
                 ToMobileNumber = toMobileNumber,
                 ToAccount = toAccount,
                 Comments = comment,
                 CreateDate = System.DateTime.Now,
             });

             _ctx.SaveChanges();

             AmazonSimpleNotificationServiceClient client = new AmazonSimpleNotificationServiceClient();

             client.Publish(new PublishRequest()
             {
                 Message = payment.Id.ToString(),
                 TopicArn = "arn:aws:sns:us-east-1:102476399870:SocialPaymentNotifications",
                 Subject = "New Payment Receivied"
             });

            return payment;
        }
        public void UpdatePayment(Payment payment)
        {
            _ctx.SaveChanges();
        }
        public void DeletePayment(Guid id)
        {
            var payment = _ctx.Payments.FirstOrDefault(p => p.Id == id);

            if (payment != null)
            {
                _ctx.Payments.Remove(payment);
                _ctx.SaveChanges();
            }
        }
    }
}
