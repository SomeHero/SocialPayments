using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Models
{
    public class CompleteDonationModels
    {
        public class DonationRequestModel
        {
            public string DonationId { get; set; }
        }
        public class DonationResponseModel
        {
            public string SecurityPin { get; set; }
        }
        public class DonationConfirmationModel
        { }
    }
}