using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class SMSLog
    {
        public Guid Id { get; set; }
        public Guid ApiKey { get; set; }
        public string MobileNumber { get; set; }
        public string Message { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int SMSStatusValue { get; set; }
        public SMSStatus SMSStatus
        {
            get { return (SMSStatus)SMSStatusValue; }
            set { SMSStatusValue = (int)value; }
        }
    }
}
