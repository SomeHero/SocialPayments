using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.CustomExceptions;

namespace Mobile_PaidThx.Services
{
    public class UserAttributeServices: ServicesBase
    {
        private string _updateUserConfigurationVariable = "{0}Users/{1}/attributes/{2}";

        public void UpdateConfigurationSetting(string userId, string key, string value)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                AttributeValue = value
            });

            var response = Put(String.Format(_updateUserConfigurationVariable, _webServicesBaseUrl, userId, key), json);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
    }
}