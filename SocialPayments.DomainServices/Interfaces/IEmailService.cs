using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.DomainServices.Interfaces
{
    public interface IEmailService
    {
        bool SendEmail(string toEmailAddress, string emailSubject, string templateName, List<KeyValuePair<string, string>> replacementElements);
        bool SendEmail(Guid apiKey, string fromAddress, string toAddress, string subject, string body);
    }
}
