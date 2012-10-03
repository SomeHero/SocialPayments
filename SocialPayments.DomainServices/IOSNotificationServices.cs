using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer.Interfaces;
using SocialPayments.DataLayer;
using NLog;
using MoonAPNS;
using PushSharp;
using PushSharp.Common;
using PushSharp.Apple;
using PushSharp.Android;
using PushSharp.WindowsPhone;
using PushSharp.Windows;
using SocialPayments.Domain;
using System.IO;

namespace SocialPayments.DomainServices
{
    public class IOSNotificationServices
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private IDbContext _ctx;

        PushService pushServ = new PushService();

        public IOSNotificationServices(IDbContext ctx)
        {
            pushServ.Events.OnDeviceSubscriptionExpired += new PushSharp.Common.ChannelEvents.DeviceSubscriptionExpired(Events_OnDeviceSubscriptionExpired);
            pushServ.Events.OnDeviceSubscriptionIdChanged += new PushSharp.Common.ChannelEvents.DeviceSubscriptionIdChanged(Events_OnDeviceSubscriptionIdChanged);
            pushServ.Events.OnChannelException += new PushSharp.Common.ChannelEvents.ChannelExceptionDelegate(Events_OnChannelException);
            pushServ.Events.OnNotificationSendFailure += new PushSharp.Common.ChannelEvents.NotificationSendFailureDelegate(Events_OnNotificationSendFailure);
            pushServ.Events.OnNotificationSent += new PushSharp.Common.ChannelEvents.NotificationSentDelegate(Events_OnNotificationSent);
            pushServ.Events.OnChannelCreated += new PushSharp.Common.ChannelEvents.ChannelCreatedDelegate(Events_OnChannelCreated);
            pushServ.Events.OnChannelDestroyed += new PushSharp.Common.ChannelEvents.ChannelDestroyedDelegate(Events_OnChannelDestroyed);
        }

        public void SendPushNotification ( string deviceToken, string message, int badgeNum )
        {
            //Configure and start Apple APNS
            // IMPORTANT: Make sure you use the right Push certificate.  Apple allows you to generate one for connecting to Sandbox,
            //   and one for connecting to Production.  You must use the right one, to match the provisioning profile you build your
            //   app with!
            var appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Resources/PdThx.DevPushCert.p12"));

            //IMPORTANT: If you are using a Development provisioning Profile, you must use the Sandbox push notification server 
            //  (so you would leave the first arg in the ctor of ApplePushChannelSettings as 'false')
            //  If you are using an AdHoc or AppStore provisioning profile, you must use the Production push notification server
            //  (so you would change the first arg in the ctor of ApplePushChannelSettings to 'true')
            pushServ.StartApplePushService(new ApplePushChannelSettings(false, appleCert, "P@!DthxPUSH"));

            //Fluent construction of an iOS notification
            //IMPORTANT: For iOS you MUST MUST MUST use your own DeviceToken here that gets generated within your iOS app itself when the Application Delegate
            //  for registered for remote notifications is called, and the device token is passed back to you
            pushServ.QueueNotification(NotificationFactory.Apple()
                .ForDeviceToken(deviceToken)
                .WithAlert(message)
                .WithSound("default")
                .WithBadge(badgeNum));

            pushServ.StopAllServices();
        }

        static void Events_OnDeviceSubscriptionIdChanged(PushSharp.Common.PlatformType platform, string oldDeviceInfo, string newDeviceInfo, PushSharp.Common.Notification notification)
        {
            //Currently this event will only ever happen for Android GCM
            _logger.Log(LogLevel.Error,"Device Registration Changed:  Old-> " + oldDeviceInfo + "  New-> " + newDeviceInfo);
        }

        static void Events_OnNotificationSent(PushSharp.Common.Notification notification)
        {
            _logger.Log(LogLevel.Error, "Sent push notification: " + notification.Platform.ToString() + " -> " + notification.ToString());
        }

        static void Events_OnNotificationSendFailure(PushSharp.Common.Notification notification, Exception notificationFailureException)
        {
            _logger.Log(LogLevel.Error, "Failure sending push notification: " + notification.Platform.ToString() + " -> " + notificationFailureException.Message + " -> " + notification.ToString());
        }

        static void Events_OnChannelException(Exception exception, PushSharp.Common.PlatformType platformType, PushSharp.Common.Notification notification)
        {
            _logger.Log(LogLevel.Error, "Channel Exception: " + platformType.ToString() + " -> " + exception.ToString());
        }

        static void Events_OnDeviceSubscriptionExpired(PushSharp.Common.PlatformType platform, string deviceInfo, PushSharp.Common.Notification notification)
        {
            _logger.Log(LogLevel.Error, "Device Subscription Expired: " + platform.ToString() + " -> " + deviceInfo);
        }

        static void Events_OnChannelDestroyed(PushSharp.Common.PlatformType platformType, int newChannelCount)
        {
            _logger.Log(LogLevel.Info, "Channel Destroyed for: " + platformType.ToString() + " Channel Count: " + newChannelCount);
        }

        static void Events_OnChannelCreated(PushSharp.Common.PlatformType platformType, int newChannelCount)
        {
            _logger.Log(LogLevel.Info, "Channel Created for: " + platformType.ToString() + " Channel Count: " + newChannelCount);
        }
    }
}
