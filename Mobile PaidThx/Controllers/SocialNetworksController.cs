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
            if (Session["UserId"] == null)
                return RedirectToAction("Index", "SignIn", null);
            
            var userService = new Services.UserServices();
            
            var user = userService.GetUser(Session["UserId"].ToString());

            Session["FBState"] = "pickle";

            return View(new SocialNetworksModels.Index()
            {
                FBState = "pickle",
                UserSocialNetworks = user.userSocialNetworks.Select(u => new SocialNetworksModels.UserSocialNetwork() {
                    Name = u.SocialNetwork
                }).ToList()
            });
        }
        public ActionResult LinkFacebookAccount(string state, string code)
        {
              if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            var userService = new Services.UserServices();
            var faceBookServices = new FacebookServices();
            var userSocialNetworkServices = new UserSocialNetworkServices();

            var user = userService.GetUser(Session["UserId"].ToString());

            var redirect = String.Format(fbTokenRedirectURL, "SocialNetworks/LinkFacebookAccount");

            var fbAccount = faceBookServices.FBauth(state, code, redirect);

            UserModels.FacebookSignInResponse facebookSignInResponse;
            bool isNewUser = false;

            try
            {
                userSocialNetworkServices.AddPaypoint(user.userId.ToString(), "Facebook", fbAccount.id, fbAccount.accessToken);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Signing In with Facebook. {0} Stack Trace: {1}", ex.Message, ex.StackTrace));

                TempData["Message"] = ex.Message;

                return RedirectToAction("Index");
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

            return RedirectToAction("Index");
            
        }
    }
}
