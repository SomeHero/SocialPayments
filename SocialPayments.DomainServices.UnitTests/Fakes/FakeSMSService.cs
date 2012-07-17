using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DomainServices.Interfaces;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeSMSService: ISMSService
    {
        public bool WasCalled { get; set; }

        public void SendSMS(Guid apiKey, string mobileNumber, string message)
        {
            WasCalled = true;
        }
    }
}
