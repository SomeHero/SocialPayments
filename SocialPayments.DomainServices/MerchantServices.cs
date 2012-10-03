using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using System.Data.Entity;

namespace SocialPayments.DomainServices
{
    public class MerchantServices
    {
        public List<Domain.Merchant> GetMerchants(string type)
        {

            using (var ctx = new Context())
            {
                MerchantType merchantType = MerchantType.NonProfit;
                if (type == "Organizations")
                    merchantType = MerchantType.Regular;

                var merchants = ctx.Merchants
                    .Include("User")
                    .Include("MerchantListings")
                    .Include("MerchantListings.MerchantOffers")
                    .Where(m => m.MerchantTypeValue.Equals((int)merchantType))
                    .ToList();

                return merchants;
            }
        }
        public Domain.Merchant GetMerchant(string id)
        {
            using (var ctx = new Context())
            {
                Domain.Merchant merchant = null; 
                Guid idGuid;

                Guid.TryParse(id, out idGuid);

                if (idGuid == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Merchant {0} Not Found", id));

                merchant = ctx.Merchants
                    .FirstOrDefault(m => m.UserId.Equals(idGuid));

                return merchant;

            }
        }
    }
}
