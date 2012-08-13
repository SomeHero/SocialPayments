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

        private string _userPayStreamServiceGetUrl = "http://23.21.203.171/api/internal/api/Users/{0}/PaystreamMessages";

        public List<MessageModels.MessageResponse> GetMessages(string userId)
        {
            var jsonResponse = Get(String.Format(_userPayStreamServiceGetUrl, userId));

            JavaScriptSerializer js = new JavaScriptSerializer();

            var paystream = js.Deserialize<List<MessageModels.MessageResponse>>(jsonResponse);

            return paystream;
        }

    }
}