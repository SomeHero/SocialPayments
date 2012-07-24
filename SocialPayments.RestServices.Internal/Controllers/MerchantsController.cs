using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using SocialPayments.Domain;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class MerchantsController : ApiController
    {
        // GET /api/merchants
        public HttpResponseMessage<List<MerchantModels.MerchantResponseModel>> Get(string type)
        {
            if (type != "NonProfits" && type != "Organizations")
            {
                return new HttpResponseMessage<List<MerchantModels.MerchantResponseModel>>(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Type specified in request must be NonProfits or Organizations"
                };
            }

            using (var ctx = new Context())
            {
                MerchantType merchantType = MerchantType.NonProfit;
                if (type == "Organizations")
                    merchantType = MerchantType.Regular;

                var merchants = ctx.Merchants
                    .Where(m => m.MerchantTypeValue.Equals((int)merchantType))
                    .ToList();
                
                var results = merchants.Select(m =>new MerchantModels.MerchantResponseModel()
                 {
                     Id = m.UserId,
                     Name = m.Name,
                     MerchantImageUrl = (!String.IsNullOrEmpty(m.User.ImageUrl) ? m.User.ImageUrl : "http://image.paidthx.com/assets/contact-icon.gif"),
                     PreferredReceiveAccountId =(m.User.PreferredReceiveAccount != null ? m.User.PreferredReceiveAccount.Id.ToString() : ""),
                     PreferredSendAccountId = (m.User.PreferredSendAccount != null ? m.User.PreferredSendAccount.Id.ToString() : ""),
                 }).ToList();
                 
                 return new HttpResponseMessage<List<MerchantModels.MerchantResponseModel>>(results, HttpStatusCode.OK);
            }
        }

        // GET /api/merchants/5
        public HttpResponseMessage<MerchantModels.MerchantDetailResponse> GetDetail(string id)
        {
            using (var ctx = new Context())
            {
                Guid idGuid;

                Guid.TryParse(id, out idGuid);
                 
                if (idGuid == null)
                {
                    var responseMessage = String.Format("Invalid Id {0}", id);
                    var response = new HttpResponseMessage<MerchantModels.MerchantDetailResponse>(HttpStatusCode.NotFound);

                    response.ReasonPhrase = responseMessage;

                    return response;
                }
                var merchant = ctx.Merchants
                    .FirstOrDefault(m => m.UserId.Equals(idGuid));

                if (merchant == null)
                {
                    var responseMessage = String.Format("Invalid Id {0}", id);
                    var response = new HttpResponseMessage<MerchantModels.MerchantDetailResponse>(HttpStatusCode.NotFound);

                    response.ReasonPhrase = responseMessage;

                    return response;
                }

                return new HttpResponseMessage<MerchantModels.MerchantDetailResponse>(new MerchantModels.MerchantDetailResponse()
                {
                    Id = merchant.UserId,
                    Name = merchant.Name,
                    MerchantTagLine = "Non Profit Tag Line",
                    MerchantDescription = "Description goes here",
                    MerchantImageUrl = (!String.IsNullOrEmpty(merchant.User.ImageUrl) ? merchant.User.ImageUrl : "http://image.paidthx.com/assets/contact-icon.gif"),
                    PreferredReceiveAccountId = (merchant.User.PreferredReceiveAccount != null ? merchant.User.PreferredReceiveAccount.Id.ToString() : ""),
                    PreferredSendAccountId = (merchant.User.PreferredSendAccount != null ? merchant.User.PreferredSendAccount.Id.ToString() : ""),
                    SuggestedAmount = 50
                }, HttpStatusCode.OK);

            }
        }

        // POST /api/merchants
        public HttpResponseMessage Post(string value)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // PUT /api/merchants/5
        public HttpResponseMessage Put(int id, string value)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }

        // DELETE /api/merchants/5
        public HttpResponseMessage Delete(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
        }
    }
}
