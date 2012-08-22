using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;
using System.Net;

namespace Mobile_PaidThx.Services
{
    public class UserConfigurationServices : ServicesBase
    {
        private string _updateUserConfigurationVariable = "{0}Users/{1}/configurations/";
        
        public void UpdateConfigurationSetting(string userId, string key, string value)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            
            var json = js.Serialize(new
            {
                Key = key,
                Value = value
            });

            var response = Post(String.Format(_updateUserConfigurationVariable, _webServicesBaseUrl), json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);

        }
    }
}