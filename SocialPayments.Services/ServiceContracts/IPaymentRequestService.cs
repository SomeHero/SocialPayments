using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SocialPayments.Services.ServiceContracts
{
    [ServiceContract]
    public interface IPaymentRequestService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/PaymentRequests", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        DataContracts.PaymentRequest.PaymentRequestResponse AddPaymentRequest(DataContracts.PaymentRequest.PaymentRequestRequest paymentRequest);

    }
}