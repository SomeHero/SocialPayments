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
        private string _merchantServicesBaseUrl = "http://23.21.203.171/api/internal/api/merchants?type={0}";

        public string GetMerchants(string type)
        {
            var jsonResponse = Get(String.Format(_merchantServicesBaseUrl, type));

            return jsonResponse;
        }
    }
}