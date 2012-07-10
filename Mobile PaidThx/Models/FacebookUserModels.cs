using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class FacebookUserModels
    {
        public class FBuser
        {
            public string id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public FBLocation location { get; set; }
            public string email { get; set; }
            public string zipcode { get; set; }
        }
        public class FBLocation
        {
            public string name { get; set; }
        }
    }
}