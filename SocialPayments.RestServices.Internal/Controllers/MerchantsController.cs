using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class MerchantsController : ApiController
    {
        // GET /api/merchants
        public HttpResponseMessage<List<MerchantModels.MerchantResponseModel>> Get()
        {
            using (var ctx = new Context())
            {
                return new HttpResponseMessage<List<MerchantModels.MerchantResponseModel>>(ctx.Merchants.Select(m =>new MerchantModels.MerchantResponseModel()
                 {
                     Id = m.Id,
                     Name = m.Name,
                     MerchantImageUrl = "http://image.paidthx.com/assets/contact-icon.gif"
                 }).ToList(), HttpStatusCode.OK);
            }
        }

        // GET /api/merchants/5
        public HttpResponseMessage Get(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.NotImplemented);
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
