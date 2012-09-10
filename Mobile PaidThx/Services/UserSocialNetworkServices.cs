using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Script.Serialization;
using System.Net;

namespace Mobile_PaidThx.Services
{
    public class UserSocialNetworkServices : ServicesBase
    {
        private string _getUserSocialNetworkUrl = "{0}Users/{1}/socialnetworks";
        private string _deleteUserSocialNetworkUrl = "{0}Users/{1}/socialnetworks/{2}";
        private string _addUserSocialNetworkUrl = "{0}Users/{1}/socialnetworks";

        public void AddPaypoint(String userId, String socialNetworkName, String socialNetworkUserId, String socialNetworkUserToken)
        {
            var serviceUrl = String.Format(_addUserSocialNetworkUrl, _webServicesBaseUrl, userId);
            JavaScriptSerializer js = new JavaScriptSerializer();

            var json = js.Serialize(new
            {
                SocialNetworkType = socialNetworkName,
                SocialNetworkUserId = socialNetworkUserId,
                SocialNetworkUserToken =socialNetworkUserToken
            });

            var response = Post(serviceUrl, json);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(response.Description);

        }
    }
}