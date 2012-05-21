using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using SocialPayments.DataLayer;
using NLog;
using SocialPayments.DataLayer.Interfaces;

namespace SocialPayments.DomainServices
{
    public class SMSService
    {
        private ApplicationService _applicationService;
        private SMSLogService _smsLogService;
        private IDbContext _ctx;
        private Logger _logger;

        public SMSService(IDbContext context)
        {
            _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();
            _applicationService = new ApplicationService(_ctx);
            _smsLogService = new SMSLogService(_ctx);
        }
        public SMSService(IDbContext context, Logger logger)
        {
            _ctx = context;
            _logger = logger;
            _applicationService = new ApplicationService(_ctx);
            _smsLogService = new SMSLogService(_ctx);
        }
        public SMSService(ApplicationService applicationService, SMSLogService smsLogService, IDbContext context
            , Logger logger)
        {
            _applicationService = applicationService;
            _smsLogService = smsLogService;
            _ctx = context;
            _logger = logger;
        }
        public void SendSMS(Guid apiKey, string mobileNumber, string message)
        {
            var application = _applicationService.GetApplication(apiKey);

            //Log Request
            var smsLog = _smsLogService.AddSMSLog(apiKey, mobileNumber, message, Domain.SMSStatus.Pending, null);

            // SMSified API endpoint.
            string webTarget = "https://api.smsified.com/v1/smsmessaging/outbound/{0}/requests";

            // Parameters to send with API request.
            string webPost = "address={0}&message={1}";

            // SMSified credentials.
            string userName = "jrhodes621";
            string password = "james123";
            string senderNumber = "2892100266";

            //format to number
            string toMobileNumber = String.Format("{0}{1}", "1", mobileNumber.Replace(@"-", @""));

            //var aliases = _ctx.MobileDeviceAliases
            //    .FirstOrDefault(m => m.MobileNumber == mobileNumber);

            //if (aliases != null)
            //{
            //    _logger.Log(LogLevel.Info, String.Format("Alias found for {0}", mobileNumber));
            //    toMobileNumber = String.Format("{0}{1}", "1", aliases.MobileNumberAlias.Replace(@"-", @""));
            //}

            _logger.Log(LogLevel.Info, String.Format("Sending SMS Message to {0}", toMobileNumber));

            // Create new HTTP request.
            string url = String.Format(webTarget, senderNumber);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            byte[] postData = Encoding.ASCII.GetBytes(String.Format(webPost, toMobileNumber, message));
            req.ContentLength = postData.Length;

            // Set HTTP authorization header.
            string authInfo = userName + ":" + password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;

            // Send HTTP request.
            Stream PostStream = req.GetRequestStream();
            PostStream.Write(postData, 0, postData.Length);
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            // Evaluate result of HTTP request.
            if (res.StatusCode == HttpStatusCode.Created)
            {
                _smsLogService.UpdateSMSLog(smsLog.Id, smsLog.MobileNumber, smsLog.Message, Domain.SMSStatus.Sent, System.DateTime.Now);
            }
            else
            {
                _smsLogService.UpdateSMSLog(smsLog.Id, smsLog.MobileNumber, smsLog.Message, Domain.SMSStatus.Failed, null);
            }
        }
    }
}
