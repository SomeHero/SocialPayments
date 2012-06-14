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
using MoonAPNS;
using SocialPayments.DomainServices.Interfaces;
using System.Collections.ObjectModel;

namespace SocialPayments.Services.MessageProcessors
{
    public class SubmittedPaymentMessageProcessor : IMessageProcessor.IMessageProcessor
    {
        private IDbContext _ctx;
        private Logger _logger;

        private FormattingServices _formattingService;
        private TransactionBatchService _transactionBatchService;
        private ValidationService _validationService;
        private UserService _userService;
        private ISMSService _smsService;
        private IEmailService _emailService;
        private MessageServices _messageService;

        private string _recipientSMSMessage = "{1} just sent you {0:C} using PaidThx.  The payment has been submitted for processing. Go to {2}.";
        private string _senderSMSMessage = "Your payment in the amount {0:C} was delivered to {1}.  The payment has been submitted for processing. Go to {2}";

        private string _senderConfirmationEmailSubject = "Confirmation of your payment to {0}";
        private string _senderConfirmationEmailBody = "Your payment in the amount of {0:C} was delivered to {1}.  The payment has been submitted for processing. Go to {2}";
        private string _recipientConfirmationEmailSubject = "{0} just sent you {1:C} using PaidThx.";
        private string _recipientConfirmationEmailBody = "{0} sent you {1:C} using PaidThx. Go to {2} to complete your transaction.";

        private string _senderSMSMessageRecipientNotRegistered= "Your payment of {0:C} was delivered to {1}. The payment is pending unit {1} completes registration with PaidThx. Go to {2}";
        private string _recipientSMSMessageRecipientNotRegistered = "{0} just sent you {1:C} using PaidThx.  Go to {2} to complete the transaction.";

        private string _recipientWasPaidNotification = "{0} sent you {1:C} using PaidThx!";
        private string _recipientRequestNotification = "{0} requested {1:C} from you using PaidThx!";

        private string _senderConfirmationEmailSubjectRecipientNotRegistered = "Confirmation of your payment to {0}.";
        private string _senderConfirmationEmailBodyRecipientNotRegistered = "Your payment in the amount of {0:C} was delivered to {1}.  {1} does not have an account with PaidThx.  We have sent their mobile number information about your payment and instructions to register.";
        private string _mobileWebSiteUrl = System.Configuration.ConfigurationManager.AppSettings["MobileWebSetURL"];
       
        private string _paymentReceivedRecipientNotRegisteredTemplate = "Payment Received Recipient Not Registered";
        private string _paymentReceivedRecipientRegisteredTemplate = "Payment Received Recipient Registered";

        private string _defaultAvatarImage = System.Configuration.ConfigurationManager.AppSettings["DefaultAvatarImage"].ToString();

        public SubmittedPaymentMessageProcessor() {
            _ctx  = new DataLayer.Context();
            _logger = LogManager.GetCurrentClassLogger();
        }

        public SubmittedPaymentMessageProcessor(IDbContext context, IEmailService emailService, ISMSService smsService)
        {
            _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();
            _smsService = smsService;
            _emailService = emailService;
        }

