using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.DataLayer.Interfaces;
using NLog;
using SocialPayments.Domain;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Web;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SocialPayments.DomainServices
{
    public class AndroidNotificationService
    {
        private IDbContext _ctx;
        private Logger _logger;
        private ApplicationService _applicationService;

        public AndroidNotificationService(IDbContext ctx, Logger logger, ApplicationService applicationService)
        {
            _ctx = ctx;
            _logger = logger;
            _applicationService = applicationService;
        }

        public AndroidNotificationService(IDbContext ctx, Logger logger)
        {
            _ctx = ctx;
            _logger = logger;
            _applicationService = new ApplicationService();
        }

        public AndroidNotificationService(IDbContext ctx)
        {
            _ctx = ctx;
            _logger = LogManager.GetCurrentClassLogger();
            _applicationService = new ApplicationService();
        }

        public static string getToken(String email, String password)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Email=").Append(email);
            builder.Append("&Passwd=").Append(password);
            builder.Append("&accountType=GOOGLE");
            builder.Append("&source=PaidThx-Android-1");
            builder.Append("&service=ac2dm");

            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            string url = @"https://www.google.com/accounts/ClientLogin";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;

            Stream PostStream = req.GetRequestStream();
            PostStream.Write(data, 0, data.Length);
            PostStream.Close();
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            String auth_key = null;
            using (Stream responseStream = res.GetResponseStream())
            {
                using (StreamReader readStream = new StreamReader(responseStream, Encoding.ASCII))
                {
                    String line = null;
                    
                    while ((line = readStream.ReadLine()) != null)
                    {
                        if (line.StartsWith("Auth="))
                        {
                            auth_key = line.Substring(5);
                            
                        }
                    }
                }
            }

            return auth_key;

            
        }

        public static int sendAndroidPushNotification(String auth_token, String userId, String registrationId, String senderName, Message message)
        {
            var log = LogManager.GetCurrentClassLogger();
            string paymentNotificationText = "{0} sent you {1:C} using PaidThx!";
            string requestNotificationText = "{0} requested {1:C} from you using PaidThx!";

            StringBuilder postDataBuilder = new StringBuilder();
            postDataBuilder.Append("registration_id").Append("=")
                    .Append(registrationId);
            postDataBuilder.Append("&").Append("collapse_key").Append("=")
                    .Append(userId);
            postDataBuilder.Append("&").Append("data.userId").Append("=").Append(userId);
            postDataBuilder.Append("&").Append("data.transactionId").Append("=").Append(message.Id.ToString());

            string text;

            if (message.MessageType == Domain.MessageType.Payment)
            {
                text = String.Format(paymentNotificationText, senderName, message.Amount);
                log.Log(LogLevel.Info, String.Format("Sending Android Push for Payment: {0}", text));
            }
            else if (message.MessageType == Domain.MessageType.PaymentRequest)
            {
                text = String.Format(requestNotificationText, senderName, message.Amount);
                log.Log(LogLevel.Info, String.Format("Sending Android Push for Payment Request: {0}", text));
            }
            else
            {
                return 0;
            }
            postDataBuilder.Append("&").Append("data.notificationString").Append("=").Append(text);
            byte[] postData = ASCIIEncoding.UTF8.GetBytes(postDataBuilder.ToString());
            SetCertPolicy();
            string url = @"https://android.apis.google.com/c2dm/send";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            req.ContentLength = postData.Length;
            req.Headers.Add("Authorization", "GoogleLogin auth=" + auth_token);

            Stream PostStream = req.GetRequestStream();
            PostStream.Write(postData, 0, postData.Length);
            PostStream.Close();
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            return (int)res.StatusCode;

        }

        public static void SetCertPolicy()
        {
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertValidate;
        }

        private static bool RemoteCertValidate(object sender, X509Certificate cert, X509Chain chain,
                SslPolicyErrors error)
        {
            // trust any cert!!!
            return true;
        }
    }
}
