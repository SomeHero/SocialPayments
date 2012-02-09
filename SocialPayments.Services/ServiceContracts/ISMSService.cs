using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SocialPayments.Services.ServiceContracts
{
    [ServiceContract]
    public interface ISMSService
    {
        [OperationContract]
        [WebInvoke(Method = "Post", UriTemplate = "/SendSMS", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        DataContracts.SMS.SMSResponse SendSMS(DataContracts.SMS.SMSRequest smsRequest);

        [OperationContract]
        [WebInvoke(Method = "Post", UriTemplate = "/SendAuthenticationTokens", BodyStyle = WebMessageBodyStyle.Bare,
            ResponseFormat = WebMessageFormat.Json)]
        void SendAuthenticationTokens(DataContracts.SMS.SMSAuthenticationTokenRequest request);
    }
}