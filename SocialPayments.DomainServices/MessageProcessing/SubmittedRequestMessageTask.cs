using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using MoonAPNS;
using System.Configuration;

namespace SocialPayments.DomainServices.MessageProcessing
{
    public class SubmittedRequestMessageTask
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        
        private string _mobileWebSiteUrl = ConfigurationManager.AppSettings["MobileWebSetURL"];
        private string _defaultAvatarImage = ConfigurationManager.AppSettings["DefaultAvatarImage"].ToString();
        
        private string _recipientSMSMessage = "You received a PdThx request for {0:C} from {1}.";
        private string _recipientConfirmationEmailSubject = "You received a payment request for {0:C} from {1} using PaidThx.";
        private string _recipientConfirmationEmailBody = "You received a PdThx request for {0:C} from {1}.";
        private string _senderSMSMessage = "Your PdThx request for {0:C} to {1} was sent.";
        private string _senderConfirmationEmailSubject = "Confirmation of your PaidThx request to {0}.";
        private string _senderConfirmationEmailBody = "Your PaidThx request in the amount of {0:C} was delivered to {1}.";
        private string _recipientRequestNotification = "{0} requested {1:C} from you using PaidThx!";

        private string _fromEmailAddress = ConfigurationManager.AppSettings["FromEmailAddress"];

