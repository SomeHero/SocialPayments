using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DomainServices.Interfaces;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeAmazonNotificationService : IAmazonNotificationService
    {
        public bool WasCalled { get; set; }

        public void PushSNSNotification(string topicARN, string subject, string message)
        {
            WasCalled = true;
        }
    }
}
