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
using SocialPayments.DomainServices.Interfaces;


namespace SocialPayments.Services.MessageProcessors
{

    public class SubmittedRequestMessageProcessor : IMessageProcessor.IMessageProcessor
    {
        private IDbContext _ctx;
        private Logger _logger;

        private FormattingServices _formattingService;
        private TransactionBatchService _transactionBatchService;
        private ValidationService _validationService;
        private UserService _userService;
        private ISMSService _smsService;
        private IEmailService _emailService;
        private IAmazonNotificationService _amazonNotificationService;
        private MessageServices _messageService;

        private string _recipientSMSMessage = "You received a PdThx request for {0:C} from {1}.";
        private string _recipientConfirmationEmailSubject = "You received a payment request for {0:C} from {1} using PaidThx.";
        private string _recipientConfirmationEmailBody = "You received a PdThx request for {0:C} from {1}.";
        private string _senderSMSMessage = "Your PdThx request for {0:C} to {1} was sent.";
        private string _senderConfirmationEmailSubject = "Confirmation of your PaidThx request to {0}.";
        private string _senderConfirmationEmailBody = "Your PaidThx request in the amount of {0:C} was delivered to {1}.";
        
        private string _fromAddress;

        public SubmittedRequestMessageProcessor() {
            _ctx  = new DataLayer.Context();
            _logger = LogManager.GetCurrentClassLogger();

            _formattingService = new FormattingServices();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _validationService = new ValidationService(_logger);
            _smsService = new SMSService(_ctx);
            _emailService = new EmailService(_ctx);
            _userService = new UserService(_ctx);
            _messageService = new MessageServices(_ctx, _amazonNotificationService);

            _fromAddress = "jrhodes2705@gmail.com";
            
        }

        public SubmittedRequestMessageProcessor(IDbContext context)
        {
            _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();

            _formattingService = new FormattingServices();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _validationService = new ValidationService(_logger);
            _smsService = new SMSService(_ctx);
            _emailService = new EmailService(_ctx);
            _userService = new UserService(_ctx);
            _amazonNotificationService = new AmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);

            _fromAddress = "jrhodes2705@gmail.com";
            
        }
        public SubmittedRequestMessageProcessor(IDbContext context, IEmailService emailService, ISMSService smsService)
        {
            _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();

            _formattingService = new FormattingServices();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _validationService = new ValidationService(_logger);
            _smsService = smsService;
            _emailService = emailService;
            _userService = new UserService(_ctx);
            _amazonNotificationService = new AmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);

