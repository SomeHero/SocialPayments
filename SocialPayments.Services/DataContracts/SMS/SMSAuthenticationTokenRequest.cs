using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SocialPayments.Services.DataContracts.SMS
{
    [DataContract]
    public class SMSAuthenticationTokenRequest
    {
        public Guid UserId { get; set; }
        public DateTime RequestDate { get; set; }
    }
}
