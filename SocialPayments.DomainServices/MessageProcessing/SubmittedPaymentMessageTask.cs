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

        private string _defaultAvatarImage = ConfigurationManager.AppSettings["DefaultAvatarImage"].ToString();
        private string _mobileWebSiteUrl = ConfigurationManager.AppSettings["MobileWebSetURL"];
        private string _mobileWebSiteEngageURl = ConfigurationManager.AppSettings["MobileWebSetURL"];

        private string _recipientWasPaidNotification = "{0} sent you {1:C} using PaidThx!";

        private DomainServices.MessageServices _messageServices;
        private DomainServices.UserService _userServices;
        private DomainServices.EmailService _emailServices;
        private DomainServices.SMSService _smsServices;
        private DomainServices.CommunicationService _communicationServices;
        private DomainServices.FacebookServices _facebookServices;
        private DomainServices.IOSNotificationServices _iosNotificationServices;

        private string _senderName;

        public void Execute(Guid messageId)
        {
            using (var ctx = new Context())
            {
                try
                {
                    _messageServices = new DomainServices.MessageServices();
                    _userServices = new DomainServices.UserService(ctx);
                    _smsServices = new DomainServices.SMSService(ctx);
                    _emailServices = new DomainServices.EmailService(ctx);
                    _communicationServices = new DomainServices.CommunicationService(ctx);
                    _facebookServices = new DomainServices.FacebookServices(ctx);
                    _iosNotificationServices = new DomainServices.IOSNotificationServices(ctx);

                    var communicationService = new DomainServices.CommunicationService(ctx);
                    var urlShortnerService = new DomainServices.GoogleURLShortener();
                    var transactionBatchServices = new DomainServices.TransactionBatchService(ctx, _logger);

                    var message = _messageServices.GetMessage(messageId);
                    ctx.Messages.Attach(message);

                    //Batch Transacations
                    //Calculate the # of hold days
                    var holdDays = 0;
                    var scheduledProcessingDate = System.DateTime.Now.Date;
                    var verificationLevel = PaymentVerificationLevel.Verified;
                    var verifiedLimit = _userServices.GetUserInstantLimit(message.Sender);

                    _senderName = _userServices.GetSenderName(message.Sender);

                    if (message.Amount > verifiedLimit)
                    {
                        holdDays = 3;
                        scheduledProcessingDate = scheduledProcessingDate.AddDays(holdDays);
                        verificationLevel = PaymentVerificationLevel.UnVerified;
                    }

                    var transactionBatch = transactionBatchServices.GetOpenBatch();
                    ctx.TransactionBatches.Attach(transactionBatch);

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
                        PaymentVerificationLevel = verificationLevel,
                        EstimatedDeliveryDate = System.DateTime.Now,
                        ExpressDeliveryFee = message.deliveryFeeAmount,
                        ExpressDeliveryDate = System.DateTime.Now,
                        IsExpressed = (message.deliveryMethod == DeliveryMethod.Express ? true : false),
                        Transactions = new List<Transaction>()
                    };

                    //Add the withdrawal transaction
                    message.Payment.Transactions.Add(new Domain.Transaction()
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
                        Type = TransactionType.Withdrawal,
                        TransactionBatch = transactionBatch,
                        Payment = message.Payment
                    });

                    transactionBatch.TotalNumberOfWithdrawals += 1;
                    transactionBatch.TotalWithdrawalAmount += message.Payment.Amount;

                    message.WorkflowStatus = PaystreamMessageWorkflowStatus.Complete;
                    message.LastUpdatedDate = System.DateTime.Now;

                    message.shortUrl = urlShortnerService.ShortenURL(_mobileWebSiteUrl, message.Id.ToString());
                    var recipient = _userServices.GetUser(message.RecipientUri);

                    if (recipient != null)
                    {
                        ctx.Users.Attach(recipient);
                       message.Recipient = recipient;

                        string recipientName = _userServices.GetSenderName(message.Recipient);
                        bool isRecipientEngaged = true;

                        var preferredReceiveAccount = message.Recipient.PreferredReceiveAccount;

                        if (preferredReceiveAccount == null)
                            isRecipientEngaged = false;

                        //create deposit transaction for recipient if they have an account that is verified
                        //and the payment is not being held
                        if (isRecipientEngaged && message.Payment.ScheduledProcessingDate <= System.DateTime.Now)
                        {
                            message.Status = PaystreamMessageStatus.ProcessingPayment;

                            transactionBatch.TotalNumberOfDeposits += 1;
                            transactionBatch.TotalWithdrawalAmount += message.Payment.Amount;

                            message.Payment.Transactions.Add(new Transaction()
                            {
                                AccountNumber = preferredReceiveAccount.AccountNumber,
                                RoutingNumber = preferredReceiveAccount.RoutingNumber,
                                NameOnAccount = preferredReceiveAccount.NameOnAccount,
                                AccountType = Domain.AccountType.Checking,
                                ACHTransactionId = "",
                                Amount = message.Payment.Amount,
                                CreateDate = System.DateTime.Now,
                                Id = Guid.NewGuid(),
                                PaymentChannelType = PaymentChannelType.Single,
                                StandardEntryClass = StandardEntryClass.Web,
                                Status = TransactionStatus.Pending,
                                TransactionBatch = transactionBatch,
                                Type = TransactionType.Deposit,
                                Payment = message.Payment,
                                IndividualIdentifier = _senderName
                            });
                        }

                        //Send SMS Message to recipient - not engaged
                        if (!isRecipientEngaged)
                        {
                            message.Status = PaystreamMessageStatus.ProcessingPayment;

                            //Send Recipient Not Engaged Communication
                            try
                            {
                                SendRecipientNotEngagedCommunication(message);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else
                        {
                            if (holdDays > 0)
                                message.Status = PaystreamMessageStatus.HoldPayment;
                            else
                                message.Status = PaystreamMessageStatus.ProcessingPayment;
                            try
                            {
                                SendRecipientReceiptCommunication(message);
                            }
                            catch (Exception ex)
                            { }
                        }
                    }
                    else
                    {
                        message.Status = PaystreamMessageStatus.NotifiedPayment;

                        try
                        {
                            SendRecipientNotRegisteredCommunication(message);
                        }
                        catch (Exception ex)
                        { }
                    }

                    ctx.SaveChanges();
                }
                catch(Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Occurred Executing Post Payment Message Task. Exception: {0}. Stack Trace: {1}", ex.Message, ex.StackTrace));

                    var innerException = ex.InnerException;
                    while (innerException !=  null)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Inner Exception Occurred Executing Post Payment Message Task. Inner Exception: {0}. Stack Trace: {1}", innerException.Message, innerException.StackTrace));
                        innerException = innerException.InnerException;
                    }
                }

            }
        }
        private void SendRecipientNotEngagedCommunication(Domain.Message message)
        {
            if (!String.IsNullOrEmpty(message.Recipient.MobileNumber))
            {
                _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient - Not Engaged"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Payment_NotEngaged_SMS");

                if (communicationTemplate != null)
                {
                    _smsServices.SendSMS(message.ApiKey, message.Recipient.MobileNumber,
                          String.Format(communicationTemplate.Template, _senderName, message.Amount, _mobileWebSiteUrl));
                }
            }
            if (!String.IsNullOrEmpty(message.Recipient.EmailAddress))
            {
                _logger.Log(LogLevel.Info, String.Format("Sending Email Confirmation to Recipient"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Payment_NotEngaged_Email");

                try
                {
                    var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");
                    DateTime createDate = ConvertToLocalTime(message.CreateDate, "Eastern Standard Time");
                        
                    _emailServices.SendEmail(message.Recipient.EmailAddress, String.Format("{0} sent you {1} using PaidThx", _senderName, message.Amount),
                                        communicationTemplate.Template, new List<KeyValuePair<string, string>>()
                                {
                                    new KeyValuePair<string, string>("REC_SENDER", _senderName),
                                    new KeyValuePair<string, string>("REC_AMOUNT", String.Format("{0:C}", message.Amount)),
                                    new KeyValuePair<string, string>("REC_SENDER_PHOTO_URL",  (message.Sender.ImageUrl != null ? message.Sender.ImageUrl : _defaultAvatarImage)),
                                    new KeyValuePair<string, string>("REC_DATETIME", String.Format("{0} at {1}",createDate.ToString("dddd, MMMM dd"), createDate.ToString("hh:mm tt"))),
                                    new KeyValuePair<string, string>("REC_COMMENTS", (!String.IsNullOrEmpty(message.Comments) ? message.Comments : "")),
                                    new KeyValuePair<string, string>("REC_COMMENTS_DISPLAY", (String.IsNullOrEmpty(message.Comments) ? "display: none" : "")),
                                    new KeyValuePair<string, string>("LINK_ENGAGE",  _mobileWebSiteEngageURl),
                                });
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled exception sending email to recipient {0}", ex.Message));
                }
            }
            if (message.Recipient.FacebookUser != null)
            {
                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Payment_NotEngaged_Facebook");
                    
                try
                {
                    var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");
 
                    _facebookServices.MakeWallPost(message.Sender.FacebookUser.OAuthToken, message.Recipient.FacebookUser.FBUserID,
                        String.Format(communicationTemplate.Template, message.Amount, comment, message.shortUrl),
                        message.shortUrl);

                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Exception posting to wall for Facebook user {0}. {1}", message.Recipient.FacebookUser.FBUserID, ex.Message));
                }
            }
        }
        private void SendRecipientReceiptCommunication(Domain.Message message)
        {
            //Send SMS Message to recipient
            if (!String.IsNullOrEmpty(message.Recipient.MobileNumber))
            {
                _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Payment_Receipt_SMS");
                try
                {
                    if (communicationTemplate != null)
                    {
                        _smsServices.SendSMS(message.ApiKey, message.Recipient.MobileNumber,
                              String.Format(communicationTemplate.Template, _senderName, message.Amount, _mobileWebSiteUrl));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Exception sending sms to recipient {0}", message.Recipient.MobileNumber));
                }
            }
            //Send confirmation email to recipient
            if (!String.IsNullOrEmpty(message.Recipient.EmailAddress))
            {
                _logger.Log(LogLevel.Info, String.Format("Sending Email Confirmation to Recipient"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Payment_Receipt_Email");

                if (communicationTemplate != null)
                {
                    //Payment Registered Recipient
                    try
                    {
                        DateTime createDate = ConvertToLocalTime(message.CreateDate, "Eastern Standard Time");
                        
                        _emailServices.SendEmail(message.Recipient.EmailAddress, String.Format("{0} sent you {1:C} using PaidThx", _senderName, message.Amount),
                            communicationTemplate.Template, new List<KeyValuePair<string, string>>()
                                    {
                                        new KeyValuePair<string, string>("first_name", message.Recipient.FirstName),
                                        new KeyValuePair<string, string>("last_name", message.Recipient.LastName),
                                        new KeyValuePair<string, string>("rec_amount",  String.Format("{0:C}", message.Amount)),
                                        new KeyValuePair<string, string>("rec_sender", _senderName),
                                        new KeyValuePair<string, string>("rec_sender_photo_url", (message.Sender.ImageUrl != null ? message.Sender.ImageUrl : _defaultAvatarImage)),
                                        new KeyValuePair<string, string>("rec_datetime", String.Format("{0} at {1}", createDate.ToString("dddd, MMMM dd"), createDate.ToString("hh:mm tt"))),
                                        new KeyValuePair<string, string>("rec_comments", (!String.IsNullOrEmpty(message.Comments) ? message.Comments : "")),
                                        new KeyValuePair<string, string>("REC_COMMENTS_DISPLAY", (String.IsNullOrEmpty(message.Comments) ? "display: none" : "")),
                                        new KeyValuePair<string, string>("link_registration", message.shortUrl),
                                        new KeyValuePair<string, string>("app_user", "false")
                                    });
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Unhandled exception sending email to recipient {0}", ex.Message));
                    }
                }
            }
            if (message.Recipient.FacebookUser != null)
            {
                try
                {
                    var communicationTemplate = _communicationServices.GetCommunicationTemplate("Payment_Receipt_Facebook");
                    var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");

                    _facebookServices.MakeWallPost(message.Sender.FacebookUser.OAuthToken, message.Recipient.FacebookUser.FBUserID,
                        String.Format(communicationTemplate.Template, message.Amount, comment, message.shortUrl),
                        message.shortUrl);

                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Exception posting to wall for Facebook user {0}. {1}", message.Recipient.FacebookUser.FBUserID, ex.Message));
                }
            }
            
            /*Push Notification Stuff
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
                            auth_token, message.Recipient.UserId.ToString(), message.Recipient.RegistrationId, _senderName, message);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Exception Pushing Android Notification. {0}", ex.Message));
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Info, String.Format("Sending iOS Push Notification to Recipient"));
                    
                    try
                    {

                        var communicationTemplate = _communicationServices.GetCommunicationTemplate("Payment_NotRegistered_iOSPushNotification");

                        _iosNotificationServices.PushIOSNotification(communicationTemplate.Template, message, _senderName);

                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Exception Pushing IOS Notification. {0}", ex.Message));
                    }
                }

            }*/

            if (!String.IsNullOrEmpty(message.Recipient.DeviceToken))
            {
                // Submitted Payment:
                // iOS Notification Payload:
                // Device Token from User
                // -> Message: ?
                var sndrName = message.Sender.SenderName;
                var pushMsg = String.Format("{0} sent you {1:C}.", sndrName, message.Amount);
                // -> Alert#: ? (1 for now)
                var badgeNum = 1;
                _iosNotificationServices.SendPushNotification(message.Recipient.DeviceToken, pushMsg, badgeNum);
            }

            if (message.Recipient.FacebookUser != null)
            {
                //Send Facebook Message
                // I don't think we can do this through the server. Nice try though.
                // We should, however, publish something to the user's page that says sender sent payment

            }
        }
        private void SendRecipientNotRegisteredCommunication(Domain.Message message)
        {
            // var link = String.Format("{0}{1}", _mobileWebSiteUrl, message.Id.ToString());
            URIType recipientType = _messageServices.GetURIType(message.RecipientUri);

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

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Payment_NotRegistered_SMS");

                //Send out SMS message to recipient
                _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient (Recipient is not an registered user)."));

                var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");

                _smsServices.SendSMS(message.ApiKey, message.RecipientUri,
                    String.Format(communicationTemplate.Template, _senderName, message.Amount,
                    comment, message.shortUrl));
            }

            //Payment Registered Recipient
            if (recipientType == URIType.EmailAddress)
            {
                //Send confirmation email to recipient
                _logger.Log(LogLevel.Info, String.Format("Send Email to Recipient (Recipient is not an registered user)."));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Payment_NotRegistered_Email");
                DateTime createDate = ConvertToLocalTime(message.CreateDate, "Eastern Standard Time");
                        
                var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");
                var subject = String.Format("{0} sent you {1:C} using PaidThx{2}", _senderName, message.Amount, comment);

                _emailServices.SendEmail(message.RecipientUri, String.Format("{0} sent you {1:C} using PaidThx", _senderName, message.Amount),
                                 communicationTemplate.Template, new List<KeyValuePair<string, string>>()
                                    {
                                        new KeyValuePair<string, string>("REC_SENDER", _senderName),
                                        new KeyValuePair<string, string>("REC_AMOUNT", String.Format("{0:C}", message.Amount)),
                                        new KeyValuePair<string, string>("REC_SENDER_PHOTO_URL",  (message.Sender.ImageUrl != null ? message.Sender.ImageUrl : _defaultAvatarImage)),
                                        new KeyValuePair<string, string>("REC_DATETIME", String.Format("{0} at {1}", createDate.ToString("dddd, MMMM dd"), createDate.ToString("hh:mm tt"))),
                                        new KeyValuePair<string, string>("REC_COMMENTS", (!String.IsNullOrEmpty(message.Comments) ? message.Comments : "")),
                                        new KeyValuePair<string, string>("REC_COMMENTS_DISPLAY", (String.IsNullOrEmpty(message.Comments) ? "display: none" : "")),
                                        new KeyValuePair<string, string>("LINK_REGISTRATION", message.shortUrl),
                                    });
            }
            if (recipientType == URIType.FacebookAccount)
            {
                try
                {
                    var communicationTemplate = _communicationServices.GetCommunicationTemplate("Payment_NotRegistered_Facebook");

                    var client = new Facebook.FacebookClient(message.Sender.FacebookUser.OAuthToken);
                    var args = new Dictionary<string, object>();

                    var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");

                    _facebookServices.MakeWallPost(message.Sender.FacebookUser.OAuthToken, message.RecipientUri.Substring(3),
                        String.Format(communicationTemplate.Template, message.Amount, comment, message.shortUrl),
                        message.shortUrl);

                    client.Post(String.Format("/{0}/feed", message.Recipient.FacebookUser.FBUserID), args);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, ex.Message);
                }
            }
        }
        private static DateTime ConvertToLocalTime(DateTime utcDate, string timeZoneId)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime createDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, timeZoneInfo);

            if (timeZoneInfo.IsDaylightSavingTime(createDate))
                createDate = createDate.AddHours(-1);

            return createDate;
        }
    }
}
