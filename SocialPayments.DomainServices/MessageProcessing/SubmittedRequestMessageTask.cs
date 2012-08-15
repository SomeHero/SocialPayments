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
        private string _mobileWebSiteEngageURl = ConfigurationManager.AppSettings["MobileWebSetURL"];
        private string _defaultAvatarImage = ConfigurationManager.AppSettings["DefaultAvatarImage"].ToString();
        
        private string _recipientConfirmationEmailSubject = "You received a payment request for {0:C} from {1} using PaidThx.";
        private string _recipientRequestNotification = "{0} requested {1:C} from you using PaidThx!";

        private DomainServices.MessageServices _messageService;
        private DomainServices.UserService _userServices;
        private DomainServices.EmailService _emailServices;
        private DomainServices.SMSService _smsServices;
        private DomainServices.CommunicationService _communicationServices;
        private DomainServices.FacebookServices _facebookServices;
        private DomainServices.IOSNotificationServices _iosNotificationServices;

        public void Execute(Guid messageId)
        {
            using (var ctx = new Context())
            {
                _logger.Log(LogLevel.Info, String.Format("Processing Request Message to {0}", messageId));

                _messageService  = new DomainServices.MessageServices(ctx);
                _userServices =  new DomainServices.UserService(ctx);
                _smsServices = new DomainServices.SMSService(ctx);
                _emailServices = new DomainServices.EmailService(ctx);
                _communicationServices = new DomainServices.CommunicationService(ctx);
                _facebookServices = new DomainServices.FacebookServices(ctx);
                _iosNotificationServices = new DomainServices.IOSNotificationServices(ctx);

                var urlShortnerService = new DomainServices.GoogleURLShortener();
        
                var message = ctx.Messages
                    .FirstOrDefault(m => m.Id == messageId);

                //Validate Payment Request
                bool isRecipientEngaged = true;

                message.Recipient = _userServices.GetUser(message.RecipientUri);
                message.shortUrl = urlShortnerService.ShortenURL(_mobileWebSiteUrl, message.Id.ToString());

                if (message.Recipient != null)
                {
                    if (message.Recipient.PaymentAccounts.Where(a => a.IsActive).Count() == 0)
                        isRecipientEngaged = false;

                    if (isRecipientEngaged)
                    {
                        _logger.Log(LogLevel.Info, String.Format("Sending Communication to Engaged Recipient {0} for Message {1}", message.Recipient.UserId, message.Id));

                        message.Status = PaystreamMessageStatus.ProcessingPayment;

                        SendRecipientEngagedCommunication(message);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Info, String.Format("Sending Communication to Non Engaged Recipient {0} for Message {1}", message.Recipient.UserId, message.Id));

                        message.Status = PaystreamMessageStatus.PendingRequest;

                        SendRecipientNotEngagedCommunication(message);
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Info, String.Format("Sending Communication to Not Registered Recipient {0} for Message {1}", message.RecipientUri, message.Id));

                    message.Status = PaystreamMessageStatus.NotifiedRequest;

                    SendRecipientNotRegisteredCommunication(message);
                }

                _logger.Log(LogLevel.Info, String.Format("Sending Communication to Sender {0} for Message {1}", message.Sender.UserId, message.Id));
                SendSenderReceiptCommunication(message);

                //Update Payment Status
                _logger.Log(LogLevel.Info, String.Format("Updating Payment Request"));

                message.LastUpdatedDate = System.DateTime.Now;

                ctx.SaveChanges();
            }
        }
        private void SendRecipientEngagedCommunication(Domain.Message message)
        {
            string smsMessage;
            string emailSubject;

            var senderName = _userServices.GetSenderName(message.Sender);

            //if the recipient has a mobile #; send SMS
            if (!String.IsNullOrEmpty(message.Recipient.MobileNumber))
            {
                _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_Receipt_SMS");
                var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");
                               
                try
                {
                    smsMessage = String.Format(communicationTemplate.Template, senderName, message.Amount, comment, _mobileWebSiteUrl);
                    _smsServices.SendSMS(message.ApiKey, message.Recipient.MobileNumber, smsMessage);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Exception sending SMS to recipient {0}. Exception: {1}", message.Recipient.MobileNumber, ex.Message));
                }
            }

            //if the recipient has an email address; send an email
            if (!String.IsNullOrEmpty(message.Recipient.EmailAddress))
            {

                _logger.Log(LogLevel.Info, String.Format("Send Email to Recipient - Not Registered"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_Receipt_Email");

                try
                {
                    if (communicationTemplate != null)
                    {
                        emailSubject = String.Format("{0} requested {1:C} from you using PaidThx.", senderName, message.Amount);

                        //REC_SENDER, 
                        //REC_AMOUNT, 
                        //REC_SENDER_PHOTO_URL, 
                        //REC_DATETIME, 
                        //REC_COMMENTS, 
                        //LINK_REGISTRATION
                        _emailServices.SendEmail(message.Recipient.EmailAddress, String.Format(_recipientConfirmationEmailSubject, message.Amount, senderName),
                                    communicationTemplate.Template, new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("REC_SENDER", senderName),
                            new KeyValuePair<string, string>("REC_AMOUNT", String.Format("{0:C}", message.Amount)),
                            new KeyValuePair<string, string>("REC_SENDER_PHOTO_URL",  (message.Sender.ImageUrl != null ? message.Sender.ImageUrl : _defaultAvatarImage)),
                            new KeyValuePair<string, string>("REC_DATETIME", String.Format("{0} at {1}",message.CreateDate.ToString("dddd, MMMM dd"), message.CreateDate.ToString("hh:mm tt"))),
                            new KeyValuePair<string, string>("REC_COMMENTS", (!String.IsNullOrEmpty(message.Comments) ? message.Comments : "")),
                            new KeyValuePair<string, string>("REC_COMMENTS_DISPLAY", (String.IsNullOrEmpty(message.Comments) ? "display: none" : "")),
                            new KeyValuePair<string, string>("LINK_REGISTRATION", message.shortUrl),
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Info, String.Format("Exception sending SMS to recipient {0}. Exception {1}", message.Recipient.MobileNumber, ex.Message));
                }

            }
            //if the recipient has a device token; send a push notification
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

                    _iosNotificationServices.PushIOSNotification(_recipientRequestNotification, message, senderName);
                }

            }
            //if the recipient has a linked facebook account; send a facebook message
            if (message.Recipient.FacebookUser != null)
            {
                _logger.Log(LogLevel.Info, String.Format("Send Facebook Message to Recipient"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_Receipt_Facebook");
                                
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
        private void SendRecipientNotEngagedCommunication(Domain.Message message)
        {
            var senderName = _userServices.GetSenderName(message.Sender);

            if (!String.IsNullOrEmpty(message.Recipient.MobileNumber))
            {
                _logger.Log(LogLevel.Info, String.Format("Send SMS to Recipient - Request Not Engaged"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_NotEngaged_SMS");

                try
                {
                    if (communicationTemplate != null)
                    {
                        _smsServices.SendSMS(message.ApiKey, message.Recipient.MobileNumber,
                              String.Format(communicationTemplate.Template, senderName, message.Amount, _mobileWebSiteUrl));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Exception sending sms to user {0}. {1}", message.Recipient.MobileNumber, ex.Message));
                }
            }
            if (!String.IsNullOrEmpty(message.Recipient.EmailAddress))
            {
                _logger.Log(LogLevel.Info, String.Format("Sending Email Confirmation to Recipient"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_NotEngaged_Email");

                try
                {
                    var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");
                    var subject = String.Format("{0} sent you {1:C} using PaidThx{2}", senderName, message.Amount, comment);

                    _emailServices.SendEmail(message.Recipient.EmailAddress, String.Format(_recipientConfirmationEmailSubject, senderName, message.Amount),
                                        communicationTemplate.Template, new List<KeyValuePair<string, string>>()
                                {
                                    new KeyValuePair<string, string>("REC_SENDER", senderName),
                                    new KeyValuePair<string, string>("REC_AMOUNT", String.Format("{0:C}", message.Amount)),
                                    new KeyValuePair<string, string>("REC_SENDER_PHOTO_URL",  (message.Sender.ImageUrl != null ? message.Sender.ImageUrl : _defaultAvatarImage)),
                                    new KeyValuePair<string, string>("REC_DATETIME", String.Format("{0} at {1}",message.CreateDate.ToString("dddd, MMMM dd"), message.CreateDate.ToString("hh:mm tt"))),
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
                try
                {

                    _logger.Log(LogLevel.Info, String.Format("Make Facebook Wall Post to Recipient - Not Engaged {0}", message.Recipient.FacebookUser.FBUserID));

                    var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_NotEngaged_Facebook");

                    try
                    {
                        var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");

                        _facebookServices.MakeWallPost(message.Sender.FacebookUser.OAuthToken, message.Recipient.FacebookUser.FBUserID,
                           String.Format(communicationTemplate.Template, message.Amount, comment, message.shortUrl),
                            message.shortUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, ex.Message);
                    }

                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Exception posting to wall for Facebook user {0}. {1}", message.Recipient.FacebookUser.FBUserID, ex.Message));
                }
            }
        }
        private void SendRecipientNotRegisteredCommunication(Domain.Message message)
        {
            string smsMessage;
            string emailSubject;

            var senderName = _userServices.GetSenderName(message.Sender);
            var recipientType = _userServices.GetURIType(message.RecipientUri);
  
            //if recipient Uri Type is Mobile Number, Send SMS
            if (recipientType == URIType.MobileNumber)
            {
                _logger.Log(LogLevel.Info, String.Format("Send SMS Message to Recipient - Not Registered"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_NotRegistered_SMS");

                var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");

                if (communicationTemplate != null)
                {
                    smsMessage = String.Format(communicationTemplate.Template, senderName, message.Amount, comment, message.shortUrl);
                    _smsServices.SendSMS(message.ApiKey, message.RecipientUri, smsMessage);
                }
            }
            //if recipient Uri Type is email address, Send Email
            if (recipientType == URIType.EmailAddress)
            {
                _logger.Log(LogLevel.Info, String.Format("Send Email Message to Recipient - Not Registered"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_NotRegistered_Email");

                try
                {
                    if (communicationTemplate != null)
                    {
                        emailSubject = String.Format("{0} requested {1:C} from you using PaidThx.", senderName, message.Amount);

                        //REC_SENDER, 
                        //REC_AMOUNT, 
                        //REC_SENDER_PHOTO_URL, 
                        //REC_DATETIME, 
                        //REC_COMMENTS, 
                        //LINK_REGISTRATION
                        _emailServices.SendEmail(message.RecipientUri, String.Format(_recipientConfirmationEmailSubject, message.Amount, senderName),
                                    communicationTemplate.Template, new List<KeyValuePair<string, string>>()
                                    {
                                        new KeyValuePair<string, string>("REC_SENDER", senderName),
                                        new KeyValuePair<string, string>("REC_AMOUNT", String.Format("{0:C}", message.Amount)),
                                        new KeyValuePair<string, string>("REC_SENDER_PHOTO_URL",  (message.Sender.ImageUrl != null ? message.Sender.ImageUrl : _defaultAvatarImage)),
                                        new KeyValuePair<string, string>("REC_DATETIME", String.Format("{0} at {1}",message.CreateDate.ToString("dddd, MMMM dd"), message.CreateDate.ToString("hh:mm tt"))),
                                        new KeyValuePair<string, string>("REC_COMMENTS", (!String.IsNullOrEmpty(message.Comments) ? message.Comments : "")),
                                        new KeyValuePair<string, string>("REC_COMMENTS_DISPLAY", (String.IsNullOrEmpty(message.Comments) ? "display: none" : "")),
                                        new KeyValuePair<string, string>("LINK_REGISTRATION", message.shortUrl),
                                    });
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unable to send Request Not Registered Email to Recipient {0}. Exception: {1}", message.RecipientUri, ex.Message));
                }
            }
            if (recipientType == URIType.FacebookAccount)
            {
                _logger.Log(LogLevel.Info, String.Format("Make Facebook Wall Post to Recipient - Not Registered"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_NotRegistered_Facebook");

                try
                {
                    var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");
 
                    _facebookServices.MakeWallPost(message.Sender.FacebookUser.OAuthToken, message.RecipientUri.Substring(3), 
                        String.Format(communicationTemplate.Template, message.Amount, comment, message.shortUrl),
                        message.shortUrl);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, ex.Message);
                }

            }
        }
        private void SendSenderReceiptCommunication(Domain.Message message)
        {
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

        }
    }
}
