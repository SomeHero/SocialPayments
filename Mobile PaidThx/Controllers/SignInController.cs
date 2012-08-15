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
    }
}
