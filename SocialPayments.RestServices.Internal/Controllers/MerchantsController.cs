using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.Domain;
using SocialPayments.DomainServices.CustomExceptions;
using NLog;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class MerchantsController : ApiController
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        // GET /api/merchants
        [HttpGet]
        public HttpResponseMessage Get(string type)
        {
            var merchantServices = new DomainServices.MerchantServices();
            List<Domain.Merchant> merchants = null;
            HttpResponseMessage response = null;
            
            if (type != "NonProfits" && type != "Organizations")
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Type Must Be NonProfits or Organizations");

            try
            {
                merchants = merchantServices.GetMerchants(type);
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Merchants. Exception {0}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
 
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Merchants. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Merchants. Exception {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }



            var results = merchants.Select(m => new MerchantModels.MerchantResponseModel()
            {
                Id = m.UserId,
                Name = m.Name,
                MerchantImageUrl = (!String.IsNullOrEmpty(m.User.ImageUrl) ? m.User.ImageUrl : "http://images.paidthx.com/assets/contact-icon.gif"),
                PreferredReceiveAccountId = (m.User.PreferredReceiveAccountId != null ? m.User.PreferredReceiveAccountId.ToString() : ""),
                PreferredSendAccountId = (m.User.PreferredSendAccountId != null ? m.User.PreferredSendAccountId.ToString() : ""),
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

            return Request.CreateResponse<List<MerchantModels.MerchantResponseModel>>(HttpStatusCode.OK, results);
        }

        // GET /api/merchants/5
        [HttpGet]
        public HttpResponseMessage GetDetail(string id)
        {
            Domain.Merchant merchant = null;
            HttpResponseMessage response;
            var merchantServices = new DomainServices.MerchantServices();

            try
            {
                merchant = merchantServices.GetMerchant(id);
                
                if (merchant == null)
                    throw new SocialPayments.DomainServices.CustomExceptions.NotFoundException(String.Format("Merchant {0} Not Found", id));
            }
            catch (NotFoundException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Not Found Exception Getting Merchant Detail {0}. Exception {1}.", id, ex.Message));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.NotFound, error);
            }
            catch (BadRequestException ex)
            {
                _logger.Log(LogLevel.Warn, String.Format("Bad Request Exception Getting Merchant Detail {0}. Exception {1}.", id, ex.Message));

                var error = new HttpError(ex.Message);
                error["ErrorCode"] = ex.ErrorCode;

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Getting Merchant Detail {0}. Exception {1}. Stack Trace {2}", ex.Message, ex.StackTrace));

                var error = new HttpError(ex.Message);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, error);
            }


            return Request.CreateResponse<MerchantModels.MerchantDetailResponse>(HttpStatusCode.OK, new MerchantModels.MerchantDetailResponse()
            {
                Id = merchant.UserId,
                Name = merchant.Name,
                MerchantTagLine = "Non Profit Tag Line",
                MerchantDescription = "Description goes here",
                MerchantImageUrl = (!String.IsNullOrEmpty(merchant.User.ImageUrl) ? merchant.User.ImageUrl : "http://image.paidthx.com/assets/contact-icon.gif"),
                PreferredReceiveAccountId = (merchant.User.PreferredReceiveAccount != null ? merchant.User.PreferredReceiveAccount.Id.ToString() : ""),
                PreferredSendAccountId = (merchant.User.PreferredSendAccount != null ? merchant.User.PreferredSendAccount.Id.ToString() : ""),
                SuggestedAmount = 50
            });
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
