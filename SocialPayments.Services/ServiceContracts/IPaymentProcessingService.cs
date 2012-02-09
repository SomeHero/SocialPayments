using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using SocialPayments.Services.DataContracts;

namespace SocialPayments.Services.ServiceContracts
{
    [ServiceContract]
    public interface IPaymentProcessingService
    {
        [OperationContract]
        [WebInvoke(Method = "Post", UriTemplate = "/ProcessPayment", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        void ProcessPayment(SubmittedPaymentRequest submittedPaymentRequest);
    }
}