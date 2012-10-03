using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class RoutingNumberModels
    {
        public class ValidateRoutingNumberRequest
        {
            public string RoutingNumber { get; set; }
        }
    }
}