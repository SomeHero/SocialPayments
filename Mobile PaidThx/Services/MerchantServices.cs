using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;

namespace Mobile_PaidThx.Services
{
    public class MerchantServices : ServicesBase
    {
        private string _merchantServicesBaseUrl = "{0}merchants?type={1}";

        public string GetMerchants(string type)
        {
            var response = Get(String.Format(_merchantServicesBaseUrl, _webServicesBaseUrl, type));

            return response.JsonResponse;
        }
    }
}