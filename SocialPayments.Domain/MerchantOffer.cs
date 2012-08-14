using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class MerchantOffer
    {
        public Guid Id { get; set; }
        [ForeignKey("MerchantListingId")]
        public virtual MerchantListing MerchantListing { get; set; }
        public Guid MerchantListingId { get; set; }
        public double Amount { get; set; }
    }
}
