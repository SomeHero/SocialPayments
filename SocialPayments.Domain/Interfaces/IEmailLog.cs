using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface IEmailLog
    {
        Guid Id { get; set; }
        Guid ApiKey { get; set; }
        string ToEmailAddress { get; set; }
        string FromEmailAddress { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
        DateTime CreateDate { get; set; }
        DateTime? SentDate { get; set; }
        DateTime? LastUpdatedDate { get; set; }
        int EmailStatusValue { get; set; }
        EmailStatus EmailStatus { get; set; }
    }
}
