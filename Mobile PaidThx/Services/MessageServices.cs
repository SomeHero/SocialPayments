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
        private string _messageServicesBaseUrl = "http://23.21.203.171/api/internal/api/PaystreamMessages/{0}";

        public MessageModels.MessageResponse GetMessage(string messageId)
        {
            var serviceUrl = String.Format(_messageServicesBaseUrl, messageId);

            var response = Get(serviceUrl);

            JavaScriptSerializer js = new JavaScriptSerializer();

            var message = js.Deserialize<MessageModels.MessageResponse>(response.JsonResponse);

            return message;
        }
    }
}