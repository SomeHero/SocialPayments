using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class EmailLogService
    {
        private readonly Context _ctx = new Context();
            
        public EmailLog AddEmailLog(Guid apiKey, string fromAddress, string toAddress, string subject, string body, DateTime? sentDate)
        {
            var emailLog= _ctx.EmailLog.Add(new EmailLog() {
                Id = Guid.NewGuid(),
                ApiKey = apiKey,
                Body = body,
                CreateDate = System.DateTime.Now,
                EmailStatus = EmailStatus.Pending,
                FromEmailAddress = fromAddress,
                Subject = subject,
                ToEmailAddress = toAddress,
                SentDate = sentDate
            });

            _ctx.SaveChanges();

            return emailLog;
        }
        public List<EmailLog> GetEmailLogs()
        {
            return _ctx.EmailLog.Select(e => e).ToList<EmailLog>();
        }
        public EmailLog GetEmailLog(Guid id)
        {
            return _ctx.EmailLog.FirstOrDefault(e => e.Id == id);
        }
        public EmailLog UpdateEmailLog(Guid id, string fromAddress, string toAddress, string subject, string body, EmailStatus emailStatus, DateTime? sentDate)
        {
            var emailLog = GetEmailLog(id);

            emailLog.FromEmailAddress = fromAddress;
            emailLog.ToEmailAddress = toAddress;
            emailLog.Body = body;
            emailLog.Subject = subject;
            emailLog.EmailStatus = emailStatus;
            emailLog.SentDate = sentDate;

            _ctx.SaveChanges();

            return emailLog;
        }
    }
}
