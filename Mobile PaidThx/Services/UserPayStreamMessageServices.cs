using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;
using System.Collections;

namespace Mobile_PaidThx.Services
{
    public class UserPayStreamMessageServices: ServicesBase
    {

        private string _userPayStreamServiceGetUrl = "{0}/Users/{1}/PaystreamMessages";

        public List<MessageModels.MessageResponse> GetMessages(string userId)
        {
            var response = Get(String.Format(_userPayStreamServiceGetUrl, _webServicesBaseUrl, userId));

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(response.Description);

            JavaScriptSerializer js = new JavaScriptSerializer();

            var paystream = js.Deserialize<List<MessageModels.MessageResponse>>(response.JsonResponse);

            return paystream;
        }

    }
}