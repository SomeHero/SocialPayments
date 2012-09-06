using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Net;

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
                throw new Exception(response.Description);

            return js.Deserialize<bool>(response.JsonResponse);

        }
    }
}