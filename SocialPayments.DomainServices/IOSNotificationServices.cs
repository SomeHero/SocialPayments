using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DataLayer;
using NLog;
using MoonAPNS;

namespace SocialPayments.DomainServices
{
    public class IOSNotificationServices
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private IDbContext _ctx;

        private string _iOSP12FileName = @"C:\APNS\DevKey\aps_developer_identity.p12";
        private string _iOSP12FileNamePassword = "KKreap1566";

        public IOSNotificationServices() : this(new Context())
        {}

        public IOSNotificationServices(IDbContext context)
        {
            _ctx = context;
        }

        public void PushIOSNotification(string template, Domain.Message message, string senderName)
        {
            // We need to know the number of pending requests that the user must take action on for the application badge #
            // The badge number is the number of PaymentRequests in the Messages database with the Status of (1 - Pending)
            //      If we are processing a payment, we simply add 1 to the number in this list. This will allow the user to
            //      Be notified of money received, but it will not stick on the application until the users looks at it. Simplyt
            //      Opening the application is sufficient
            var numPending = _ctx.Messages.Where(p => p.MessageTypeValue
                .Equals((int)Domain.MessageType.PaymentRequest) && p.StatusValue.Equals((int)Domain.PaystreamMessageStatus.NotifiedRequest));

            NotificationPayload payload = null;
            String notification;

            // Send a mobile push notification
            if (message.MessageType == Domain.MessageType.PaymentRequest)
            {
                notification = String.Format(template, senderName, message.Amount);
                payload = new NotificationPayload(message.Recipient.DeviceToken, notification, numPending.Count());

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
                var push = new PushNotification(true, _iOSP12FileName, _iOSP12FileNamePassword);
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
}
