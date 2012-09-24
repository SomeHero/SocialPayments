using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Net;
using System.Web.Script.Serialization;
using Mobile_PaidThx.Services.CustomExceptions;
using Mobile_PaidThx.Models;

namespace Mobile_PaidThx.Services
{
    public class ApplicationServices: ServicesBase
    {
        private string _applicationServicesBaseUrl = "{0}applications/{1}";

        public ApplicationResponse GetApplication(string apiKey)
        {
            var js = new JavaScriptSerializer();
            var response = Get(String.Format(_applicationServicesBaseUrl, _webServicesBaseUrl, apiKey));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            return js.Deserialize<ApplicationResponse>(response.JsonResponse);
        }
    }
}