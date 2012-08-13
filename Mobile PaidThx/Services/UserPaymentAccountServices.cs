using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

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
    public class UserPaymentAccountServices: ServicesBase
    {
        public string SetupACHAccount(string serviceUrl, string apiKey, string nameOnAccount, string nickName, string routingNumber, string accountNumber,
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

            string jsonResponse = Post(serviceUrl, json);

            var jsonObject = js.Deserialize<Dictionary<string, dynamic>>(jsonResponse);

            return jsonObject["paymentAccountId"];
        }
    }
}