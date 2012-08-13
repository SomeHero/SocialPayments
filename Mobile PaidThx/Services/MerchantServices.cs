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
        private string _merchantServicesBaseUrl = "/api/merchants?type={0}";

        public string GetMessages(string type)
        {
            var jsonResponse = Get(String.Format(_merchantServicesBaseUrl, type));

            return jsonResponse;
        }
    }
}