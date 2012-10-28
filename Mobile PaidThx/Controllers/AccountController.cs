using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Mobile_PaidThx.Models;
using NLog;
using Mobile_PaidThx.Controllers.Base;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
using Mobile_PaidThx.Services;
using System.Web.Script.Serialization;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Controllers
{
   
    public class AccountController : PaidThxBaseController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        private string fbState = "pickle"; //TODO: randomly generate this per session
        private string fbAppID = "332189543469634";
        private string fbAppSecret = "628b100a8e6e9fd8278406a4a675ce0c";
        private string fbTokenRedirectURL = ConfigurationManager.AppSettings["fbTokenRedirectURL"];

        //
        // GET: /Account/LogOn

        public ActionResult LogOn()
        {
            return View();
        }

        //
        // POST: /Account/LogOn

        //[HttpPost]
        //public ActionResult LogOn(LogOnModel model, string returnUrl)
       // {
            //if (ModelState.IsValid)
            //{
            //    if (Membership.ValidateUser(model.UserName, model.Password))
            //    {
            //        FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
            //        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
            //            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
            //        {
            //            return Redirect(returnUrl);
            //        }
            //        else
            //        {
            //            return RedirectToAction("Index", "Home");
            //        }
            //    }
            //    else
            //    {
            //        ModelState.AddModelError("", "The user name or password provided is incorrect.");
            //    }
            //}

            //// If we got this far, something failed, redisplay form
            //return View(model);
       // }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            //using (var ctx = new Context())
            //{
            //    if (ModelState.IsValid)
            //    {

            //        // ChangePassword will throw an exception rather
            //        // than return false in certain failure scenarios.
            //        bool changePasswordSucceeded;
            //        try
            //        {
            //            MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
            //            changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
            //        }
            //        catch (Exception)
            //        {
            //            changePasswordSucceeded = false;
            //        }

            //        if (changePasswordSucceeded)
            //        {
            //            return RedirectToAction("ChangePasswordSuccess");
            //        }
            //        else
            //        {
            //            ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
            //        }
            //    }

            //    // If we got this far, something failed, redisplay form
            //    return View(model);
            //}
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }
        public ActionResult VerifyPayPoint(string id)
        {
            var userService = new UserServices();

            try
            {
                userService.VerifyPayPoint(id);
            }
            catch (Exception ex)
            {
                ViewBag.Confirmed = false;
                ViewBag.Message = ex.Message;

                return View();
            }

            ViewBag.Confirmed = true;

            return View();
        }

        
        public ActionResult ForgotPassword()
        {
            var model = new ForgotPasswordModel();
            model.PasswordSent = false;
            return View(model);
        }
        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordModel model)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    var userServices = new UserServices();
                    userServices.ForgotPassword(_apiKey, model.UserName);
                }
            }
            catch (Exception ex)
            {
                model.PasswordSent = false;

                ModelState.AddModelError("", ex.Message);

                return View(model);
            }

            model.PasswordSent = true;

            return View(model);
        }
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "SignIn");
        }

        public ActionResult ResetPassword(string id)
        {
             var userService = new UserServices();
             ResetPasswordModel model = new ResetPasswordModel();
             UserModels.ValidateResetPasswordAttemptResponse response = null;

            try {
                response = userService.ValidateResetPasswordAttempt(_apiKey, id);

                model.HasSecurityQuestion = response.HasSecurityQuestion;
                model.SecurityQuestion = response.SecurityQuestion;

                Session["UserId"] = response.UserId;
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ResetPassword(string id, ResetPasswordModel model)
        {
            if (model.NewPassword.Equals(model.ConfirmPassword))
            {
                var userServices = new UserServices();

                try
                {
                    string userId = Session["UserId"].ToString();

                    userServices.ResetPassword(userId, model.SecurityQuestionAnswer, model.NewPassword);

                    TempData["Success"] = "reset";

                    return RedirectToAction("Index", "SignIn");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);

                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "New password and confirm password do not match.");
            }

            return View(model);
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
