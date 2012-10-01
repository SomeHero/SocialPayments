using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DomainServices.Interfaces;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeEmailService: IEmailService
    {
        public bool WasCalled { get; set; }

        public bool SendEmail(string toEmailAddress, string emailSubject, string templateName, List<KeyValuePair<string, string>> replacementElements)
        {
            WasCalled = true;

            return true;
        }

        public bool SendEmail(Guid apiKey, string fromAddress, string toAddress, string subject, string body)
        {
            WasCalled = true;

            return true;
        }
    }
}