            _fromAddress = "jrhodes2705@gmail.com";

        }
        public bool Process(Message message)
        {
            _logger.Log(LogLevel.Info, String.Format("Processing Request Message to {0}", message.RecipientUri));

            //Validate Payment Request
            //Batch Transacation WithDrawal

            URIType recipientType = _messageService.GetURIType(message.RecipientUri);

            _logger.Log(LogLevel.Info, String.Format("URI Type {0}", recipientType));
            
            //Attempt to find the recipient of the request
            var sender = message.Sender;
            _logger.Log(LogLevel.Info, String.Format("Getting Recipient"));

            var recipient = _userService.GetUser(message.RecipientUri);
            message.Recipient = recipient;

            string smsMessage;
            string emailSubject;
            string emailBody;

            _logger.Log(LogLevel.Info, String.Format("Getting Sender"));

            var senderName = _userService.GetSenderName(sender);
            _logger.Log(LogLevel.Info, String.Format("Retrieved Sender Name {0}", senderName));

            var recipientName = message.RecipientUri;
            
            if (recipient != null)
            {
                _logger.Log(LogLevel.Debug, String.Format("Recipient Found"));

                //if the recipient has a mobile #; send SMS
                if (!String.IsNullOrEmpty(recipient.MobileNumber))
                {
                    _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient"));

                    smsMessage = String.Format(_recipientSMSMessage, message.Amount, senderName);
                    _smsService.SendSMS(message.ApiKey, recipient.MobileNumber, smsMessage);
                }

                //if the recipient has an email address; send an email
                if(!String.IsNullOrEmpty(recipient.EmailAddress))
                {

                    _logger.Log(LogLevel.Info, String.Format("Send Email to Recipient"));
                    
                    emailSubject = String.Format(_recipientConfirmationEmailSubject, message.Amount, senderName);
                    emailBody = String.Format(_recipientConfirmationEmailBody, message.Amount, senderName);
    
                    _emailService.SendEmail(message.ApiKey, _fromAddress, recipient.EmailAddress, emailSubject, emailBody);

                }
                //if the recipient has a device token; send a push notification
                if (!String.IsNullOrEmpty(recipient.DeviceToken))
                {
                    _logger.Log(LogLevel.Info, String.Format("Send Push Notification to Recipient"));

                }
                //if the recipient has a linked facebook account; send a facebook message
                if (recipient.FacebookUser != null)
                {
                    _logger.Log(LogLevel.Info, String.Format("Send Facebook Message to Recipient"));

                }
               
            }
            else
            {
                //if recipient Uri Type is Mobile Number, Send SMS
                if (recipientType == URIType.MobileNumber)
                {
                    _logger.Log(LogLevel.Info, String.Format("Send SMS Message to Recipient"));

                    smsMessage = String.Format(_recipientSMSMessage, message.Amount, senderName);
                    _smsService.SendSMS(message.ApiKey, message.RecipientUri, smsMessage);
                }
                //if recipient Uri Type is email address, Send Email
                if (recipientType == URIType.EmailAddress)
                {
                    _logger.Log(LogLevel.Info, String.Format("Send Emaili Message to Recipient"));

                    emailSubject = String.Format(_recipientConfirmationEmailSubject, message.Amount, senderName);
                    emailBody = String.Format(_recipientConfirmationEmailBody, message.Amount, senderName);

                    _emailService.SendEmail(message.ApiKey, _fromAddress, message.RecipientUri, emailSubject, emailBody);

                }
                
            }

            //if sender has mobile #, send confirmation email to sender
            if (!String.IsNullOrEmpty(sender.MobileNumber))
            {
                _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient"));

                smsMessage = String.Format(_senderSMSMessage, message.Amount, recipientName);
                _smsService.SendSMS(message.ApiKey, sender.MobileNumber, smsMessage);

            }

            //if sender has email address; send an email
            if (!String.IsNullOrEmpty(sender.EmailAddress))
            {
                _logger.Log(LogLevel.Info, String.Format("Send Email to Recipient"));

                emailSubject = String.Format(_senderConfirmationEmailSubject, recipientName);
                emailBody = String.Format(_senderConfirmationEmailBody, message.Amount, recipientName);

                _emailService.SendEmail(message.ApiKey, _fromAddress, sender.EmailAddress, emailSubject, emailBody);

            }

            //if sender has a device token; send a push notification
            if (!String.IsNullOrEmpty(sender.DeviceToken))
            {
                _logger.Log(LogLevel.Info, String.Format("Send Push Notification to Recipient"));

            }
            //if sender has a linked facebook account; send a facebook message
            if (sender.FacebookUser != null)
            {
                _logger.Log(LogLevel.Info, String.Format("Send Facebook Message to Recipient"));
            }
            if (recipientType == URIType.FacebookAccount)
            {
                try
                {
                    var client = new Facebook.FacebookClient(sender.FacebookUser.OAuthToken);
                    var args = new Dictionary<string, object>();

                    // All this next line is doing is ending it with a period if it does not end in a period or !
                    // I'm sure this can be done better, but for now it looks good.
                    var formattedComments = message.Comments.Trim();

                    if (!(message.Comments.Length > 0 && message.Comments[message.Comments.Length - 1].Equals('.')) && !(message.Comments.Length > 0 && message.Comments[message.Comments.Length - 1].Equals('!')))
                        formattedComments = String.Format("{0}.", formattedComments);

                    args["message"] = String.Format("I requested ${0} from you using PaidThx. Why, you ask? {1} Click on this link to send me it! {2}", message.Amount, formattedComments, message.shortUrl);
                    args["link"] = message.shortUrl;

                    args["name"] = "PaidThx";
                    args["caption"] = "The FREE Social Payment Network";
                    args["picture"] = "http://www.crunchbase.com/assets/images/resized/0019/7057/197057v2-max-250x250.png";
                    args["description"] = "PaidThx lets you send and receive money whenever you want, wherever you want. Whether you owe a friend a few bucks or want to donate to your favorite cause, it should be simple and not cost you a penny.";
                    
                    client.Post(String.Format("/{0}/feed", message.RecipientUri.Substring(3)), args);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, ex.Message);
                }

            }

            //Update Payment Status
            _logger.Log(LogLevel.Info, String.Format("Updating Payment Request"));

            message.Status = PaystreamMessageStatus.Processing;
            message.LastUpdatedDate = System.DateTime.Now;

            _ctx.SaveChanges();

            return true;
        }
    }
}
