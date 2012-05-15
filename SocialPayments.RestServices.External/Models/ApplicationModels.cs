using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.External.Models
{
    public class ApplicationModels
    {
        public class ApplicationResponse {
            public String id { get; set; }
            public String url { get; set; }
            public String apiKey { get; set; }
            public bool isActive { get; set; }
        }
        public class UpdateApplicationRequest
        {
            public String apiKey { get; set; }
            public String url { get; set; }
        }
    }
}