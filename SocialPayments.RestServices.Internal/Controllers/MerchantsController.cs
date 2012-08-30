using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using SocialPayments.Domain;
using SocialPayments.DomainServices.CustomExceptions;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class MerchantsController : ApiController
    {
        // GET /api/merchants
        public HttpResponseMessage<List<MerchantModels.MerchantResponseModel>> Get(string type)
        {
            var merchantServices = new DomainServices.MerchantServices();
            List<Domain.Merchant> merchants = null;
            HttpResponseMessage<List<MerchantModels.MerchantResponseModel>> response = null;
            
            if (type != "NonProfits" && type != "Organizations")
            {
                return new HttpResponseMessage<List<MerchantModels.MerchantResponseModel>>(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Type specified in request must be NonProfits or Organizations"
                };
            }

            try
            {
                merchants = merchantServices.GetMerchants(type);
            }
            catch (SocialPayments.DomainServices.CustomExceptions.NotFoundException ex)
            {
                response = new HttpResponseMessage<List<MerchantModels.MerchantResponseModel>>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage<List<MerchantModels.MerchantResponseModel>>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

                return response;
            }


            var results = merchants.Select(m => new MerchantModels.MerchantResponseModel()
            {
                Id = m.UserId,
                Name = m.Name,
                MerchantImageUrl = (!String.IsNullOrEmpty(m.User.ImageUrl) ? m.User.ImageUrl : "http://images.paidthx.com/assets/contact-icon.gif"),
                PreferredReceiveAccountId = (m.User.PreferredReceiveAccount != null ? m.User.PreferredReceiveAccount.Id.ToString() : ""),
                PreferredSendAccountId = (m.User.PreferredSendAccount != null ? m.User.PreferredSendAccount.Id.ToString() : ""),
                Listings = m.MerchantListings.Select(l => new MerchantModels.MerchantListingResponse()
                {
                    Description = l.Description,
                    Id = l.Id,
                    TagLine = l.TagLine,
                    Offers = l.MerchantOffers.Select(o => new MerchantModels.MerchantOfferResponse()
                    {
                        Amount = o.Amount,
                        Id = o.Id
                    }).ToList()
                }).ToList()
            }).ToList();

            return new HttpResponseMessage<List<MerchantModels.MerchantResponseModel>>(results, HttpStatusCode.OK);
        }

        // GET /api/merchants/5
        public HttpResponseMessage<MerchantModels.MerchantDetailResponse> GetDetail(string id)
        {
            Domain.Merchant merchant;
            HttpResponseMessage<MerchantModels.MerchantDetailResponse> response;
            var merchantServices = new DomainServices.MerchantServices();

            try
            {
                merchant = merchantServices.GetMerchant(id);
                
                if (merchant == null)
                    throw new SocialPayments.DomainServices.CustomExceptions.NotFoundException(String.Format("Merchant {0} Not Found", id));
            }
            catch (SocialPayments.DomainServices.CustomExceptions.NotFoundException ex)
            {
                response = new HttpResponseMessage<MerchantModels.MerchantDetailResponse>(HttpStatusCode.NotFound);
                response.ReasonPhrase = ex.Message;

                return response;
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage<MerchantModels.MerchantDetailResponse>(HttpStatusCode.InternalServerError);
                response.ReasonPhrase = ex.Message;

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
