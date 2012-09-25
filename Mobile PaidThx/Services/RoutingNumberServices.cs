using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Net;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.CustomExceptions;

namespace Mobile_PaidThx.Services
{
    public class RoutingNumberServices : ServicesBase
    {
        private string _routingNumberServiceUrl = "{0}routingnumber/validate";
        
        public bool ValidateRoutingNumber(string routingNumber)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                RoutingNumber = routingNumber
            });

            var serviceUrl = String.Format(_routingNumberServiceUrl, _webServicesBaseUrl);

            var response = Post(serviceUrl, json);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            return js.Deserialize<bool>(response.JsonResponse);

        }
    }
}