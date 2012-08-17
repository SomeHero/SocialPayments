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

        private string _mobileWebSiteUrl = ConfigurationManager.AppSettings["MobileWebSiteUrl"];
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
                UserModels.ValidateUserResponse validateResponse;
                    
                try
                {
                    validateResponse = userService.ValidateUser(model.Email, model.Password);
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);

                    _logger.Log(LogLevel.Error, String.Format("Exception Signing In User. Exception: {0} StackTrace: {1}", ex.Message, ex.StackTrace));
                
                    return View();
                }

                UserModels.UserResponse user;
                  
                try {
                    user = userService.GetUser(validateResponse.userId);
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);

                    _logger.Log(LogLevel.Error, String.Format("Exception Getting User. Exception: {0}. StackTrace: {1}", ex.Message, ex.StackTrace));

                    return View();
                }

                FormsAuthentication.SetAuthCookie(model.Email, false);

                Session["UserId"] = validateResponse.userId;
                Session["User"] = user;

                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    TempData["DataUrl"] = "data-url=./Paystream";

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
