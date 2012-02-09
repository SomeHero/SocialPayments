using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using NLog;
using SocialPayments.DataLayer;
using SocialPayments.Services.DataContracts.SMS;

namespace SocialPayments.Services
{
    public class SMSService: ServiceContracts.ISMSService
    {
        DomainServices.SMSLogService smsLogService = new DomainServices.SMSLogService();
        DomainServices.ApplicationService applicationService = new DomainServices.ApplicationService();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Context _ctx = new Context();

        public DataContracts.SMS.SMSResponse SendSMS(DataContracts.SMS.SMSRequest smsRequest)
        {
            var application = applicationService.GetApplication(smsRequest.ApiKey);

            //Log Request
            var smsLog = smsLogService.AddSMSLog(smsRequest.ApiKey, smsRequest.MobileNumber, smsRequest.Message, Domain.SMSStatus.Pending, null);

            // SMSified API endpoint.
			string webTarget = "https://api.smsified.com/v1/smsmessaging/outbound/{0}/requests";

			// Parameters to send with API request.
			string webPost = "address={0}&message={1}";

			// SMSified credentials.
			string userName = "jrhodes621";
			string password = "james123";
            string senderNumber = "2892100266";

            //format to number
            string toMobileNumber = String.Format("{0}{1}", "1", smsRequest.MobileNumber.Replace(@"-", @""));

            var aliases = _ctx.MobileDeviceAliases
                .FirstOrDefault(m => m.MobileNumber == smsRequest.MobileNumber);

            if (aliases != null)
            {
                logger.Log(LogLevel.Info, String.Format("Alias found for {0}", smsRequest.MobileNumber));
                toMobileNumber = String.Format("{0}{1}", "1", aliases.MobileNumberAlias.Replace(@"-", @""));
            }

            logger.Log(LogLevel.Info, String.Format("Sending SMS Message to {0}", toMobileNumber));

			// Create new HTTP request.
			string url = String.Format(webTarget, senderNumber);
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			req.Method = "POST";
			req.ContentType =  "application/x-www-form-urlencoded";
            byte[] postData = Encoding.ASCII.GetBytes(String.Format(webPost, toMobileNumber, smsRequest.Message));
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
			if(res.StatusCode == HttpStatusCode.Created) {
                smsLogService.UpdateSMSLog(smsLog.Id, smsLog.MobileNumber, smsLog.Message, Domain.SMSStatus.Sent, System.DateTime.Now);
			}
			else
			{
                smsLogService.UpdateSMSLog(smsLog.Id, smsLog.MobileNumber, smsLog.Message, Domain.SMSStatus.Failed, null);
            }

            return new DataContracts.SMS.SMSResponse()
            {
                Application = new DataContracts.Application.ApplicationResponse()
                {
                    ApiKey = application.ApiKey.ToString(),
                    ApplicationName = application.ApplicationName,
                    IsActive =application.IsActive,
                    Url = url
                },
                CreateDate= smsLog.CreateDate,
                LastUpdateDate = smsLog.LastUpdatedDate,
                Message = smsLog.Message,
                MobileNumber = smsLog.MobileNumber,
                SentDate = smsLog.SentDate,
                SMSMessageId = smsLog.Id
            };
        }
        public void SendAuthenticationTokens(DataContracts.SMS.SMSAuthenticationTokenRequest request)
        {
            //Get Random  Tokens
            //Format Message
            //SendSMS();
        }
    }
}