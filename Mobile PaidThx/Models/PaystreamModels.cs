using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Models
{
    public class PaystreamModels
    {
        public class PaystreamModel
        {
            public String UserId { get; set; }
            public ProfileModels ProfileModel { get; set; }
        }
        public class PinSwipeRequestModel
        {
            public string PaystreamAction { get; set; }
            public string MessageId { get; set; }
            public MessageModels.MessageResponse Message { get; set; }
        }
        public class PinSwipeModel
        {
            public string Pincode { get; set; }
        }
    }
}