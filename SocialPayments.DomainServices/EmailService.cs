﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using System.Net.Mail;
using NLog;
using SocialPayments.DataLayer.Interfaces;
using System.Web;
using System.Net;
using System.IO;

namespace SocialPayments.DomainServices
{
    public class EmailService
    {
        private IDbContext _ctx;
        private Logger _logger;
        private ApplicationService _applicationService;
        private EmailLogService _emailLogService;

        public EmailService(IDbContext context)
        {
            _ctx = context;
            _logger = LogManager.GetCurrentClassLogger();
            _applicationService = new ApplicationService(_ctx);
            _emailLogService = new EmailLogService(_ctx);
        }
        public EmailService(IDbContext context, Logger logger)
        {
            _ctx = context;
            _logger = logger;
            _applicationService = new ApplicationService(_ctx);
            _emailLogService = new EmailLogService(_ctx);
        }
        public EmailService(IDbContext context, Logger logger, ApplicationService applicationService, EmailLogService emailLogService)
        {
            _ctx = context;
            _logger = logger;
            _applicationService = applicationService;
            _emailLogService = emailLogService;
        }
        public bool SendEmail(string toEmailAddress, string emailSubject, string templateName, List<KeyValuePair<string, string>> replacementElements)
        {
            string elasticEmailUrl = @"https://api.elasticemail.com/mailer/send";
            string elasticEmailUserName = "notify@paidthx.com";
            string elasticEmailApiKey = "20a00674-374b-4190-81ee-8fb96798a69c";
            string elasticEmailPost = "username={0}&api_key={1}&from={0}&from_name={0}&to={2}&subject={3}&template={4}{5}";

            StringBuilder mergeFields = new StringBuilder();
            foreach(var item in replacementElements) {
                mergeFields.Append("&");
                mergeFields.Append(String.Format("merge_{0}={1}", item.Key.ToUpper(), item.Value));
            }
            
            string requestBody = String.Format(elasticEmailPost, elasticEmailUserName, elasticEmailApiKey,
                toEmailAddress, emailSubject, HttpUtility.UrlEncode(templateName), mergeFields.ToString());

            // Create new HTTP request.
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(elasticEmailUrl);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            byte[] postData = Encoding.ASCII.GetBytes(requestBody);
            req.ContentLength = postData.Length;


            // Send HTTP request.
            Stream PostStream = req.GetRequestStream();
            PostStream.Write(postData, 0, postData.Length);
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            _logger.Log(LogLevel.Info, String.Format("{0} - {1}", res.StatusCode, res.StatusDescription));
       
            return true;
        }
        public bool SendEmail(Guid apiKey, string fromAddress, string toAddress, string subject, string body)
        {
            //Create Email Log
            var application = _applicationService.GetApplication(apiKey);

            var emailLog = _emailLogService.AddEmailLog(application.ApiKey, fromAddress, toAddress, subject, body, null);

            //Send Email
            SmtpClient sc = new SmtpClient();
            sc.EnableSsl = true;
            try
            {
                sc.Send(fromAddress, toAddress, subject, body);

                //Update Email Status
                _emailLogService.UpdateEmailLog(emailLog.ApiKey, emailLog.FromEmailAddress, emailLog.ToEmailAddress, emailLog.Subject, emailLog.Body,
                    Domain.EmailStatus.Sent, System.DateTime.Now);

                _logger.Log(LogLevel.Info, String.Format("Email Sent  to {0}.", toAddress));

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Sending Email to {0}. {1}.", toAddress, ex.Message));

                //Update Email Status
                _emailLogService.UpdateEmailLog(emailLog.Id, emailLog.FromEmailAddress, emailLog.ToEmailAddress, emailLog.Subject, emailLog.Body,
                   Domain.EmailStatus.Failed, null);

                return false;
            }

            return true;
        }

    }
}
