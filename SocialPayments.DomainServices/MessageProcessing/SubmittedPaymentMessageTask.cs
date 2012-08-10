using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using NLog;
using MoonAPNS;
using System.Configuration;

namespace SocialPayments.DomainServices.MessageProcessing
{
    public class SubmittedPaymentMessageTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private string _mobileWebSiteUrl = ConfigurationManager.AppSettings["MobileWebSetURL"];

        private string _recipientSMSMessage = "{1} just sent you {0:C} using PaidThx.  The payment has been submitted for processing. Go to {2}.";
        private string _senderSMSMessage = "Your payment in the amount {0:C} was delivered to {1}.  The payment has been submitted for processing. Go to {2}";

        private string _senderConfirmationEmailSubject = "Confirmation of your payment to {0}";
        private string _senderConfirmationEmailBody = "Your payment in the amount of {0:C} was delivered to {1}.  The payment has been submitted for processing. Go to {2}";
        private string _recipientConfirmationEmailSubject = "{0} just sent you {1:C} using PaidThx.";
        private string _recipientConfirmationEmailBody = "{0} sent you {1:C} using PaidThx. Go to {2} to complete your transaction.";

        private string _senderSMSMessageRecipientNotRegistered = "Your payment of {0:C} was delivered to {1}.  We're waiting for {1} to complete registration. Go to {2}";
        private string _recipientSMSMessageRecipientNotRegistered = "{0} just sent you {1:C} using PaidThx.  Go to {2} to complete the transaction.";

        private string _recipientWasPaidNotification = "{0} sent you {1:C} using PaidThx!";

        private string _senderConfirmationEmailSubjectRecipientNotRegistered = "Confirmation of your payment to {0}.";
        private string _senderConfirmationEmailBodyRecipientNotRegistered = "Your payment in the amount of {0:C} was delivered to {1}.  {1} does not have an account with PaidThx.  We have sent their mobile number information about your payment and instructions to register.";

        private string _paymentReceivedRecipientNotRegisteredTemplate = "Money Received - Not Registered";
        private string _paymentReceivedRecipientRegisteredTemplate = "Money Received - Registered";

        private string _facebookFriendWallPostMessageTemplate = "I sent you ${0} using PaidThx. Why, you ask? {1} Click on this link to pick it up! {2}";
        private string _facebookFriendWallPostLinkTitleTemplate = "You were sent money!";
        private string _facebookFriendWallPostPictureURL = "http://www.crunchbase.com/assets/images/resized/0019/7057/197057v2-max-250x250.png";
        private string _facebookFriendWallPostDescription = "PaidThx lets you send and receive money whenever you want, wherever you want. It is easy to do, and doesn't cost you a penny.";
        private string _facebookFriendWallPostCaption = "The FREE Social Payment Network";

        private string _defaultAvatarImage = ConfigurationManager.AppSettings["DefaultAvatarImage"].ToString();
        private string _fromEmailAddress = ConfigurationManager.AppSettings["FromEmailAddress"];



