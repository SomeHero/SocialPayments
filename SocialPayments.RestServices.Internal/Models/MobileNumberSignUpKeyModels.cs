using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class MobileNumberSignUpKeyModels
    {
        public class GetKeyRequest {
            
        }
        public class GetKeyResponse
        {
            public string Key { get; set; }
        }

    }
}