        public bool Process(Message message)
        {
            _formattingService = new FormattingServices();
            _transactionBatchService = new TransactionBatchService(_ctx, _logger);
            _validationService = new ValidationService(_logger);
            _userService = new UserService(_ctx);

            IAmazonNotificationService _amazonNotificationService = new AmazonNotificationService();
            _messageService = new MessageServices(_ctx, _amazonNotificationService);

            string fromAddress = "jrhodes2705@gmail.com";
            URIType recipientType = _messageService.GetURIType(message.RecipientUri);

            _logger.Log(LogLevel.Info, String.Format("Processing Payment Message to {0} - {1}", message.RecipientUri, _defaultAvatarImage));

            _logger.Log(LogLevel.Info, String.Format("URI Type {0}", recipientType));
            
            string smsMessage;
            string emailSubject;
            string emailBody;

            var sender = message.Sender;
            var recipient = _userService.GetUser(message.RecipientUri);

            var senderName = _userService.GetSenderName(sender);
            var recipientName = message.RecipientUri;
            //check to see if recipient uri is mobile #, email address, or ME code

            //Validate Payment

            //Batch Transacations
            _logger.Log(LogLevel.Info, String.Format("Batching Transactions for message {0}", message.Id));

            try
            {
                var transactionsList = new Collection<Transaction>();

                message.Recipient = recipient;
                //Create Payment
                message.Payment = new Payment()
                {
                    Amount = message.Amount,
                    ApiKey = message.ApiKey,
                    Comments = message.Comments,
                    CreateDate = System.DateTime.Now,
                    Id = Guid.NewGuid(),
                    PaymentStatus = PaymentStatus.Pending,
                    SenderAccount = message.SenderAccount,
                };

                transactionsList.Add(_ctx.Transactions.Add(new Transaction()
                {
                    Amount = message.Payment.Amount,
                    Category = TransactionCategory.Payment,
                    CreateDate = System.DateTime.Now,
                    FromAccount = message.SenderAccount,
                    Id = Guid.NewGuid(),
                    Payment = message.Payment,
                    PaymentChannelType = PaymentChannelType.Single,
                    SentDate = System.DateTime.Now,
                    StandardEntryClass = StandardEntryClass.Web,
                    Status = TransactionStatus.Pending,
                    //TransactionBatch = transactionBatch,
                    Type = TransactionType.Withdrawal,
                    User = sender
                }));
                
                if (recipient != null && recipient.PaymentAccounts[0] != null)
                {
                    transactionsList.Add(_ctx.Transactions.Add(new Transaction()
                    {
                        Amount = message.Payment.Amount,
                        Category = TransactionCategory.Payment,
                        CreateDate = System.DateTime.Now,
                        FromAccount = recipient.PaymentAccounts[0],
                        Id = Guid.NewGuid(),
                        Payment = message.Payment,
                        PaymentChannelType = PaymentChannelType.Single,
                        SentDate = System.DateTime.Now,
                        StandardEntryClass = StandardEntryClass.Web,
                        Status = TransactionStatus.Pending,
                        //TransactionBatch = transactionBatch,
                        Type = TransactionType.Deposit,
                        User = sender
                    }));
                }

                _logger.Log(LogLevel.Info, String.Format("Updating Payment"));

                message.WorkflowStatus = PaystreamMessageWorkflowStatus.Complete;
                message.LastUpdatedDate = System.DateTime.Now;

                _transactionBatchService.AddTransactionsToBatch(transactionsList);

                _ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unable to process message {0}. {1}", message.Id, ex.Message));

                throw ex;
            }

            //Attempt to assign payment to Payee
            if (recipient != null)
            {
                recipientName = recipient.UserName;

                if (!String.IsNullOrEmpty(recipient.SenderName))
                    recipientName = recipient.SenderName;
                else if (!String.IsNullOrEmpty(recipient.MobileNumber))
                    recipientName = _formattingService.FormatMobileNumber(recipient.MobileNumber);

                //Send out SMS Message to recipient
                if (!String.IsNullOrEmpty(recipient.MobileNumber))
                {
                    _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient"));

                    smsMessage = String.Format(_recipientSMSMessage, message.Amount, senderName, _mobileWebSiteUrl);
                    _smsService.SendSMS(message.ApiKey, recipient.MobileNumber, smsMessage);
                }
                //Send SMS Message to sender
                if (!String.IsNullOrEmpty(sender.MobileNumber))
                {
                    _logger.Log(LogLevel.Info, String.Format("Send SMS to Sender"));

                    smsMessage = String.Format(_senderSMSMessage, message.Amount, recipientName, _mobileWebSiteUrl);
                    _smsService.SendSMS(message.ApiKey, sender.MobileNumber, smsMessage);

                }
                //Send confirmation email to sender
                if (!String.IsNullOrEmpty(sender.EmailAddress))
                {
                    _logger.Log(LogLevel.Info, String.Format("Sending Email Confirmation to Sender"));

                    emailSubject = String.Format(_senderConfirmationEmailSubject, recipientName);
                    emailBody = String.Format(_senderConfirmationEmailBody, recipientName, message.Amount, _mobileWebSiteUrl);

                    _emailService.SendEmail(message.ApiKey, fromAddress, sender.EmailAddress, emailSubject, emailBody);
                }
                //Send confirmation email to recipient
                if (!String.IsNullOrEmpty(recipient.EmailAddress))
                {
                    _logger.Log(LogLevel.Info, String.Format("Sending Email Confirmation to Recipient"));

                    emailSubject = String.Format(_recipientConfirmationEmailSubject, senderName, message.Amount);
                    
            //Payment Registered Recipient
            //first_name
            //last_name
            //rec_amount
            //rec_sender
            //rec_sender_photo_url
            //rec_datetime formatted dddd, MMMM dd(rd) at hh:mm tt
            //rec_comments
            //app_user
            //link_registration - empty
                    try
                    {
                        _emailService.SendEmail(recipient.EmailAddress, emailSubject, _paymentReceivedRecipientRegisteredTemplate, new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("first_name", recipient.FirstName),
                            new KeyValuePair<string, string>("last_name", recipient.LastName),
                            new KeyValuePair<string, string>("rec_amount",  String.Format("{0:C}", message.Amount)),
                            new KeyValuePair<string, string>("rec_sender", senderName),
                            new KeyValuePair<string, string>("rec_sender_photo_url", (sender.ImageUrl != null ? sender.ImageUrl : _defaultAvatarImage)),
                            new KeyValuePair<string, string>("rec_datetime", String.Format("{0} at {1}", message.CreateDate.ToString("dddd, MMMM dd"), message.CreateDate.ToString("hh:mm tt"))),
                            new KeyValuePair<string, string>("rec_comments", (!String.IsNullOrEmpty(message.Comments) ? message.Comments : "")),
                            new KeyValuePair<string, string>("link_registration", "http://paidthx.com/mobile"),
                            new KeyValuePair<string, string>("app_user", "false")
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Unhandled exception sending email to recipient {0}", ex.Message));
                    }
                }
                if (recipient.DeviceToken.Length > 0)
                {
                    _logger.Log(LogLevel.Info, String.Format("Sending iOS Push Notification to Recipient"));
                    

                    // We need to know the number of pending requests that the user must take action on for the application badge #
                    // The badge number is the number of PaymentRequests in the Messages database with the Status of (1 - Pending)
                    //      If we are processing a payment, we simply add 1 to the number in this list. This will allow the user to
                    //      Be notified of money received, but it will not stick on the application until the users looks at it. Simplyt
                    //      Opening the application is sufficient
                    var numPending = _ctx.Messages.Where(p => p.MessageTypeValue.Equals((int)Domain.MessageType.PaymentRequest) && p.StatusValue.Equals((int)Domain.PaystreamMessageStatus.Processing));

                    _logger.Log(LogLevel.Info, String.Format("iOS Push Notification Num Pending: {0}", numPending.Count()));
                    
                    NotificationPayload payload = null;
                    String notification;

                    // Send a mobile push notification
                    if (message.MessageType == Domain.MessageType.Payment)
                    {
                        notification = String.Format(_recipientWasPaidNotification, senderName, message.Amount);
                        payload = new NotificationPayload(recipient.DeviceToken, notification, numPending.Count()+1);
                        payload.AddCustom("nType", "recPCNF");
                    }
                    else if (message.MessageType == Domain.MessageType.PaymentRequest)
                    {
                        notification = String.Format(_recipientRequestNotification, senderName, message.Amount);
                        payload = new NotificationPayload(recipient.DeviceToken, notification, numPending.Count());
                        payload.AddCustom("nType", "recPRQ");
                    }

                    /*
                     *  Payment Notification Types:
                     *      Payment Request [recPRQ]
                     *          - Recipient receives notification that takes them to the
                     *                 paystream detail view about that payment request
                     *      Payment Confirmation [recPCNF]
                     *          - Recipient receices notification that takes them to the paysteam detail view about the payment request
                     */
                    
                    payload.AddCustom("tID", message.Id);
                    var notificationList = new List<NotificationPayload>() { payload };

                    List<string> result;

                    try
                    {
                        var push = new PushNotification(true, @"C:\APNS\DevKey\aps_developer_identity.p12", "KKreap1566");
                        result = push.SendToApple(notificationList); // You are done!
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Fatal, String.Format("Exception sending iOS push notification. {0}", ex.Message));
                        var exception = ex.InnerException;

                        while (exception != null)
                        {
                            _logger.Log(LogLevel.Fatal, String.Format("Exception sending iOS push notification. {0}", exception.Message));
                        
                        }
                    }
                    
                }
                if (recipient.FacebookUser != null)
                {
                    //Send Facebook Message
                    // I don't think we can do this through the server. Nice try though.
                    // We should, however, publish something to the user's page that says sender sent payment
                    
                }

            }
            else
            {

                _logger.Log(LogLevel.Info, String.Format("Send SMS to Payee not found"));

                var link = String.Format("{0}{1}", _mobileWebSiteUrl, message.Id.ToString());

                //Send out SMS message to sender
                if (!String.IsNullOrEmpty(sender.MobileNumber))
                {
                    _logger.Log(LogLevel.Info, String.Format("Send SMS to Sender (Recipient is not an registered user)."));

                    smsMessage = String.Format(_senderSMSMessageRecipientNotRegistered, message.Amount, message.RecipientUri, link);
                    _smsService.SendSMS(message.ApiKey, sender.MobileNumber, smsMessage);
                }
                if (!String.IsNullOrEmpty(sender.EmailAddress))
                {
                    emailSubject = String.Format(_senderConfirmationEmailSubjectRecipientNotRegistered, message.RecipientUri);
                    emailBody = String.Format(_senderConfirmationEmailBodyRecipientNotRegistered, message.Amount, message.RecipientUri);

                    //Send confirmation email to sender
                    _logger.Log(LogLevel.Info, String.Format("Send Email to Sender (Recipient is not an registered user)."));

                    _emailService.SendEmail(message.ApiKey, fromAddress, sender.EmailAddress, emailSubject, emailBody);
                }
                if (recipientType == URIType.MobileNumber)
                {
                    //Send out SMS message to recipient
                    _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient (Recipient is not an registered user)."));

                    smsMessage = String.Format(_recipientSMSMessageRecipientNotRegistered, senderName, message.Amount, link);
                    _smsService.SendSMS(message.ApiKey, message.RecipientUri, smsMessage);
                }

                emailSubject = String.Format(_recipientConfirmationEmailSubject, senderName, message.Amount);
                
                //Payment Registered Recipient
                //first_name
                //last_name
                //rec_amount
                //rec_sender
                //rec_sender_photo_url
                //rec_datetime formatted DayOfWeek, MM dd(rd) at hh:mm:tt
                //rec_comments
                //app_user
                //link_registration - empty
                if (recipientType == URIType.EmailAddress)
                {
                    //Send confirmation email to recipient
                    _logger.Log(LogLevel.Info, String.Format("Send Email to Recipient (Recipient is not an registered user)."));

                    _emailService.SendEmail(message.RecipientUri, emailSubject, _paymentReceivedRecipientNotRegisteredTemplate, new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("first_name", ""),
                        new KeyValuePair<string, string>("last_name", ""),
                        new KeyValuePair<string, string>("rec_amount",  String.Format("{0:C}", message.Amount)),
                        new KeyValuePair<string, string>("rec_sender", senderName),
                        new KeyValuePair<string, string>("rec_sender_photo_url", (sender.ImageUrl != null ? sender.ImageUrl : _defaultAvatarImage)),
                        new KeyValuePair<string, string>("rec_datetime", String.Format("{0} at {1}", message.CreateDate.ToString("dddd, MMMM dd"), message.CreateDate.ToString("hh:mm tt"))),
                        new KeyValuePair<string, string>("rec_comments", (!String.IsNullOrEmpty(message.Comments) ? message.Comments : "")),
                        new KeyValuePair<string, string>("link_registration", link),
                        new KeyValuePair<string, string>("app_user", "false")
                    });
                }

            }

            return true;

        }

    }
}
