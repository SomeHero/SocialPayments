using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialPayments.RestServices.Internal.Models
{
    public class MerchantModels
    {
        public class MerchantResponseModel
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string MerchantImageUrl { get; set; }
        }
    }
}