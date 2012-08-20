using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Net;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Services
{
    public class UserPayPointServices: ServicesBase
    {
        private string _getPayPointsUrl = "{0}Users/{1}/paypoints";
        private string _deletePaypointUrl = "{0}Users/{1}/paypoints/{2}";
        private string _addPaypointUrl = "{0}Users/{1}/paypoints";

        public List<UserModels.UserPayPointResponse> GetPayPoints(string userId)
        {
            var serviceUrl = String.Format(_getPayPointsUrl, _webServicesBaseUrl, userId);

            var response = Get(serviceUrl);

            if(response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);

            var js = new JavaScriptSerializer();


            return js.Deserialize<List<UserModels.UserPayPointResponse>>(response.JsonResponse);
        }
        public void AddPaypoint(String userId, String uri, String type)
        {
            var serviceUrl = String.Format(_addPaypointUrl, _webServicesBaseUrl, userId);
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                Uri = uri,
                PayPointType = type
            });

            var response = Post(serviceUrl, json);

            if (response.StatusCode != HttpStatusCode.Created)
                throw new Exception(response.Description);

        }


        public void DeletePaypoint(string apiKey, string userId, string paypointId)
        {
            string serviceUrl = String.Format(_deletePaypointUrl, _webServicesBaseUrl, userId, paypointId);

            var response = Delete(serviceUrl);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);
        }
    }
}