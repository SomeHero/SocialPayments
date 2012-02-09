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
    public interface IPaymentService
    {

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/Payments", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        DataContracts.Payment.PaymentResponse AddPayment(DataContracts.Payment.PaymentRequest payment);

        [OperationContract]
        [WebGet(UriTemplate = "/Payments", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        List<DataContracts.Payment.PaymentResponse> GetPayments();

        [OperationContract]
        [WebGet(UriTemplate = "/Payments/{id}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        DataContracts.Payment.PaymentResponse GetPayment(string id);

        [OperationContract]
        [WebInvoke(Method = "Put", UriTemplate = "/Payments", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void UpdatePayment(DataContracts.Payment.PaymentRequest payment);

        [OperationContract]
        [WebInvoke(Method = "Delete", UriTemplate = "/Payments", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void DeletePayment(DataContracts.Payment.PaymentRequest payment);
    }
}