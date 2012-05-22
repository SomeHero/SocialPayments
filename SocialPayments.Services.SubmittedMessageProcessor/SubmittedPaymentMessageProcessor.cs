using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using System.Text.RegularExpressions;
using SocialPayments.Services.IMessageProcessor;
using SocialPayments.DomainServices;
using SocialPayments.Domain;
using NLog;
using SocialPayments.DataLayer.Interfaces;
using System.Net;
using System.IO;
using System.Web;

namespace SocialPayments.Services.MessageProcessors
{
    enum URIType
    {
        MobileNumber,
        EmailAddress,
        MECode,
        FacebookAccount
    }

    public class SubmittedPaymentMessageProcessor : IMessageProcessor.IMessageProcessor
    {
        private IDbContext _ctx;
        private Logger _logger;

        private FormattingServices _formattingService;
        private TransactionBatchService _transactionBatchService;
        private ValidationService _validationService;
        private UserService _userService;
        private SMSService _smsService;
        private EmailService _emailService;

        private string _recipientSMSMessage = "{1} just sent you {0:C} using PaidThx.  The payment has been submitted for processing. Go to {2}.";
        private string _senderSMSMessage = "Your payment in the amount {0:C} was delivered to {1}.  The payment has been submitted for processing. Go to {2}";

        private string _senderConfirmationEmailSubject = "Confirmation of your payment to {0}";
        private string _senderConfirmationEmailBody = "Your payment in the amount of {0:C} was delivered to {1}.  The payment has been submitted for processing. Go to {2}";
        private string _recipientConfirmationEmailSubject = "{0} just sent you {1:C} using PaidThx.";
        private string _recipientConfirmationEmailBody = "{0} sent you {1:C} using PaidThx. Go to {2} to complete your transaction.";

        private string _senderSMSMessageRecipientNotRegistered= "Your payment of {0:C} was delivered to {1}. The payment is pending unit {1} completes registration with PaidThx. Go to {2}";
        private string _recipientSMSMessageRecipientNotRegistered = "{0} just sent you {1:C} using PaidThx.  Go to {2} to complete the transaction.";
        private string _senderConfirmationEmailSubjectRecipientNotRegistered = "Confirmation of your payment to {0}.";
        private string _senderConfirmationEmailBodyRecipientNotRegistered = "Your payment in the amount of {0:C} was delivered to {1}.  {1} does not have an account with PaidThx.  We have sent their mobile number information about your payment and instructions to register.";
        private string _mobileWebSiteUrl = @"http://beta.paidthx.com/mobile/";
        
        public SubmittedPaymentMessageProcessor() {
            _ctx  = new DataLayer.Context();
            _logger = LogManager.GetCurrentClassLogger();
        }

