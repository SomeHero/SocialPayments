using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Net;
using System.Web.Script.Serialization;

namespace Mobile_PaidThx.Services
{
    public class ApplicationServices: ServicesBase
    {
        private string _applicationServicesBaseUrl = "{0}applications/{1}";

        public ApplicationResponse GetApplication(string apiKey)
        {
            var response = Get(String.Format(_applicationServicesBaseUrl, _webServicesBaseUrl, apiKey));

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);

            var js = new JavaScriptSerializer();

            return js.Deserialize<ApplicationResponse>(response.JsonResponse);
        }
    }
}