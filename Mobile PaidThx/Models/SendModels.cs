using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Models
{
    public class SendModels 
    {
        public class SendMoneyModel
        {
            public String RecipientUri { get; set; }
            public Double Amount { get; set; }
            public String Comments { get; set; }
            public String Pincode { get; set; }
        }
        public class AddContactSendModel
        {
            public List<FacebookModels.Friend> Friends { get; set; }
            public String RecipientUri { get; set; }
        }
        public class AmountToSendModel
        {
            public double Amount { get; set; }
        }
        public class PinSwipModel
        {
            public string Pincode { get; set; }
        }

    }
}
