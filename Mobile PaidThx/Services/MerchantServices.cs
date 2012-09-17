using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;
using System.Net;

namespace Mobile_PaidThx.Services
{
    public class MerchantServices : ServicesBase
    {
        private string _merchantServicesBaseUrl = "{0}merchants?type={1}";

        public List<MerchantModels.MerchantResponseModel> GetMerchants(string type)
        {
            var response = Get(String.Format(_merchantServicesBaseUrl, _webServicesBaseUrl, type));

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);

            var js = new JavaScriptSerializer();

            return js.Deserialize<List<MerchantModels.MerchantResponseModel>>(response.JsonResponse);
        }
    }
}