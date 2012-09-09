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
            Session["FBState"] = RandomString(8, false);

            return View(new SignInModels.SignInModel()
            {
                FBState = Session["FBState"].ToString()
            });
        }

        [HttpPost]
        public ActionResult Index(LogOnModel model, string returnUrl)
        {
            _logger.Log(LogLevel.Info, String.Format("Attempting to signin user {0}", model.Email));

            var userService = new UserServices();
            var applicationServices = new ApplicationServices();

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

                ApplicationResponse application;

                try
                {
                    application = applicationServices.GetApplication(_apiKey);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);

                    _logger.Log(LogLevel.Error, String.Format("Exception Getting Application {0}. Exception: {1}. StackTrace: {2}", _apiKey, ex.Message, ex.StackTrace));

                    return View();
                }

                Session["Application"] = application;
                Session["UserId"] = validateResponse.userId;
                Session["User"] = user;

                if (String.IsNullOrEmpty(user.firstName)  || String.IsNullOrEmpty(user.lastName))
                {
                    return RedirectToAction("Personalize", "Register");
                }
                else if (user.bankAccounts.Count() == 0)
                {
                    return RedirectToAction("SetupACHAccount", "Register");
                }

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
        /// <summary>
        /// Generates a random string with the given length
        /// </summary>
        /// <param name="size">Size of the string</param>
        /// <param name="lowerCase">If true, generate lowercase string</param>
        /// <returns>Random string</returns>
        private string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
    }

}
