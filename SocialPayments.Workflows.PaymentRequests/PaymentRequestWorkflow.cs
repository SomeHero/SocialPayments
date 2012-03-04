using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Services;
using SocialPayments.DataLayer;
using NLog;
using SocialPayments.Domain;
using System.Text.RegularExpressions;

namespace SocialPayments.Workflows.PaymentRequests
{
    public class PaymentRequestWorkflow
    {
        private readonly Context _ctx = new Context();
        EmailService emailService = new EmailService();
        SMSService smsService = new SMSService();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Process User Initiated payment request (a request to another user to send money)
        /// </summary>
        /// <param name="paymentRequestId">the unique id of the payment request</param>
        public void ProcessPayment(string paymentRequestId)
        {
            var paymentRequest = _ctx.PaymentRequests
                .Include("Application")
                .Include("Requestor")
                .FirstOrDefault(r => r.PaymentRequestId == new Guid(paymentRequestId));

            if (paymentRequest == null)
                throw new Exception("Payment Request id is invalid");

            if (paymentRequest.Requestor == null)
                throw new Exception("Requestor is invalid");

            logger.Log(LogLevel.Info, String.Format("Processing Payment Request {0}", paymentRequest.PaymentRequestId.ToString()));

            switch (paymentRequest.PaymentRequestStatus)
            {
                case PaymentRequestStatus.Submitted:
                    string fromAddress = System.Configuration.ConfigurationManager.AppSettings["FromAddress"];

                    string phoneNumberUnformatted = Regex.Replace(paymentRequest.RecipientUri, @"\D", string.Empty);

                    logger.Log(LogLevel.Info, string.Format("Phone Number UnFormatted {0}", phoneNumberUnformatted));

                    if (phoneNumberUnformatted.Length != 10)
                        throw new Exception("To Mobile Number is not valid");

                    string areaCode = phoneNumberUnformatted.Substring(0, 3);
                    string major = phoneNumberUnformatted.Substring(3, 3);
                    string minor = phoneNumberUnformatted.Substring(6);

                    string phoneNumberFormatted = string.Format("{0}-{1}-{2}", areaCode, major, minor);

                    paymentRequest.RecipientUri = phoneNumberFormatted;

                    //Validate Payment Request
                    
                    //Attempt to find the recipient of the request
                    var recipient = _ctx.Users.FirstOrDefault(u => u.MobileNumber == paymentRequest.RecipientUri);

                    //Send out SMS Message to recipient
                    smsService.SendSMS(new SocialPayments.Services.DataContracts.SMS.SMSRequest()
                    {
                        ApiKey = paymentRequest.ApiKey,
                        Message = String.Format("You received a PdThx request for {0:C} from {1}.", paymentRequest.Amount, paymentRequest.Requestor.MobileNumber),
                        MobileNumber = paymentRequest.RecipientUri
                    });
                    logger.Log(LogLevel.Info, String.Format("Send SMS to Payer"));

                    //Send out SMS Message to requestor
                    smsService.SendSMS(new SocialPayments.Services.DataContracts.SMS.SMSRequest()
                    {
                        ApiKey = paymentRequest.Application.ApiKey,
                        Message = String.Format("Your PdThx request for {0:C} to {1} was sent. Thx - PdThx.me", paymentRequest.Amount, paymentRequest.RecipientUri),
                        MobileNumber = paymentRequest.Requestor.MobileNumber
                    });

                    //Send out confirmation email to requester
                    if (paymentRequest.Requestor.EmailAddress.Length > 0)
                    {
                        emailService.SendEmail(new SocialPayments.Services.DataContracts.Email.EmailRequest()
                        {
                            ApiKey = paymentRequest.Application.ApiKey,
                            Subject = "Confirmation of your payment to " + paymentRequest.RecipientUri + ".",
                            Body = String.Format("Your PdThx request in the amount of {0:C} was delivered to {1}.", paymentRequest.Amount, paymentRequest.RecipientUri),
                            FromAddress = fromAddress,
                            ToAddress = paymentRequest.Requestor.EmailAddress
                        });
                    }


                    //if we found a recipient and they have an email address; send the sender an email
                    if (recipient != null)
                    {
                        if (recipient.EmailAddress.Length > 0)
                        {
                            emailService.SendEmail(new SocialPayments.Services.DataContracts.Email.EmailRequest()
                            {
                                ApiKey = paymentRequest.Application.ApiKey,
                                Subject = "You received a payment request from " + paymentRequest.Requestor.MobileNumber + ".",
                                Body = String.Format("You received a PdThx request for {0:C} from {1}. {2}", paymentRequest.Amount, paymentRequest.Requestor.MobileNumber, paymentRequest.Comments),
                                FromAddress = fromAddress,
                                ToAddress = recipient.EmailAddress
                            });
                        }
                        else
                        {
                            logger.Log(LogLevel.Info, "Invalid email address for recipient {0}", recipient.EmailAddress);
                        }
                    };

                    //Update Payment Status
                    paymentRequest.PaymentRequestStatus= Domain.PaymentRequestStatus.Complete;
                    
                    logger.Log(LogLevel.Info, String.Format("Updating Payment Request"));

                    paymentRequest.UpdateDate = System.DateTime.Now;

                    _ctx.SaveChanges();

                    break;
            }

        }
    }
}
