using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mobile_PaidThx.Services.ResponseModels
{
    public class MerchantModels
    {
        public class MerchantResponseModel
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string MerchantImageUrl { get; set; }
            public string PreferredReceiveAccountId { get; set; }
            public string PreferredSendAccountId { get; set; }
            public List<MerchantListingResponse> Listings { get; set; }
        }
        public class MerchantDetailResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string MerchantImageUrl { get; set; }
            public string PreferredReceiveAccountId { get; set; }
            public string PreferredSendAccountId { get; set; }
            public string MerchantTagLine { get; set; }
            public string MerchantDescription { get; set; }
            public decimal SuggestedAmount { get; set; }
        }
        public class MerchantListingResponse
        {
            public Guid Id { get; set; }
            public string TagLine { get; set; }
            public string Description { get; set; }
            public List<MerchantOfferResponse> Offers { get; set; }
        }
        public class MerchantOfferResponse
        {
            public Guid Id { get; set; }
            public double Amount { get; set; }
        }
    }
}