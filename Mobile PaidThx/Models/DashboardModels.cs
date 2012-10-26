using Mobile_PaidThx.Services.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class DashboardModels
    {
        public class DashboardModel
        {
            public double UserProfileComplete{ get; set; }
            public int UserNewActivity{ get; set; }
            public string UserName { get; set; }
            public string UserPic { get; set; }
            public PendingMessage PendingMessage { get; set; }
        }
        public class PendingMessage
        {
            public string MessageType { get; set; }
            public double Amount { get; set; }
            public string RecipientUriType { get; set; }
            public string RecipientUri { get; set; }
            public string CurrentUriType { get; set; }
            public string CurrentUserName { get; set; }
        }
    }
}