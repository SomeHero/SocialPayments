using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class SharingModels
    {
        public class SharingModel {
            public List<SharingSubject> SharingSubjects { get; set; }
        }
        public class SharingSubject {
            public String Description { get; set; }
            public List<SharingItem> SharingItems { get; set; }
        }
        public class SharingItem {
            public String UserConfigurationId { get; set; }
            public String Description { get; set; }
            public bool On { get; set; }
        }
    }
}