using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class PaystreamModels
    {
        public class PaystreamModel
        {
            public String UserId { get; set; }
            public ProfileModels ProfileModel { get; set; }
        }
        public class PinSwipeModel
        {
            public string Pincode { get; set; }
        }
    }
}