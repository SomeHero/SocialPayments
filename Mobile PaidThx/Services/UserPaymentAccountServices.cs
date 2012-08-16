using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Services
{
    //    public string ApiKey { get; set; }
    //public string Nickname { get; set; }
    //public string NameOnAccount { get; set; }
    //public string RoutingNumber { get; set; }
    //public string AccountNumber { get; set; }
    //public string AccountType { get; set; }
    //public string SecurityPin { get; set; }
    //public int SecurityQuestionID { get; set; }
    //public string SecurityQuestionAnswer { get; set; }
    public class UserPaymentAccountServices : ServicesBase
    {
        private string _setupACHAccountServiceUrl = "{0}Users/{1}/PaymentAccounts";
        private string _setPreferredReceiveAccountUrl = "{0}Users/{1}/PaymentAccounts/set_preferred_received_account";
        private string _setPreferredSendAccountUrl = "{0}Users/{1}/PaymentAccounts/set_preferred_send_account";
        private string _editACHAccountUrl = "{0}Users/{1}/PaymentAccounts/{2}";
        private string _deleteACHAccountUrl = "{0}Users/{1}/PaymentAccounts/{2}";

        public ServiceResponse DeleteAccount(String apiKey, String userId, String bankId)
        {
            string serviceUrl = String.Format(_deleteACHAccountUrl, _webServicesBaseUrl, userId, bankId);
            var response = Delete(serviceUrl);
            return response;
        }
        
        public string SetSendAccount(String apiKey, String userId, String bankId, String securityPin)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                ApiKey = apiKey,
                PaymentAccountId = bankId,
                SecurityPin = securityPin
            });

            string serviceUrl = String.Format(_setPreferredSendAccountUrl, _webServicesBaseUrl, userId);
            var response = Post(serviceUrl, json);

            var jsonObject = js.Deserialize<Dictionary<string, dynamic>>(response.JsonResponse);

            return jsonObject["paymentAccountId"];
        }

        public string SetReceiveAccount(String apiKey, String userId, String bankId, String securityPin)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                ApiKey = apiKey,
                PaymentAccountId = bankId,
                SecurityPin = securityPin
            });

            string serviceUrl = String.Format(_setPreferredReceiveAccountUrl, _webServicesBaseUrl, userId);
            var response = Post(serviceUrl, json);

            var jsonObject = js.Deserialize<Dictionary<string, dynamic>>(response.JsonResponse);

            return jsonObject["paymentAccountId"];
        }

        public string EditAccount(String apiKey, String userId, String bankId, String nickname, String nameOnAccount, String routingNumber, String accountType)
        {
            string editUrl = String.Format(_setupACHAccountServiceUrl, _webServicesBaseUrl, userId,  bankId);
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                ApiKey = apiKey,
                Nickname = nickname,
                NameOnAccount = nameOnAccount,
                RoutingNumber = routingNumber,
                AccountType = accountType
            });

            var response = Put(editUrl, json);

            var jsonObject = js.Deserialize<Dictionary<string, dynamic>>(response.JsonResponse);

            return jsonObject["paymentAccountId"];
        }

        public string AddAccount(String apiKey, String id, String nickName, String nameOnAccount, String routingNumber, string accountNumber, string accountType, string securityPin, string securityQuestionId, string securityQuestionAnswer)
        {
            var serviceUrl = String.Format(_setupACHAccountServiceUrl, id);
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                ApiKey = apiKey,
                NickName = nickName,
                NameOnAccount = nameOnAccount,
                RoutingNumber = routingNumber,
                AccountNumber = accountNumber,
                AccountType = accountType,
                SecurityPin = securityPin,
                securityQuestionId = securityQuestionId,
                SecurityQuestionAnswer = securityQuestionAnswer
            });

            var response = Post(serviceUrl, json);

            var jsonObject = js.Deserialize<Dictionary<string, dynamic>>(response.JsonResponse);

            return jsonObject["paymentAccountId"];
        }

        public string SetupACHAccount(string userId, string apiKey, string nameOnAccount, string nickName, string routingNumber, string accountNumber,
            string accountType, string securityPin, int securityQuestionId, string securityQuestionAnswer)
        {

            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                ApiKey = apiKey,
                NickName = nickName,
                NameOnAccount = nameOnAccount,
                RoutingNumber = routingNumber,
                AccountNumber = accountNumber,
                AccountType = accountType,
                SecurityPin = securityPin,
                securityQuestionId = securityQuestionId,
                SecurityQuestionAnswer = securityQuestionAnswer
            });

            var response = Post(String.Format(_setupACHAccountServiceUrl, _webServicesBaseUrl, userId), json);

            var jsonObject = js.Deserialize<Dictionary<string, dynamic>>(response.JsonResponse);

            return jsonObject["paymentAccountId"];
        }
    }
}