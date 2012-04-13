using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Web;
using SocialPayments.Services.DataContracts.Transaction;

namespace SocialPayments.Services.ServiceContracts
{
    [ServiceContract]
    public interface ITransactionService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/Transactions/{id}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        List<DataContracts.Transaction.TransactionResponse> GetTransactions(String id);

    }
}