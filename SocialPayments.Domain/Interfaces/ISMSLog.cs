using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface ISMSLog
    {
        Guid Id { get; set; }
        Guid ApiKey { get; set; }
        string MobileNumber { get; set; }
        string Message { get; set; }
        DateTime CreateDate { get; set; }
        DateTime? SentDate { get; set; }
        DateTime? LastUpdatedDate { get; set; }
        int SMSStatusValue { get; set; }
        SMSStatus SMSStatus { get; set; }
    }
}
