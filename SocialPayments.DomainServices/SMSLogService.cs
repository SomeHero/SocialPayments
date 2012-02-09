using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class SMSLogService
    {
        private readonly Context _ctx = new Context();

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
        public SMSLog UpdateSMSLog(Guid id, string mobileNumber, string message, Domain.SMSStatus smsStatus, DateTime? sentDate)
        {
            var smsLog = GetSMSLog(id);

            smsLog.MobileNumber = mobileNumber;
            smsLog.Message = message;
            smsLog.SMSStatus = smsStatus;
            smsLog.SentDate = sentDate;
            smsLog.LastUpdatedDate = System.DateTime.Now;

            _ctx.SaveChanges();

            return smsLog;
        }
    }
}
