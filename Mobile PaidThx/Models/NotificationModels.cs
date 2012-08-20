using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

namespace Mobile_PaidThx.Models
{
    public class NotificationModels
    {
        public class NotificationModel {
            public List<NotificationSubject> NotificationSubjects { get; set; }
        }
        public class NotificationSubject {
            public String Description { get; set; }
            public List<NotificationItem> NotificationItems { get; set; }
        }
        public class NotificationItem {
            public String UserConfigurationId { get; set; }
            public String Description { get; set; }
            public bool On { get; set; }
            public String SelectedValue { get; set; }
            public List<KeyValuePair<string, string>> Options { get; set; }
        }
        public class NotificationOption
        {

        }
    }
}