using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Mobile_PaidThx.Models;
using SocialPayments.DataLayer;
using SocialPayments.DomainServices;
using NLog;
using Mobile_PaidThx.Controllers.Base;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;

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
        // GET: /Account/FBauth
        public ActionResult SignInWithFacebook(string state, string code)
        {
            using (var ctx = new Context())
            {
                var fbAccount = FBauth(state, code);

                var userService = new UserService(ctx);

                bool isNewUser = false;
                var user = userService.SignInWithFacebook(Guid.Parse(_apiKey), fbAccount.id, fbAccount.email, fbAccount.first_name, fbAccount.last_name, "", fbAccount.accessToken, System.DateTime.Now.AddDays(30), out isNewUser);

                //validate fbAccount.Id is associated with active user
                if (user == null)
                {
                    ModelState.AddModelError("", "Error. Try again..");

                    return View("SignIn");
                }

                Session["User"] = user;
                Session["UserId"] = user.UserId;

                return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
            }
        }
        public ActionResult RegisterWithFacebook(string state, string code)
        {
            using (var ctx = new Context())
            {
                var fbAccount = FBauth(state, code);

                //validate fbAccount.Id is not already associated with active user
                var user = ctx.Users
                    .FirstOrDefault(u => u.FacebookUser.FBUserID == fbAccount.id);

                if (user != null)
                {
                    ModelState.AddModelError("", "The Facebook user is already associated with a registered PaidThx account.");

                    return View("SignIn");
                }

                if (fbAccount != null)
                {
                    ModelState.AddModelError("", "Error registering Facebook user.");

                    return View("SignIn");
                }

                return RedirectToAction("ValidateMobileDevice", "ValidateMobileDevice");
            }
        }

        public FacebookUserModels.FBuser FBauth(string state, string code)
        {
            string response = null;
            string token = null;
            string tokenExp = null;
            FacebookUserModels.FBuser fbAccount = null;


            if (state == fbState)
            {
                //Exchange FB Code for FB Token
                string requestToken = "https://graph.facebook.com/oauth/access_token?" +
                    "client_id=" + fbAppID +
                    "&redirect_uri=" + fbTokenRedirectURL +
                    "&client_secret=" + fbAppSecret +
                    "&code=" + code;

                HttpWebRequest wr = GetWebRequest(requestToken);
                HttpWebResponse resp = null;

                try
                {
                    resp = (HttpWebResponse)wr.GetResponse();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Info, ex.Message);
                }

                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                    if (response.Length > 0)
                    {
                        NameValueCollection qs = HttpUtility.ParseQueryString(response);
                        if (qs["access_token"] != null)
                        {
                            token = qs["access_token"];
                            tokenExp = qs["expires"];
                        }
                    }
                    sr.Close();
                }

                fbAccount.accessToken = token;
                fbAccount.tokenExpires = tokenExp;

                //Use Graph API to get FB UserID and email
                string requestStuff = "https://graph.facebook.com/me?access_token=" + token;
                wr = GetWebRequest(requestStuff);
                resp = (HttpWebResponse)wr.GetResponse();

                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                    if (response.Length > 0)
                    {
                        fbAccount = JsonConvert.DeserializeObject<FacebookUserModels.FBuser>(response);
                    }
                    sr.Close();
                }
            }

            return fbAccount;
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            using (var ctx = new Context())
            {
                if (ModelState.IsValid)
                {
                    // Attempt to register the user
                    MembershipCreateStatus createStatus;

                    Membership.CreateUser(model.Email, model.Password, model.Email, null, null, true, null, out createStatus);

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        FormsAuthentication.SetAuthCookie(model.Email, false /* createPersistentCookie */);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorCodeToString(createStatus));
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
        }

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
            using (var ctx = new Context())
            {
                if (ModelState.IsValid)
                {

                    // ChangePassword will throw an exception rather
                    // than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                        changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("ChangePasswordSuccess");
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }
        public ActionResult SignIn()
        {
            logger.Log(LogLevel.Info, String.Format("Displaying SignIn View"));

            return View();
        }
        [HttpPost]
        public ActionResult SignIn(LogOnModel model, string returnUrl)
        {
            logger.Log(LogLevel.Info, String.Format("Attempting to signin user {0}", model.Email));

            using(var ctx = new Context())
            {
                UserService userService = new UserService(ctx);

                if (ModelState.IsValid)
                {
                    SocialPayments.Domain.User user;
                    if (userService.ValidateUser(model.Email, model.Password, out user))
                    {

                        FormsAuthentication.SetAuthCookie(model.Email, false);
                        Session["User"] = user;
                        Session["UserId"] = user.UserId;

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
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
        }
        public ActionResult ForgotEmail()
        {
            var model = new ForgotPasswordModel();
            model.PasswordSent = false;
            return View();
        }
        [HttpPost]
        public ActionResult ForgotEmail(ForgotPasswordModel model)
        {
            using (var ctx = new Context())
            {
                var securityService = new SocialPayments.DomainServices.SecurityService();

                try
                {
                    if (ModelState.IsValid)
                    {
                        var user = ctx.Users.FirstOrDefault(u => u.UserName == model.UserName || u.MobileNumber == model.UserName);

                        if (user == null)
                            throw new Exception(String.Format("Unable to find user {0}", model.UserName));

                        if (user.EmailAddress.Length == 0)
                            throw new Exception(String.Format("No email address asssociated with username {0}", model.UserName));

                        StringBuilder sbBody = new StringBuilder();
                        sbBody.AppendFormat("Your password is {0}.", securityService.Decrypt(user.Password));

                        //Send Email
                        SmtpClient sc = new SmtpClient();
                        sc.EnableSsl = true;

                        sc.Send("admin@paidthx.com", user.EmailAddress, "Your PaidThx Password", sbBody.ToString());

                        model.PasswordSent = true;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }

                return View(model);
            }
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
            using(var ctx = new Context()) {

                Guid idGuid;

                Guid.TryParse(id, out idGuid);

                if (idGuid == null)
                {
                    ModelState.AddModelError("", "Invalid Id");
                }


                SocialPayments.Domain.PasswordResetAttempt passwordResetDb = ctx.PasswordResetAttempts
                    .FirstOrDefault(p => p.Id == idGuid);

                ResetPasswordModelInput model = new ResetPasswordModelInput();

                if (passwordResetDb == null)
                {
                    ModelState.AddModelError("", "Invalid Attempt");
                    return View(model);
                }

                if (passwordResetDb.ExpiresDate < System.DateTime.Now)
                {
                    ModelState.AddModelError("", "Password reset link has expired.");
                    passwordResetDb.Clicked = true;
                    return View(model);
                }

                if (passwordResetDb.Clicked)
                {
                    ModelState.AddModelError("", "Password reset link has been clicked before. Please generate a new link in the app");
                    return View(model);
                }

                if (passwordResetDb.User.SecurityQuestion == null)
                {
                    model.SecurityQuestion = "";
                    model.HasSecurityQuestion = false;

                    return View(model);
                }
                passwordResetDb.Clicked = true;
                model.HasSecurityQuestion = true;
                model.SecurityQuestion = passwordResetDb.User.SecurityQuestion.Question;

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult ResetPassword(string id, ResetPasswordModelOutput model)
        {
            if (model.NewPassword.Equals(model.ConfirmPassword))
            {
                using (var ctx = new Context())
                {
                    SocialPayments.DomainServices.SecurityService securityService = new SecurityService();
                    UserService userService = new UserService(ctx);

                    try
                    {
                        Guid idGuid;

                        Guid.TryParse(id, out idGuid);

                        if (idGuid == null)
                        {
                            ModelState.AddModelError("", "Invalid Id");
                        }


                        SocialPayments.Domain.PasswordResetAttempt passwordResetDb = ctx.PasswordResetAttempts
                            .FirstOrDefault(p => p.Id == idGuid);

                        if (passwordResetDb == null)
                        {
                            ModelState.AddModelError("", "Invalid Attempt");
                        }

                        if (model.SecurityQuestionAnswer == null)
                        {
                            userService.ResetPassword(passwordResetDb.UserId.ToString(), model.NewPassword);
                            return View("SignIn");
                        }
                        else
                        {
                            userService.ResetPassword(passwordResetDb.UserId.ToString(), model.SecurityQuestionAnswer, model.NewPassword);
                            return View("SignIn");
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "New password and confirm password do not match.");
            }

            return View(model);
        }

        private static HttpWebRequest GetWebRequest(string formattedUri)
        {
            // Create the request’s URI.
            Uri serviceUri = new Uri(formattedUri, UriKind.Absolute);

            // Return the HttpWebRequest.
            return (HttpWebRequest)WebRequest.Create(serviceUri);
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
