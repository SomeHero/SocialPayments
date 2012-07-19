using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class Merchant
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        [Key(), ForeignKey("User")]
        public Guid UserId { get; set; }
        public int MerchantTypeValue { get; set; }
        public MerchantType MerchantType 
        {
            get { return (MerchantType)MerchantTypeValue; }
            set { MerchantTypeValue =  (int)value; }
        }
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }

        public virtual User User { get; set; }
    }
}
