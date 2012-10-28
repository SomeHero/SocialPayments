using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web.Script.Serialization;
using NLog;

namespace SocialPayments.Jobs.ProcessSettlementProcessor.Services
{
    public class TransactionServices : ServicesBase
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private string _transactionServicesBaseUrl = "{0}Transactions?withStatus={1}";
        private string _transactionServicesUpdateStatusBaseUrl = "{0}Transactions/{1}";
        public void UpdateTransactionStatusToComplete(Guid transactionId)
        {
            var js = new JavaScriptSerializer();
            var serviceUrl = String.Format(_transactionServicesUpdateStatusBaseUrl, _webServicesBaseUrl, transactionId);


            var json = js.Serialize(new
            {
                Status = "Complete"
            });
            var response = Post(serviceUrl, json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);

        }
        public List<ResponseModels.Transaction> GetTransactionsWithStatusSentToBank()
        {
            _logger.Log(LogLevel.Info, String.Format("Getting Transactions Sent To Bank"));

            var js = new JavaScriptSerializer();
            var serviceUrl = String.Format(_transactionServicesBaseUrl, _webServicesBaseUrl, "SentToBank");

            var response = Get(serviceUrl);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);


            return js.Deserialize<List<ResponseModels.Transaction>>(response.JsonResponse); 

        }
    }
}
