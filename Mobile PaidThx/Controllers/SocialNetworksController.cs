using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using Mobile_PaidThx.Services;
using System.Web.Script.Serialization;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Security;
using System.Web.Routing;
using System.Configuration;
using System.Text;


namespace Mobile_PaidThx.Controllers
{
    public class SocialNetworksController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        private string fbTokenRedirectURL = ConfigurationManager.AppSettings["fbTokenRedirectURL"];

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LinkFacebookAccount(string state, string code)
        {
            var faceBookServices = new FacebookServices();
            var userServices = new UserServices();

            var redirect = String.Format(fbTokenRedirectURL, "SocialNetworks/LinkFacebookAccount");

            var fbAccount = faceBookServices.FBauth(state, code, redirect);

            UserModels.FacebookSignInResponse facebookSignInResponse;
            bool isNewUser = false;

            try
            {
                facebookSignInResponse = userServices.SignInWithFacebook(_apiKey, fbAccount.id, fbAccount.first_name, fbAccount.last_name, fbAccount.email, "", fbAccount.accessToken, System.DateTime.Now.AddDays(30),
                (Session["MessageId"] != null ? Session["MessageId"].ToString() : ""), out isNewUser);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Signing In with Facebook. {0} Stack Trace: {1}", ex.Message, ex.StackTrace));

                ModelState.AddModelError("", ex.Message);

                return View("Index", new JoinModels.JoinModel()
                    {
                        UserName = "",
                        Message = null
                    });
            }

            List<FacebookModels.Friend> friends;

            try
            {
                _logger.Log(LogLevel.Error, String.Format("Getting Facebook Friends. Access Token {0}", fbAccount.accessToken));
            
                friends = faceBookServices.GetFriendsList(fbAccount.accessToken);
            }
            catch (Exception ex)
            {
                friends = new List<FacebookModels.Friend>();

                _logger.Log(LogLevel.Error, String.Format("Exception Getting Facebook Friends. Access Token {0}. Exception: {1}", fbAccount.accessToken, ex.Message));
            }

            Session["Friends"] = friends;

            return View("Index");
        }
    }
}
