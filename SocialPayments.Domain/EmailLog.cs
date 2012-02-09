using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class EmailLog
    {
        public Guid Id { get; set; }
        public Guid ApiKey { get; set; }
        public string ToEmailAddress { get; set; }
        public string FromEmailAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int EmailStatusValue { get; set; }
        public EmailStatus EmailStatus
        {
            get { return (EmailStatus)EmailStatusValue; }
            set { EmailStatusValue = (int)value; }
        }
    }
}
