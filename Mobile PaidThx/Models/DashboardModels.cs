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
        }
    }
}