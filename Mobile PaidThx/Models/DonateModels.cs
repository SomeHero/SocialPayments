using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class DonateModels
    {
        public class DonateMoneyModel
        {
            public String RecipientId { get; set; }
            public String RecipientName { get; set; }
            public String RecipientImageUrl { get; set; }
            public Double Amount { get; set; }
            public String Comments { get; set; }
            public String Pincode { get; set; }
        }
        public class AddContactModel
        {
            public SortedDictionary<string, List<OrganizationModels.OrganizationModel>> SortedNonProfits { get; set; }
            public List<OrganizationModels.OrganizationModel> NonProfits { get; set; }
            public String RecipientId { get; set; }
            public String RecipientName { get; set; }
            public String RecipientImageUrl { get; set; } 
            public Double Amount { get; set; }
        }
        public class SelectAmountModel
        {
            public double Amount { get; set; }
        }
        public class PinSwipeModel
        {
            public string Pincode { get; set; }
            public String RecipientId { get; set; }
            public String RecipientName { get; set; }
            public String RecipientImageUrl { get; set; } 
            public Double Amount { get; set; }
        }
    }
}