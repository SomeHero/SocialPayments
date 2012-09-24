using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.CustomExceptions;

namespace Mobile_PaidThx.Services
{
    public class MessageServices: ServicesBase
    {
        private string _messageServicesBaseUrl = "{0}PaystreamMessages/{1}";

        public MessageModels.MessageResponse GetMessage(string messageId)
        {
            var serviceUrl = String.Format(_messageServicesBaseUrl, _webServicesBaseUrl, messageId);
            var js = new JavaScriptSerializer();

            var response = Get(serviceUrl);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            return js.Deserialize<MessageModels.MessageResponse>(response.JsonResponse);
        }
    }
}