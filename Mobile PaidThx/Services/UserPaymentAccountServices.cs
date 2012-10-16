using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Mobile_PaidThx.Services.ResponseModels;
using System.Net;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.CustomExceptions;

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
        private string _setPreferredReceiveAccountUrl = "{0}Users/{1}/PaymentAccounts/set_preferred_receive_account";
        private string _setPreferredSendAccountUrl = "{0}Users/{1}/PaymentAccounts/set_preferred_send_account";
        private string _addACHAccountUrl = "{0}Users/{1}/PaymentAccounts/add_account";
        private string _editACHAccountUrl = "{0}Users/{1}/PaymentAccounts/{2}";
        private string _deleteACHAccountUrl = "{0}Users/{1}/PaymentAccounts/{2}";
        private string _getACHAccountsUrl = "{0}Users/{1}/PaymentAccounts";
        private string _verifyACHAccountUrl = "{0}Users/{1}/paymentaccounts/{2}/verify_account";

        public List<AccountModels.AccountResponse> GetAccounts(String apiKey, String userId)
        {
            var serviceUrl = String.Format(_getACHAccountsUrl, _webServicesBaseUrl, userId);

            var response = Get(serviceUrl);
            var js = new JavaScriptSerializer();

            if(response.StatusCode != HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            return js.Deserialize<List<AccountModels.AccountResponse>>(response.JsonResponse);
        }
        public ServiceResponse DeleteAccount(String apiKey, String userId, String bankId, string securityPin)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                SecurityPin = securityPin
            });

            string serviceUrl = String.Format(_deleteACHAccountUrl, _webServicesBaseUrl, userId, bankId);
            var response = Delete(serviceUrl, json);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            return response;
        }
        
        public void SetSendAccount(String apiKey, String userId, String bankId, String securityPin)
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

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }

        public void SetReceiveAccount(String apiKey, String userId, String bankId, String securityPin)
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

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }

        public void EditAccount(String apiKey, String userId, String bankId, String nickname, String nameOnAccount, 
            String routingNumber, String accountType, string securityPin)
        {
            string editUrl = String.Format(_editACHAccountUrl, _webServicesBaseUrl, userId,  bankId);
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                ApiKey = apiKey,
                Nickname = nickname,
                NameOnAccount = nameOnAccount,
                RoutingNumber = routingNumber,
                AccountType = accountType,
                SecurityPin = securityPin
            });

            var response = Put(editUrl, json);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }

        public void AddAccount(String apiKey, String userId, String nickName, String nameOnAccount, String routingNumber, string accountNumber, string accountType, string securityPin)
        {
            var serviceUrl = String.Format(_addACHAccountUrl, _webServicesBaseUrl, userId);
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                ApiKey = apiKey,
                NickName = nickName,
                NameOnAccount = nameOnAccount,
                RoutingNumber = routingNumber,
                AccountNumber = accountNumber,
                AccountType = accountType,
                SecurityPin = securityPin
            });

            var response = Post(serviceUrl, json);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            //var jsonObject = js.Deserialize<Dictionary<string, dynamic>>(response.JsonResponse);

            //return jsonObject["paymentAccountId"];
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

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }

            var jsonObject = js.Deserialize<Dictionary<string, dynamic>>(response.JsonResponse);

            return jsonObject["paymentAccountId"];
        }
        public void VerifyACHAccount(string userId, string accountId, double amount1, double amount2)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                depositAmount1 = amount1,
                depositAmount2 = amount2
            });

            var response = Post(String.Format(_verifyACHAccountUrl, _webServicesBaseUrl, userId, accountId), json);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var error = js.Deserialize<ErrorModels.ErrorModel>(response.JsonResponse);

                throw new ErrorException(error.Message, error.ErrorCode);
            }
        }
    }
}