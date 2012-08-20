﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class PledgeModels
    {
        public class PledgeMoneyModel
        {
            public String RecipientId { get; set; }
            public String RecipientName { get; set; }
            public String RecipientUri { get; set; }
            public Double Amount { get; set; }
            public String Comments { get; set; }
            public String Pincode { get; set; }
        }
        public class AddCauseModel
        {
            public List<OrganizationModels.OrganizationModel> NonProfits { get; set; }
            public String RecipientId { get; set; }
            public String RecipientName { get; set; }
        }
        public class AddContactModel
        {
            public String RecipientUri { get; set; }
        }
        public class SelectAmountModel
        {
            public double Amount { get; set; }
        }
        public class PinSwipeModel
        {
            public string Pincode { get; set; }
        }
    }
}