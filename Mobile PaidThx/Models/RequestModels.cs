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
            public Double Amount { get; set; }
            public String Comments { get; set; }
            public String Pincode { get; set; }
        }
        public class AddContactRequestModel
        {
            public SortedDictionary<string, List<FacebookModels.Friend>> SortedContacts { get; set; }
            public String RecipientUri { get; set; }
        }
        public class AmountToSendModel
        {
            public double Amount { get; set; }
        }
        public class PinSwipeModel
        {
            public string Pincode { get; set; }
        }
    }
}