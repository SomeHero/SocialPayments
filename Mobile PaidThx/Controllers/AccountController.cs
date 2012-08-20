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
            //using(var ctx = new Context())
            //{
            //    Guid payPointVerificationID;

            //    Guid.TryParse(id, out payPointVerificationID);

            //    if (payPointVerificationID == null)
            //    {
            //        ViewBag.Message = "Sorry, we are unable to complete verification";

            //        return View();
            //    }

            //    var payPointVerification = ctx.UserPayPointVerifications
            //        .FirstOrDefault(p => p.Id == payPointVerificationID);

            //    if (payPointVerification == null)
            //    {
            //        ViewBag.Message = "Sorry, we are unable to complete verification";

            //        return View();
            //    }

            //    if (payPointVerification.Confirmed)
            //    {
            //        ViewBag.Message = String.Format("Sorry, we are unable to continue verifying this pay point.  {0} is already verified.",
            //            payPointVerification.UserPayPoint.URI);

            //        return View();
            //    }

            //    if (payPointVerification.ExpirationDate < System.DateTime.Now)
            //    {
            //        ViewBag.Message = String.Format("Sorry, we are unable to continue verifying the PayPoint {0}.  The verification link has expired.",
            //            payPointVerification.UserPayPoint.URI);

            //        return View();
            //    }

            //    payPointVerification.Confirmed = true;
            //    payPointVerification.ConfirmedDate = System.DateTime.Now;
            //    payPointVerification.UserPayPoint.Verified = true;
            //    payPointVerification.UserPayPoint.VerifiedDate = System.DateTime.Now;
                
            //    ctx.SaveChanges();

            //    ViewBag.Message = String.Format("Thanks. You have completed verification of {0}.  You can now begin to accept payment to this PayPoint.",
            //            payPointVerification.UserPayPoint.URI);

            //    return View();
            //}

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
            //using (var ctx = new Context())
            //{
            //    var securityService = new SocialPayments.DomainServices.SecurityService();

            //    try
            //    {
            //        if (ModelState.IsValid)
            //        {
            //            var user = ctx.Users.FirstOrDefault(u => u.UserName == model.UserName || u.MobileNumber == model.UserName);

            //            if (user == null)
            //                throw new Exception(String.Format("Unable to find user {0}", model.UserName));

            //            if (user.EmailAddress.Length == 0)
            //                throw new Exception(String.Format("No email address asssociated with username {0}", model.UserName));
            //            //Send Email
            //            //SmtpClient sc = new SmtpClient();
            //            //sc.EnableSsl = true;

            //            //sc.Send("admin@paidthx.com", user.EmailAddress, "Your PaidThx Password", sbBody.ToString());

            //            UserService userService = new UserService(ctx);
            //            userService.SendResetPasswordLink(user);

            //            model.PasswordSent = true;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        ModelState.AddModelError("", ex.Message);
            //    }

            //    return View(model);
            //}
            return View(model);
        }
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("SignIn", "Account");
        }

        public ActionResult ResetPassword(string id)
        {
            //using(var ctx = new Context()) {

            //    Guid idGuid;

            //    Guid.TryParse(id, out idGuid);

            //    if (idGuid == null)
            //    {
            //        ModelState.AddModelError("", "Invalid Id");
            //    }


            //    SocialPayments.Domain.PasswordResetAttempt passwordResetDb = ctx.PasswordResetAttempts
            //        .FirstOrDefault(p => p.Id == idGuid);

            //    ResetPasswordModelInput model = new ResetPasswordModelInput();

            //    if (passwordResetDb == null)
            //    {
            //        ModelState.AddModelError("", "Invalid Attempt");
            //        return View(model);
            //    }

            //    if (passwordResetDb.ExpiresDate < System.DateTime.Now)
            //    {
            //        ModelState.AddModelError("", "Password reset link has expired.");
            //        passwordResetDb.Clicked = true;
            //        return View(model);
            //    }

            //    if (passwordResetDb.Clicked)
            //    {
            //        ModelState.AddModelError("", "Password reset link has been clicked before. Please generate a new link in the app");
            //        return View(model);
            //    }

            //    if (passwordResetDb.User.SecurityQuestion == null)
            //    {
            //        model.SecurityQuestion = "";
            //        model.HasSecurityQuestion = false;

            //        return View(model);
            //    }
            //    passwordResetDb.Clicked = true;
            //    model.HasSecurityQuestion = true;
            //    model.SecurityQuestion = passwordResetDb.User.SecurityQuestion.Question;

            //    return View(model);
            //}

            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string id, ResetPasswordModelOutput model)
        {
            if (model.NewPassword.Equals(model.ConfirmPassword))
            {
                //using (var ctx = new Context())
                //{
                //    SocialPayments.DomainServices.SecurityService securityService = new SecurityService();
                //    UserService userService = new UserService(ctx);

                //    try
                //    {
                //        Guid idGuid;

                //        Guid.TryParse(id, out idGuid);

                //        if (idGuid == null)
                //        {
                //            ModelState.AddModelError("", "Invalid Id");
                //        }


                //        SocialPayments.Domain.PasswordResetAttempt passwordResetDb = ctx.PasswordResetAttempts
                //            .FirstOrDefault(p => p.Id == idGuid);

                //        if (passwordResetDb == null)
                //        {
                //            ModelState.AddModelError("", "Invalid Attempt");
                //        }

                //        if (model.SecurityQuestionAnswer == null)
                //        {
                //            userService.ResetPassword(passwordResetDb.UserId.ToString(), model.NewPassword);
                //            return View("SignIn");
                //        }
                //        else
                //        {
                //            userService.ResetPassword(passwordResetDb.UserId.ToString(), model.SecurityQuestionAnswer, model.NewPassword);
                //            return View("SignIn");
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        ModelState.AddModelError("", ex.Message);
                //    }
                //}
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
