using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SocialPayments.Services.ServiceContracts
{
    [ServiceContract]
    public interface IPaymentAccountService
    {

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/PaymentAccounts", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        DataContracts.PaymentAccount.PaymentAccountReponse AddPaymentAccount(DataContracts.PaymentAccount.PaymentAccountRequest request);

        [OperationContract]
        [WebGet(UriTemplate = "/PaymentAccounts", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        List<DataContracts.PaymentAccount.PaymentAccountReponse> GetPaymentAccounts();

        [OperationContract]
        [WebGet(UriTemplate = "/PaymentAccounts/{id}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        DataContracts.PaymentAccount.PaymentAccountReponse GetPaymentAccount(string id);

        [OperationContract]
        [WebInvoke(Method = "Put", UriTemplate = "/PaymentAccounts", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void UpdatePayment(DataContracts.PaymentAccount.PaymentAccountRequest request);

        [OperationContract]
        [WebInvoke(Method = "Delete", UriTemplate = "/PaymentAccounts", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void DeletePayment(DataContracts.PaymentAccount.PaymentAccountRequest request);
    }
}