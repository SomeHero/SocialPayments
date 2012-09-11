using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Models
{
    public class RequestModels
    {
        public class RequestMoneyModel
        {
            public String RecipientUri { get; set; }
            public String RecipientName { get; set; }
            public String RecipientImageUrl { get; set; } 
            public Double Amount { get; set; }
            public String Comments { get; set; }
            public String Pincode { get; set; }
        }
        public class AddContactRequestModel
        {
            public SortedDictionary<string, List<FacebookModels.Friend>> SortedContacts { get; set; }
            public String RecipientUri { get; set; }
            public String RecipientName { get; set; }
            public String RecipientImageUrl { get; set; }
        }
        public class AmountToSendModel
        {
            public double Amount { get; set; }
        }
        public class PinSwipeModel
        {
            public String RecipientUri { get; set; }
            public string Pincode { get; set; }
            public String RecipientName { get; set; }
            public String RecipientImageUrl { get; set; }
            public double Amount { get; set; }
        }
    }
}