        public SubmittedPaymentMessageProcessor(IDbContext context)
        {
            _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public bool Process(Message message)
        {
            _formattingService = new FormattingServices();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _validationService = new ValidationService(_logger);
            _smsService = new SMSService(_ctx);
            _emailService = new EmailService(_ctx);
            _userService = new UserService(_ctx);

            string fromAddress = "jrhodes2705@gmail.com";
            URIType recipientType = URIType.MobileNumber;

            _logger.Log(LogLevel.Info, String.Format("Processing Payment Message to {0}", message.RecipientUri));
            
            if (_validationService.IsEmailAddress(message.RecipientUri))
                recipientType = URIType.EmailAddress;
            else if (_validationService.IsMECode(message.RecipientUri))
                recipientType = URIType.MECode;

            _logger.Log(LogLevel.Info, String.Format("URI Type {0}", recipientType));
            
            string smsMessage;
            string emailSubject;
            string emailBody;

            var sender = message.Sender;
            var recipient = _userService.GetUser(message.RecipientUri);
            message.Recipient = recipient;

            _logger.Log(LogLevel.Info, "I am Here");
            var senderName = String.IsNullOrEmpty(sender.SenderName) ? _formattingService.FormatMobileNumber(sender.MobileNumber) : sender.SenderName;
            var recipientName = message.RecipientUri;
            //check to see if recipient uri is mobile #, email address, or ME code
            _logger.Log(LogLevel.Info, "I am Here 2");
            
            //Validate Payment

            //Batch Transacations
            _logger.Log(LogLevel.Info, String.Format("Batching Transactions for message {0}", message.Id));

            try
            {
                _transactionBatchService.BatchTransactions(message);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unable to process message {0}. {1}", message.Id, ex.Message));

                throw ex;
            }

            //Attempt to assign payment to Payee
            if (recipient != null)
            {
                recipientName = String.IsNullOrEmpty(recipient.SenderName) ? _formattingService.FormatMobileNumber(message.RecipientUri) : recipient.SenderName;

                //Send out SMS Message to recipient
                _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient"));

                smsMessage = String.Format(_recipientSMSMessage, message.Amount, senderName, _mobileWebSiteUrl);
                _smsService.SendSMS(message.ApiKey, recipient.MobileNumber, smsMessage);

                //Send SMS Message to sender
                _logger.Log(LogLevel.Info, String.Format("Send SMS to Sender"));

                smsMessage = String.Format(_senderSMSMessage, message.Amount, recipientName, _mobileWebSiteUrl);
                _smsService.SendSMS(message.ApiKey, sender.MobileNumber, smsMessage);

                //Send confirmation email to sender
                _logger.Log(LogLevel.Info, String.Format("Sending Email Confirmation to Sender"));

                emailSubject = String.Format(_senderConfirmationEmailSubject, recipientName);
                emailBody = String.Format(_senderConfirmationEmailBody, recipientName, message.Amount, _mobileWebSiteUrl);

                _emailService.SendEmail(message.ApiKey, fromAddress, recipient.EmailAddress, emailSubject, emailBody);

                //Send confirmation email to recipient
                _logger.Log(LogLevel.Info, String.Format("Sending Email Confirmation to Recipient"));

                emailSubject = String.Format(_recipientConfirmationEmailSubject, senderName, message.Amount);
                emailBody = String.Format(_recipientConfirmationEmailBody, senderName, message.Amount, _mobileWebSiteUrl);

                _emailService.SendEmail(message.ApiKey, fromAddress, recipient.EmailAddress, emailSubject, emailBody);

                if (recipient.DeviceToken.Length > 0)
                {
                    //Push Notification to phone
                }
                if (recipient.FacebookUser != null)
                {
                    //Send Facebook Message
                }
            }
            else
            {

                _logger.Log(LogLevel.Info, String.Format("Send SMS to Payee not found"));

                var link = String.Format("{0}{1}", _mobileWebSiteUrl, message.Id.ToString());
                
                //Send out SMS message to sender
                _logger.Log(LogLevel.Info, String.Format("Send SMS to Sender (Recipient is not an registered user)."));

                smsMessage = String.Format(_senderSMSMessageRecipientNotRegistered, message.Amount, message.RecipientUri, link);
                _smsService.SendSMS(message.ApiKey, sender.MobileNumber, smsMessage);

                emailSubject = String.Format(_senderConfirmationEmailSubjectRecipientNotRegistered, message.RecipientUri);
                emailBody = String.Format(_senderConfirmationEmailBodyRecipientNotRegistered, message.Amount, message.RecipientUri);

                //Send confirmation email to sender
                _logger.Log(LogLevel.Info, String.Format("Send Email to Sender (Recipient is not an registered user)."));

                _emailService.SendEmail(message.ApiKey, fromAddress, sender.EmailAddress, emailSubject, emailBody);
                
                if (recipientType == URIType.MobileNumber)
                {
                    //Send out SMS message to recipient
                    _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient (Recipient is not an registered user)."));

                    smsMessage = String.Format(_recipientSMSMessageRecipientNotRegistered, senderName, message.Amount, link);
                    _smsService.SendSMS(message.ApiKey, message.RecipientUri, smsMessage);
                }

                emailSubject = String.Format(_recipientConfirmationEmailSubject, senderName, message.Amount);
                emailBody = String.Format(_recipientConfirmationEmailBody, senderName, message.Amount, _mobileWebSiteUrl);
               
                if (recipientType == URIType.EmailAddress)
                {
                    //Send confirmation email to recipient
                    _logger.Log(LogLevel.Info, String.Format("Send Email to Recipient (Recipient is not an registered user)."));

                    _emailService.SendEmail(message.ApiKey, fromAddress, message.RecipientUri, emailSubject, emailBody);

                }

                var replacementElements = new List<KeyValuePair<string, string>>();
                replacementElements.Add(new KeyValuePair<string, string>("EMAILADDRESS", sender.EmailAddress));

                _emailService.SendEmail(sender.EmailAddress, emailSubject, "Welcome/Registration", replacementElements);

            }
            _logger.Log(LogLevel.Info, String.Format("Updating Payment"));

            message.MessageStatus = MessageStatus.Pending;
            message.LastUpdatedDate = System.DateTime.Now;

            _ctx.SaveChanges();

            return true;

        }

    }
}
