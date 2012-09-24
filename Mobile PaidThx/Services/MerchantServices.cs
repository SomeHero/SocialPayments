using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;
using System.Net;
using Mobile_PaidThx.Services.CustomExceptions;
using Mobile_PaidThx.Models;

namespace Mobile_PaidThx.Services
{
    public class MerchantServices : ServicesBase
    {
        private string _merchantServicesBaseUrl = "{0}merchants?type={1}";

        public List<MerchantModels.MerchantResponseModel> GetMerchants(string type)
        {
            var response = Get(String.Format(_merchantServicesBaseUrl, _webServicesBaseUrl, type));
            var js = new JavaScriptSerializer();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            return js.Deserialize<List<MerchantModels.MerchantResponseModel>>(response.JsonResponse);
        }
    }
}