        public void Execute(Guid messageId)
        {
            using (var ctx = new Context())
            {
                _logger.Log(LogLevel.Info, String.Format("Processing Request Message to {0}", messageId));

                var messageService = new DomainServices.MessageServices(ctx);
                var userService = new DomainServices.UserService(ctx);
                var smsService = new DomainServices.SMSService(ctx);
                var emailService = new DomainServices.EmailService(ctx);
                var urlShortnerService = new DomainServices.GoogleURLShortener();
                var transactionBatchService = new DomainServices.TransactionBatchService(ctx, _logger);
                var communicationService = new DomainServices.CommunicationService(ctx);

                var message = ctx.Messages
                    .FirstOrDefault(m => m.Id == messageId);

                //Validate Payment Request
                //Batch Transacation WithDrawal

                URIType recipientType = messageService.GetURIType(message.RecipientUri);

                _logger.Log(LogLevel.Info, String.Format("URI Type {0}", recipientType));

                //Attempt to find the recipient of the request
                var sender = message.Sender;
                _logger.Log(LogLevel.Info, String.Format("Getting Recipient"));

                var recipient = userService.GetUser(message.RecipientUri);
                
                message.Recipient = recipient;
                message.shortUrl = urlShortnerService.ShortenURL(_mobileWebSiteUrl, message.Id.ToString());

                string smsMessage;
                string emailSubject;
                string emailBody;

                _logger.Log(LogLevel.Info, String.Format("Getting Sender"));

                var senderName = userService.GetSenderName(sender);
                _logger.Log(LogLevel.Info, String.Format("Retrieved Sender Name {0}", senderName));

                var recipientName = message.RecipientUri;

                if (recipient != null)
                {
                    _logger.Log(LogLevel.Debug, String.Format("Recipient Found"));

                    message.Status = PaystreamMessageStatus.PendingRequest;

                    //if the recipient has a mobile #; send SMS
                    if (!String.IsNullOrEmpty(recipient.MobileNumber))
                    {
                        _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient"));

                        var communicationTemplate = communicationService.GetCommunicationTemplate("Request_NotRegistered_SMS");

                        if (communicationTemplate != null)
                        {
                            smsMessage = String.Format(communicationTemplate.Template, message.Amount, senderName);
                            smsService.SendSMS(message.ApiKey, recipient.MobileNumber, smsMessage);
                        }
                    }

                    //if the recipient has an email address; send an email
                    if (!String.IsNullOrEmpty(recipient.EmailAddress))
                    {

                        _logger.Log(LogLevel.Info, String.Format("Send Email to Recipient - Not Registered"));

                        var communicationTemplate = communicationService.GetCommunicationTemplate("Request_NotRegistered_Email");

                        if (communicationTemplate != null)
                        {
                            emailSubject = String.Format("{0} requested {1:C} from you using PaidThx.", senderName, message.Amount);
                            
                            //REC_SENDER, 
                            //REC_AMOUNT, 
                            //REC_SENDER_PHOTO_URL, 
                            //REC_DATETIME, 
                            //REC_COMMENTS, 
                            //LINK_REGISTRATION
                            emailService.SendEmail(message.Recipient.EmailAddress, String.Format(_recipientConfirmationEmailSubject, senderName, message.Amount),
                                        communicationTemplate.Template, new List<KeyValuePair<string, string>>()
                                    {
                                        new KeyValuePair<string, string>("REC_SENDER", senderName),
                                        new KeyValuePair<string, string>("REC_AMOUNT", String.Format("{0:C}", message.Amount)),
                                        new KeyValuePair<string, string>("REC_SENDER_PHOTO_URL",  (message.Sender.ImageUrl != null ? message.Sender.ImageUrl : _defaultAvatarImage)),
                                        new KeyValuePair<string, string>("REC_DATETIME", String.Format("{0} at {1}",message.CreateDate.ToString("dddd, MMMM dd"), message.CreateDate.ToString("hh:mm tt"))),
                                        new KeyValuePair<string, string>("REC_COMMENTS", (!String.IsNullOrEmpty(message.Comments) ? message.Comments : "")),
                                        new KeyValuePair<string, string>("LINK_REGISTRATION", message.shortUrl),
                                    });
                            }

                    }
                    //if the recipient has a device token; send a push notification
                    if (!String.IsNullOrEmpty(recipient.DeviceToken))
                    {
                        if (!String.IsNullOrEmpty(recipient.RegistrationId))
                        {
                            _logger.Log(LogLevel.Info, String.Format("Sending Android Push Notification to Recipient"));
                            //Fix this.
                            try
                            {
                                string auth_token = AndroidNotificationService.getToken("android.paidthx@gmail.com", "pdthx123");
                                AndroidNotificationService.sendAndroidPushNotification(
                                    auth_token, recipient.UserId.ToString(), recipient.RegistrationId, senderName, message);
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
                            if (message.MessageType == Domain.MessageType.PaymentRequest)
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

                    }
                    //if the recipient has a linked facebook account; send a facebook message
                    if (recipient.FacebookUser != null)
                    {
                        _logger.Log(LogLevel.Info, String.Format("Send Facebook Message to Recipient"));

                    }

                }
                else
                {
                    message.Status = PaystreamMessageStatus.NotifiedRequest;

                    //if recipient Uri Type is Mobile Number, Send SMS
                    if (recipientType == URIType.MobileNumber)
                    {
                        _logger.Log(LogLevel.Info, String.Format("Send SMS Message to Recipient - Not Registered"));

                        var communicationTemplate = communicationService.GetCommunicationTemplate("Request_NotRegistered_SMS");

                        var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");

                        if (communicationTemplate != null)
                        {
                            smsMessage = String.Format(communicationTemplate.Template, senderName, message.Amount, comment, message.shortUrl);
                            smsService.SendSMS(message.ApiKey, message.RecipientUri, smsMessage);
                        }
                    }
                    //if recipient Uri Type is email address, Send Email
                    if (recipientType == URIType.EmailAddress)
                    {
                        _logger.Log(LogLevel.Info, String.Format("Send Email Message to Recipient - Not Registered"));

                        var communicationTemplate = communicationService.GetCommunicationTemplate("Request_NotRegistered_Email");

                        try {
                            if (communicationTemplate != null)
                            {
                                emailSubject = String.Format("{0} requested {1:C} from you using PaidThx.", senderName, message.Amount);

                                //REC_SENDER, 
                                //REC_AMOUNT, 
                                //REC_SENDER_PHOTO_URL, 
                                //REC_DATETIME, 
                                //REC_COMMENTS, 
                                //LINK_REGISTRATION
                                emailService.SendEmail(message.RecipientUri, String.Format(_recipientConfirmationEmailSubject, senderName, message.Amount),
                                            communicationTemplate.Template, new List<KeyValuePair<string, string>>()
                                    {
                                        new KeyValuePair<string, string>("REC_SENDER", senderName),
                                        new KeyValuePair<string, string>("REC_AMOUNT", String.Format("{0:C}", message.Amount)),
                                        new KeyValuePair<string, string>("REC_SENDER_PHOTO_URL",  (message.Sender.ImageUrl != null ? message.Sender.ImageUrl : _defaultAvatarImage)),
                                        new KeyValuePair<string, string>("REC_DATETIME", String.Format("{0} at {1}",message.CreateDate.ToString("dddd, MMMM dd"), message.CreateDate.ToString("hh:mm tt"))),
                                        new KeyValuePair<string, string>("REC_COMMENTS", (!String.IsNullOrEmpty(message.Comments) ? message.Comments : "")),
                                        new KeyValuePair<string, string>("LINK_REGISTRATION", message.shortUrl),
                                    });
                                }
                        }
                        catch(Exception ex)
                        {
                            _logger.Log(LogLevel.Error, String.Format("Unable to send Request Not Registered Email to Recipient {0}. Exception: {1}", message.RecipientUri, ex.Message));
                        }
                    }
                    if (recipientType == URIType.FacebookAccount)
                    {
                        var communicationTemplate = communicationService.GetCommunicationTemplate("Request_NotRegistered_Facebook");

                        try
                        {
                            var client = new Facebook.FacebookClient(sender.FacebookUser.OAuthToken);
                            var args = new Dictionary<string, object>();

                            var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");

                            args["message"] = String.Format(communicationTemplate.Template, message.Amount, comment, message.shortUrl);
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
                }

                ////if sender has mobile #, send confirmation email to sender
                //if (!String.IsNullOrEmpty(sender.MobileNumber))
                //{
                //    _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient"));

                //    smsMessage = String.Format(_senderSMSMessage, message.Amount, recipientName);
                //    smsService.SendSMS(message.ApiKey, sender.MobileNumber, smsMessage);

                //}

                ////if sender has email address; send an email
                //if (!String.IsNullOrEmpty(sender.EmailAddress))
                //{
                //    _logger.Log(LogLevel.Info, String.Format("Send Email to Recipient"));

                //    emailSubject = String.Format(_senderConfirmationEmailSubject, recipientName);
                //    emailBody = String.Format(_senderConfirmationEmailBody, message.Amount, recipientName);

                //    emailService.SendEmail(message.ApiKey, _fromEmailAddress, sender.EmailAddress, emailSubject, emailBody);

                //}

                ////if sender has a device token; send a push notification
                //if (!String.IsNullOrEmpty(sender.DeviceToken))
                //{
                //    _logger.Log(LogLevel.Info, String.Format("Send Push Notification to Recipient"));

                //}
                ////if sender has a linked facebook account; send a facebook message
                //if (sender.FacebookUser != null)
                //{
                //    _logger.Log(LogLevel.Info, String.Format("Send Facebook Message to Recipient"));
                //}
                

                //Update Payment Status
                _logger.Log(LogLevel.Info, String.Format("Updating Payment Request"));

                message.LastUpdatedDate = System.DateTime.Now;

                ctx.SaveChanges();
            }
        }
    }
}
