using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using SocialPayments.DataLayer.Interfaces;

namespace SocialPayments.DomainServices
{
    public class SMSLogService
    {
        private IDbContext _ctx;

        public SMSLogService(IDbContext context)
        {
            _ctx = context;
        }

        public SMSLog AddSMSLog(Guid applicationId, string mobileNumber, string message, Domain.SMSStatus smsStatus, DateTime? sentDate)
        {
            var smsLog = _ctx.SMSLog.Add(new SMSLog()
            {
                Id = Guid.NewGuid(),
                ApiKey = applicationId,
                MobileNumber = mobileNumber,
                Message = message,
                SMSStatus = smsStatus,
                CreateDate = System.DateTime.Now,
                SentDate = sentDate
            });

            _ctx.SaveChanges();

            return smsLog;
        }
        public List<SMSLog> GetSMSLogs()
        {
            return _ctx.SMSLog.Select(s => s).ToList<SMSLog>();
        }
        public SMSLog GetSMSLog(Guid id)
        {
            return _ctx.SMSLog.FirstOrDefault(s => s.Id == id);
        }
        public void UpdateSMSLog(SMSLog smsLog)
        {
            _ctx.SaveChanges();
        }
    }
}
