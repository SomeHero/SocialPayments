using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.ThirdPartyServices.FedACHService;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class RoutingNumberController : ApiController
    {
        //api/routingnumber/validate
        public HttpResponseMessage<bool> ValidateRoutingNumber(RoutingNumberModels.ValidateRoutingNumberRequest request)
        {
            FedACHService fedACHService = new FedACHService();

            FedACHList fedACHList = new FedACHList();
            bool results = fedACHService.getACHByRoutingNumber(request.RoutingNumber, out fedACHList);

            return new HttpResponseMessage<bool>(results);
        }
    }
}
