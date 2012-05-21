using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class MobileNumberSignUpKeySMSListenerModels
    {
        public class UpdateSignUpKeyRequest
        {
            public SMSMessageNotification inboundSMSMessageNotification { get; set; }
        }
        public class SMSMessageNotification
        {
            public InboundMessage inboundSMSMessage { get; set; }
        }
        public class InboundMessage
        {
            public DateTime dateTime { get; set; }
            public string destinationAddress { get; set; }
            public string message { get; set; }
            public string messageId { get; set; }
            public string senderAddress { get; set; }
        }
    }
}