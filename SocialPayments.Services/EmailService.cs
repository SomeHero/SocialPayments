using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SocialPayments.Services.ServiceContracts;
using System.Net.Mail;

namespace SocialPayments.Services
{
    public class EmailService: IEmailService
    {
        DomainServices.EmailLogService emailLogService = new DomainServices.EmailLogService();
        DomainServices.ApplicationService applicationService = new DomainServices.ApplicationService();

        public DataContracts.Email.EmailResponse SendEmail(DataContracts.Email.EmailRequest emailRequest)
        {
            //Create Email Log
            var application = applicationService.GetApplication(emailRequest.ApiKey);

            var emailLog = emailLogService.AddEmailLog(application.ApiKey, emailRequest.FromAddress, emailRequest.ToAddress, emailRequest.Subject, emailRequest.Body, null);

            //Send Email
            SmtpClient sc = new SmtpClient();
            sc.EnableSsl = true;
            try
            {
                sc.Send(emailRequest.FromAddress, emailRequest.ToAddress, emailRequest.Subject, emailRequest.Body);
                //Update Email Status
                emailLogService.UpdateEmailLog(emailLog.ApiKey, emailLog.FromEmailAddress, emailLog.ToEmailAddress, emailLog.Subject, emailLog.Body,
                    Domain.EmailStatus.Sent, System.DateTime.Now);
            }
            catch (Exception ex)
            {
                //Update Email Status
                emailLogService.UpdateEmailLog(emailLog.Id, emailLog.FromEmailAddress, emailLog.ToEmailAddress, emailLog.Subject, emailLog.Body,
                   Domain.EmailStatus.Failed, null);
            }

            return new DataContracts.Email.EmailResponse()
            {
                Application = new DataContracts.Application.ApplicationResponse()
                {

                },
                Body = emailLog.Body,
                CreateDate = emailLog.CreateDate,
                FromAddress = emailLog.FromEmailAddress,
                EmailLogId = emailLog.Id,
                LastUpdatedDate = emailLog.LastUpdatedDate,
                SentDate = emailLog.SentDate,
                Subject = emailLog.Subject,
                ToAddress = emailLog.ToEmailAddress
            };
        }
    }
}