using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using SocialPayments.DataLayer.Interfaces;
using System.Data.Entity;

namespace SocialPayments.DomainServices
{
    public class EmailLogService
    {
        private IDbContext _ctx;

        public EmailLogService()
        {
            _ctx = new Context();
        }
        public EmailLogService(IDbContext context)
        {
            _ctx = context;
        }
            
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
        public void UpdateEmailLog(EmailLog emailLog)
        {
            _ctx.SaveChanges();
        }
    }
}
