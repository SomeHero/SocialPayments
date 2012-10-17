using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class UserPayStreamModels
    {
        public class SendReminderModel
        {
            public string MessageId { get; set; }
            public string ReminderMessage { get; set; }
        }
    }
}