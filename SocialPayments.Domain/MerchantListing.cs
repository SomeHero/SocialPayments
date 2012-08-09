using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class MerchantListing
    {
        public Guid Id { get; set; }
        public Guid MerchantId { get; set; }
        [ForeignKey("MerchantId")]
        public virtual Merchant Merchant { get; set; }
        public string TagLine { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }

        public virtual List<MerchantOffer> MerchantOffers { get; set; }
    }
}
