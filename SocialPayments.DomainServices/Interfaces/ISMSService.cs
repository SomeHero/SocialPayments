using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer.Interfaces;
using NLog;

namespace SocialPayments.DomainServices.Interfaces
{
    public interface ISMSService
    {
        void SendSMS(Guid apiKey, string mobileNumber, string message);
    }
}
