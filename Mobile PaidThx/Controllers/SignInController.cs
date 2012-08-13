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

namespace Mobile_PaidThx.Controllers
{
    public class SignInController : Controller
    {
        //
        // GET: /SignIn/
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        private string fbTokenRedirectURL = ConfigurationManager.AppSettings["fbTokenRedirectURL"];

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(LogOnModel model, string returnUrl)
        {
            _logger.Log(LogLevel.Info, String.Format("Attempting to signin user {0}", model.Email));

            var userService = new UserServices();

            if (ModelState.IsValid)
            {
                var jsonResponse = userService.ValidateUser(model.Email, model.Password);

                JavaScriptSerializer js = new JavaScriptSerializer();

                var userResponse = js.Deserialize<UserModels.ValidateUserResponse>(jsonResponse);


                FormsAuthentication.SetAuthCookie(model.Email, false);
                Session["UserId"] = userResponse.userId;
                var user = userService.GetUser(userResponse.userId);
                Session["User"] = user;

                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
                }
            }
            else
            {
                ModelState.AddModelError("", "The user name or password provided is incorrect.");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        //
        // GET: /Account/FBauth
        public ActionResult SignInWithFacebook(string state, string code)
        {
            var faceBookServices = new FacebookServices();
            var userServices = new UserServices();

            var redirect = String.Format(fbTokenRedirectURL, "SignIn/SignInWithFacebook/");

            var fbAccount = faceBookServices.FBauth(state, code, redirect);
            var jsonResponse = userServices.SignInWithFacebook(_apiKey, fbAccount.id, fbAccount.first_name, fbAccount.last_name, fbAccount.email, "", fbAccount.accessToken, System.DateTime.Now.AddDays(30));

            //validate fbAccount.Id is associated with active user
            //if (user == null)
            //{
            //    ModelState.AddModelError("", "Error. Try again..");

            //    return View("SignIn");
            //}

            JavaScriptSerializer js = new JavaScriptSerializer();

            var facebookSignInResponse = js.Deserialize<UserModels.FacebookSignInResponse>(jsonResponse);

            Session["UserId"] = facebookSignInResponse.userId;

            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
        }
        
    }
}
