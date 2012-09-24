using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;
using System.Collections;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.CustomExceptions;

namespace Mobile_PaidThx.Services
{
    public class UserPayStreamMessageServices: ServicesBase
    {

        private string _userPayStreamServiceGetUrl = "{0}/Users/{1}/PaystreamMessages";
        private string _userPayStreamServicesBaseUrl = "{0}/Users/{1}/PaystreamMessages/{2}";

        public List<MessageModels.MessageResponse> GetMessages(string userId)
        {
            var response = Get(String.Format(_userPayStreamServiceGetUrl, _webServicesBaseUrl, userId));
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            var paystream = js.Deserialize<List<MessageModels.MessageResponse>>(response.JsonResponse);

            return paystream;
        }
        public MessageModels.MessageResponse GetMessage(string userId, string messageId)
        {
            var serviceUrl = String.Format(_userPayStreamServicesBaseUrl, _webServicesBaseUrl, userId, messageId);

            var response = Get(serviceUrl);
            var js = new JavaScriptSerializer();

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            return js.Deserialize<MessageModels.MessageResponse>(response.JsonResponse);
        }

    }
}