using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class FBMessageModel
    {
        public class ShareRequest
        {
            public string ApiKey { get; set; }
            public string TransactionId { get; set; }
            public string WallMessage { get; set; }
        }
        public class ReminderRequest
        {
            public string ApiKey { get; set; }
            public string TransactionId { get; set; }
            public string Message { get; set; }
        }
    }
}