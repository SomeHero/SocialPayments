﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using MoonAPNS;
using System.Configuration;
using System.Data.Entity;

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

        private SocialNetwork _facebookSocialNetwork;
        private String _shortCode;


        public void Execute(Guid messageId)
        {
            using (var ctx = new Context())
            {
                try
                {
                    _logger.Log(LogLevel.Info, String.Format("Processing Request Message to {0}", messageId));

                    _messageService = new DomainServices.MessageServices();
                    _userServices = new DomainServices.UserService(ctx);
                    _smsServices = new DomainServices.SMSService(ctx);
                    _emailServices = new DomainServices.EmailService(ctx);
                    _communicationServices = new DomainServices.CommunicationService(ctx);
                    _facebookServices = new DomainServices.FacebookServices(ctx);
                    _iosNotificationServices = new DomainServices.IOSNotificationServices(ctx);

                    var urlShortnerService = new DomainServices.GoogleURLShortener();

                    var message = ctx.Messages
                        .FirstOrDefault(m => m.Id == messageId);
                    ctx.Messages.Attach(message);

                    _facebookSocialNetwork = ctx.SocialNetworks
                        .FirstOrDefault(u => u.Name == "Facebook");
                    _shortCode = urlShortnerService.GetShortCode("", message.Id.ToString());

                    //Validate Payment Request
                    bool isRecipientEngaged = true;

                    message.Recipient = _userServices.GetUser(message.RecipientUri);
                    message.shortUrl = String.Format("{0}i/{1}", _mobileWebSiteUrl, _shortCode);

                    if (message.Recipient != null)
                    {
                        if (message.Recipient.PaymentAccounts.Where(a => a.IsActive).Count() == 0)
                            isRecipientEngaged = false;

                        if (isRecipientEngaged)
                        {
                            _logger.Log(LogLevel.Info, String.Format("Sending Communication to Engaged Recipient {0} for Message {1}", message.Recipient.UserId, message.Id));

                            message.Status = PaystreamMessageStatus.PendingRequest;

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

                    SendSenderReceiptCommunication(message);

                    message.LastUpdatedDate = System.DateTime.Now;

                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Occurred Executing Post Payment Message Task. Exception: {0}. Stack Trace: {1}", ex.Message, ex.StackTrace));

                    var innerException = ex.InnerException;
                    while (innerException != null)
                    {
                        _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Occurred Executing Post Payment Message Task. Inner Exception: {0}. Stack Trace: {1}", innerException.Message, innerException.StackTrace));
                        innerException = innerException.InnerException;
                    }
                }
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

                        DateTime createDate = ConvertToLocalTime(message.CreateDate, "Eastern Standard Time");
                        
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
                            new KeyValuePair<string, string>("REC_DATETIME", String.Format("{0} at {1}", createDate.ToString("dddd, MMMM dd"), createDate.ToString("hh:mm tt"))),
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

            if (!String.IsNullOrEmpty(message.Recipient.DeviceToken))
            {
                // Submitted Request:
                // iOS Notification Payload:
                // Device Token from User
                // -> Message: ?
                var sndrName = message.Sender.SenderName;
                var pushMsg = String.Format("{0} requested {1:C} from you.", sndrName, message.Amount);
                // -> Alert#: ? (1 for now)
                var badgeNum = 1;
                _iosNotificationServices.SendPushNotification(message.Recipient.DeviceToken, pushMsg, badgeNum);
            }

            //if the recipient has a linked facebook account; send a facebook message
            if (message.Recipient.FacebookUser != null)
            {
                _logger.Log(LogLevel.Info, String.Format("Send Facebook Message to Recipient"));

                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_Receipt_Facebook");
                var facebookLink = String.Format("http://apps.facebook.com/paidthx/i/{0}", _shortCode);
          
                try
                {
                    var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");

                    _facebookServices.MakeWallPost(message.Sender.FacebookUser.OAuthToken, message.Recipient.FacebookUser.FBUserID,
                        String.Format(communicationTemplate.Template, message.Amount, comment, facebookLink),
                        facebookLink);
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

                DateTime createDate = ConvertToLocalTime(message.CreateDate, "Eastern Standard Time");
                        
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
                try
                {

                    _logger.Log(LogLevel.Info, String.Format("Make Facebook Wall Post to Recipient - Not Engaged {0}", message.Recipient.FacebookUser.FBUserID));

                    var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_NotEngaged_Facebook");
                    var facebookLink = String.Format("http://apps.facebook.com/paidthx/i/{0}", _shortCode);

                    try
                    {
                        var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");

                        _facebookServices.MakeWallPost(message.Sender.FacebookUser.OAuthToken, message.Recipient.FacebookUser.FBUserID,
                           String.Format(communicationTemplate.Template, message.Amount, comment, facebookLink),
                            facebookLink);
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
        private void    SendRecipientNotRegisteredCommunication(Domain.Message message)
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
                
                var communicationTemplate = _communicationServices.GetCommunicationTemplate("Request_NotRegistered_Email");

                _logger.Log(LogLevel.Info, String.Format("Sending Email Message to Recipient - Not Registered using Template {0}", communicationTemplate.Template));

                try
                {
                    if (communicationTemplate != null)
                    {
                        emailSubject = String.Format("{0} requested {1:C} from you using PaidThx.", senderName, message.Amount);

                        DateTime createDate = ConvertToLocalTime(message.CreateDate, "Eastern Standard Time");
                        
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
                                        new KeyValuePair<string, string>("REC_DATETIME", String.Format("{0} at {1}",createDate.ToString("dddd, MMMM dd"), createDate.ToString("hh:mm tt"))),
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
                var facebookLink = String.Format("http://apps.facebook.com/paidthx/i/{0}", _shortCode);

                try
                {
                    var comment = (!String.IsNullOrEmpty(message.Comments) ? String.Format(": \"{0}\"", message.Comments) : "");

                    _facebookServices.MakeWallPost(message.Sender.FacebookUser.OAuthToken, message.RecipientUri.Substring(3),
                        String.Format(communicationTemplate.Template, message.Amount, comment, facebookLink),
                        facebookLink);
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