        public void Execute(Guid messageId)
        {
            using (var ctx = new Context())
            {
                var messageService = new DomainServices.MessageServices(ctx);
                var userService = new DomainServices.UserService(ctx);
                var smsService = new DomainServices.SMSService(ctx);
                var emailService = new DomainServices.EmailService(ctx);
                var urlShortnerService = new DomainServices.GoogleURLShortener();
                var transactionBatchService = new DomainServices.TransactionBatchService(ctx, _logger);
                var communicationService = new DomainServices.CommunicationService(ctx);

                var message = ctx.Messages
                    .FirstOrDefault(m => m.Id == messageId);

                //Batch Transacations
                //Calculate the # of hold days
                var holdDays = 0;
                var scheduledProcessingDate = System.DateTime.Now.Date;
                var verificationLevel = PaymentVerificationLevel.Verified;
                var verifiedLimit = userService.GetUserInstantLimit(message.Sender);

                if (message.Amount > verifiedLimit)
                {
                    holdDays = 3;
                    scheduledProcessingDate = scheduledProcessingDate.AddDays(holdDays);
                    verificationLevel = PaymentVerificationLevel.UnVerified;
                }

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
                    HoldDays = holdDays,
                    ScheduledProcessingDate = scheduledProcessingDate,
                    PaymentVerificationLevel = verificationLevel
                };

                var transactionBatch = transactionBatchService.GetOpenBatch();

                //Create Withdrawal Transaction
                transactionBatch.TotalNumberOfWithdrawals += 1;
                transactionBatch.TotalWithdrawalAmount += message.Payment.Amount;

                ctx.Transactions.Add(
                   new Domain.Transaction()
                   {

                       AccountNumber = message.SenderAccount.AccountNumber,
                       Amount = message.Payment.Amount,
                       AccountType = Domain.AccountType.Checking,
                       ACHTransactionId = "",
                       CreateDate = System.DateTime.Now,
                       Id = Guid.NewGuid(),
                       IndividualIdentifier = message.RecipientUri,
                       NameOnAccount = message.SenderAccount.NameOnAccount,
                       PaymentChannelType = PaymentChannelType.Single,
                       RoutingNumber = message.SenderAccount.RoutingNumber,
                       StandardEntryClass = StandardEntryClass.Web,
                       Status = TransactionStatus.Pending,
                       Type = TransactionType.Deposit,
                       TransactionBatch = transactionBatch
                   });

                message.WorkflowStatus = PaystreamMessageWorkflowStatus.Complete;
                message.LastUpdatedDate = System.DateTime.Now;

                message.shortUrl = urlShortnerService.ShortenURL(_mobileWebSiteUrl, message.Id.ToString());

                //_transactionBatchService.AddTransactionsToBatch(transactionsList);

                ctx.SaveChanges();

                var senderName = userService.GetSenderName(message.Sender);

                if (message.Recipient != null)
                {
                    //create deposit transaction for recipient if they have an account that is verified
                    //and the payment is not being held
                    if (message.Recipient.PaymentAccounts != null && message.Recipient.PaymentAccounts.Count > 0 && message.Payment.ScheduledProcessingDate <= System.DateTime.Now)
                    {
                        transactionBatch.TotalNumberOfDeposits += 1;
                        transactionBatch.TotalWithdrawalAmount += message.Payment.Amount;

                        ctx.Transactions.Add(new Transaction()
                        {
                            AccountNumber = message.Recipient.PaymentAccounts[0].AccountNumber,
                            RoutingNumber = message.Recipient.PaymentAccounts[0].RoutingNumber,
                            NameOnAccount = message.Recipient.PaymentAccounts[0].NameOnAccount,
                            AccountType = Domain.AccountType.Checking,
                            Amount = message.Payment.Amount,
                            CreateDate = System.DateTime.Now,
                            Id = Guid.NewGuid(),
                            PaymentChannelType = PaymentChannelType.Single,
                            SentDate = System.DateTime.Now,
                            StandardEntryClass = StandardEntryClass.Web,
                            Status = TransactionStatus.Pending,
                            TransactionBatch = transactionBatch,
                            Type = TransactionType.Deposit

                        });
                    }
                    if (holdDays > 0)
                        message.Status = PaystreamMessageStatus.HoldPayment;
                    else
                        message.Status = PaystreamMessageStatus.ProcessingPayment;

                    var recipientName = userService.GetSenderName(message.Recipient);

                    //Send SMS Message to recipient
                    if (!String.IsNullOrEmpty(message.Recipient.MobileNumber))
                    {
                        _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient"));

                        smsService.SendSMS(message.ApiKey, message.Recipient.MobileNumber,
                              String.Format(_recipientSMSMessage, message.Amount, senderName, message.shortUrl));
                    }
                    //Send SMS Message to sender
                    if (!String.IsNullOrEmpty(message.Sender.MobileNumber))
                    {
                        _logger.Log(LogLevel.Info, String.Format("Send SMS to Sender"));

                        smsService.SendSMS(message.ApiKey, message.Sender.MobileNumber,
                             String.Format(_senderSMSMessage, message.Amount, recipientName, message.shortUrl));

                    }
                    //Send confirmation email to sender
                    if (!String.IsNullOrEmpty(message.Sender.EmailAddress))
                    {
                        _logger.Log(LogLevel.Info, String.Format("Sending Email Confirmation to Sender"));

                        emailService.SendEmail(message.ApiKey, _fromEmailAddress, message.Sender.EmailAddress,
                            String.Format(_senderConfirmationEmailSubject, recipientName),
                            String.Format(_senderConfirmationEmailBody, recipientName, message.Amount, message.shortUrl));
                    }
                    //Send confirmation email to recipient
                    if (!String.IsNullOrEmpty(message.Recipient.EmailAddress))
                    {
                        _logger.Log(LogLevel.Info, String.Format("Sending Email Confirmation to Recipient"));

                        //Payment Registered Recipient
                        try
                        {
                            emailService.SendEmail(message.Recipient.EmailAddress, String.Format(_recipientConfirmationEmailSubject, senderName, message.Amount),
                                _paymentReceivedRecipientRegisteredTemplate, new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("first_name", message.Recipient.FirstName),
                                new KeyValuePair<string, string>("last_name", message.Recipient.LastName),
                                new KeyValuePair<string, string>("rec_amount",  String.Format("{0:C}", message.Amount)),
                                new KeyValuePair<string, string>("rec_sender", senderName),
                                new KeyValuePair<string, string>("rec_sender_photo_url", (message.Sender.ImageUrl != null ? message.Sender.ImageUrl : _defaultAvatarImage)),
                                new KeyValuePair<string, string>("rec_datetime", String.Format("{0} at {1}", message.CreateDate.ToString("dddd, MMMM dd"), message.CreateDate.ToString("hh:mm tt"))),
                                new KeyValuePair<string, string>("rec_comments", (!String.IsNullOrEmpty(message.Comments) ? message.Comments : "")),
                                new KeyValuePair<string, string>("link_registration", message.shortUrl),
                                new KeyValuePair<string, string>("app_user", "false")
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.Log(LogLevel.Error, String.Format("Unhandled exception sending email to recipient {0}", ex.Message));
                        }
                    }

                    //PUsh Notification Stuff
                    if (!String.IsNullOrEmpty(message.Recipient.DeviceToken))
                    {
                        if (!String.IsNullOrEmpty(message.Recipient.RegistrationId))
                        {
                            _logger.Log(LogLevel.Info, String.Format("Sending Android Push Notification to Recipient"));
                            //Fix this.
                            try
                            {
                                string auth_token = AndroidNotificationService.getToken("android.paidthx@gmail.com", "pdthx123");
                                AndroidNotificationService.sendAndroidPushNotification(
                                    auth_token, message.Recipient.UserId.ToString(), message.Recipient.RegistrationId, senderName, message);
                            }
                            catch (Exception ex)
                            {
                                _logger.Log(LogLevel.Info, String.Format("Exception Pushing Android Notification. {0}", ex.Message));
                            }
                        }
                        else
                        {

                            _logger.Log(LogLevel.Info, String.Format("Sending iOS Push Notification to Recipient"));


                            // We need to know the number of pending requests that the user must take action on for the application badge #
                            // The badge number is the number of PaymentRequests in the Messages database with the Status of (1 - Pending)
                            //      If we are processing a payment, we simply add 1 to the number in this list. This will allow the user to
                            //      Be notified of money received, but it will not stick on the application until the users looks at it. Simplyt
                            //      Opening the application is sufficient
                            var numPending = ctx.Messages.Where(p => p.MessageTypeValue.Equals((int)Domain.MessageType.PaymentRequest) && p.StatusValue.Equals((int)Domain.PaystreamMessageStatus.NotifiedRequest));

                            _logger.Log(LogLevel.Info, String.Format("iOS Push Notification Num Pending: {0}", numPending.Count()));

                            NotificationPayload payload = null;
                            String notification;

                            // Send a mobile push notification
                            if (message.MessageType == Domain.MessageType.Payment)
                            {
                                notification = String.Format(_recipientWasPaidNotification, senderName, message.Amount);
                                payload = new NotificationPayload(message.Recipient.DeviceToken, notification, numPending.Count() + 1);
                                payload.AddCustom("nType", "recPCNF");
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

                    }
                    if (message.Recipient.FacebookUser != null)
                    {
                        //Send Facebook Message
                        // I don't think we can do this through the server. Nice try though.
                        // We should, however, publish something to the user's page that says sender sent payment

                    }
                }
                else
                {
                    message.Status = PaystreamMessageStatus.NotifiedPayment;

                    // var link = String.Format("{0}{1}", _mobileWebSiteUrl, message.Id.ToString());
                    URIType recipientType = messageService.GetURIType(message.RecipientUri);

                    //Send out CoSMS message to sender
                    //if (!String.IsNullOrEmpty(message.Sender.MobileNumber))
                    //{
                    //    _logger.Log(LogLevel.Info, String.Format("Send SMS to Sender (Recipient is not an registered user)."));

                    //    smsService.SendSMS(message.ApiKey, message.Sender.MobileNumber, String.Format(_senderSMSMessageRecipientNotRegistered, message.Amount, message.RecipientUri, message.shortUrl));
                    //}
                    //if (!String.IsNullOrEmpty(message.Sender.EmailAddress))
                    //{
                    //    //Send confirmation email to sender
                    //    _logger.Log(LogLevel.Info, String.Format("Send Email to Sender (Recipient is not an registered user)."));

                    //    emailService.SendEmail(message.ApiKey, _fromEmailAddress, message.Sender.EmailAddress,
                    //        String.Format(_senderConfirmationEmailSubjectRecipientNotRegistered, message.RecipientUri),
                    //        String.Format(_senderConfirmationEmailBodyRecipientNotRegistered, message.Amount, message.RecipientUri));
                    //}

                    //Look at the recipient uri and determine what type of communication to send
                    if (recipientType == URIType.MobileNumber)
                    {

                        var communicationTemplate = communicationService.GetCommunicationTemplate("Payment_NotRegistered_SMS");

                        //Send out SMS message to recipient
                        _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient (Recipient is not an registered user)."));

                        var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");

                        smsService.SendSMS(message.ApiKey, message.RecipientUri,
                            String.Format(communicationTemplate.Template, senderName, message.Amount, 
                            comment, message.shortUrl));
                    }

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

                        var communicationTemplate = communicationService.GetCommunicationTemplate("Payment_NotRegistered_Email");

                        emailService.SendEmail(message.RecipientUri,
                            String.Format(_recipientConfirmationEmailSubject, senderName, message.Amount)
                            , communicationTemplate.Template, new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("first_name", ""),
                                new KeyValuePair<string, string>("last_name", ""),
                                new KeyValuePair<string, string>("rec_amount",  String.Format("{0:C}", message.Amount)),
                                new KeyValuePair<string, string>("rec_sender", senderName),
                                new KeyValuePair<string, string>("rec_sender_photo_url", (message.Sender.ImageUrl != null ? message.Sender.ImageUrl : _defaultAvatarImage)),
                                new KeyValuePair<string, string>("rec_datetime", String.Format("{0} at {1}", message.CreateDate.ToString("dddd, MMMM dd"), message.CreateDate.ToString("hh:mm tt"))),
                                new KeyValuePair<string, string>("rec_comments", (!String.IsNullOrEmpty(message.Comments) ? message.Comments : "")),
                                new KeyValuePair<string, string>("link_registration", message.shortUrl),
                                new KeyValuePair<string, string>("app_user", "false")
                            });
                    }
                    if (recipientType == URIType.FacebookAccount)
                    {
                        try
                        {
                            var communicationTemplate = communicationService.GetCommunicationTemplate("Payment_NotRegistered_Facebook");
                            
                            var client = new Facebook.FacebookClient(message.Sender.FacebookUser.OAuthToken);
                            var args = new Dictionary<string, object>();

                            // All this next line is doing is ending it with a period if it does not end in a period or !
                            // I'm sure this can be done better, but for now it looks good.
                            var formattedComments = message.Comments.Trim();

                            if (!(message.Comments.Length > 0 && message.Comments[message.Comments.Length - 1].Equals('.')) && !(message.Comments.Length > 0 && message.Comments[message.Comments.Length - 1].Equals('!')))
                                formattedComments = String.Format("{0}.", formattedComments);

                            args["message"] = String.Format(communicationTemplate.Template, message.Amount, formattedComments, message.shortUrl);
                            args["link"] = message.shortUrl;

                            args["name"] = _facebookFriendWallPostLinkTitleTemplate;
                            args["caption"] = _facebookFriendWallPostCaption;
                            args["picture"] = _facebookFriendWallPostPictureURL;
                            args["description"] = _facebookFriendWallPostDescription;

                            client.Post(String.Format("/{0}/feed", message.RecipientUri.Substring(3)), args);
                        }
                        catch (Exception ex)
                        {
                            _logger.Log(LogLevel.Error, ex.Message);
                        }
                    }
                }
            }
        }
    }
}
