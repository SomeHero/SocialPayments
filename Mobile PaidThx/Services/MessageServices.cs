using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;

namespace Mobile_PaidThx.Services
{
    public class MessageServices: ServicesBase
    {
        private string _messageServicesBaseUrl = "{0}PaystreamMessages/{1}";

        public MessageModels.MessageResponse GetMessage(string messageId)
        {
            var serviceUrl = String.Format(_messageServicesBaseUrl, _webServicesBaseUrl, messageId);

            var response = Get(serviceUrl);

            JavaScriptSerializer js = new JavaScriptSerializer();

            var message = js.Deserialize<MessageModels.MessageResponse>(response.JsonResponse);

            return message;
        }
    }
}