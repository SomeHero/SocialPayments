using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SocialPayments.Services.ServiceContracts
{
    [ServiceContract]
    public interface IEmailService
    {
        [OperationContract]
        [WebInvoke(Method = "Post", UriTemplate = "/SendEmail", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        DataContracts.Email.EmailResponse SendEmail(DataContracts.Email.EmailRequest emailRequest);
    }
}