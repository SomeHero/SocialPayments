using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using SocialPayments.Domain;
using SocialPayments.DomainServices;
using SocialPayments.Web.Models;
using CodeFirstMembershipDemoSharp.Code;
using System.Net.Mail;
using SocialPayments.ThirdPartyServices.FedACHService;

namespace SocialPayments.Web.Controllers
{
    public class AccountController : Controller
    {

        //
        // GET: /Account/LogOn
        private UserService userService = new UserService();

        public ActionResult LogOn()
        {
            return View();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                User user;
                if (userService.ValidateUser(model.UserName, model.Password, out user))
                {
                    
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    Session["UserId"] = user.UserId;

                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
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

        public ActionResult LogOff()
        {
            CodeFirstSecurity.Logout();
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

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            FedACHService fedACHService = new FedACHService();
            FedACHList fedACHList;

            if (ModelState.IsValid)
            {
                //Validate Routing Number
                var isRoutingNumberValid = false;

                try
                {
                    isRoutingNumberValid = fedACHService.getACHByRoutingNumber(model.RoutingNumber, out fedACHList);
                }
                catch (Exception ex)
                {
                    //ToDo: Add Logging
                }

                if (!isRoutingNumberValid)
                    throw new Exception("Unable to validate routing number");

                // Attempt to register the user
                try
                {
                    MembershipCreateStatus createStatus = MembershipCreateStatus.ProviderError;
                    string token = CodeFirstSecurity.CreateAccount(model.UserName, model.Password, model.Email,
                                                                   model.MobileNumber, model.RoutingNumber,
                                                                   model.AccountNumber,
                                                                   model.AccountType, out createStatus, true);
                    if (createStatus == MembershipCreateStatus.Success)
                    {
                       // var emailLog = emailLogService.AddEmailLog(application.ApiKey, emailRequest.FromAddress, emailRequest.ToAddress, emailRequest.Subject, emailRequest.Body, null);

                        //Send Email
                        SmtpClient sc = new SmtpClient();
                        sc.EnableSsl = true;
                        try
                        {
                            string link = String.Format("{0}/Web/Account/ConfirmUser?confirmationToken={1}", HttpContext.ApplicationInstance.Request.Url.GetLeftPart(UriPartial.Authority), token);

                            sc.Send("admin@pdthx.me", model.Email, "Confirm your PdThx Account",
                                    String.Format("To activate your account please follow the link {0}", link));
                            //Update Email Status
                            //emailLogService.UpdateEmailLog(emailLog.ApiKey, emailLog.FromEmailAddress, emailLog.ToEmailAddress, emailLog.Subject, emailLog.Body,
                            // Domain.EmailStatus.Sent, System.DateTime.Now);
                        }
                        catch (Exception ex)
                        {
                            //Update Email Status
                            //emailLogService.UpdateEmailLog(emailLog.Id, emailLog.FromEmailAddress, emailLog.ToEmailAddress, emailLog.Subject, emailLog.Body,
                               //Domain.EmailStatus.Failed, null);
                            throw;
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorCodeToString(createStatus));
                    }
                }
                catch (MembershipCreateUserException ex)
                {
                    ModelState.AddModelError("", ErrorCodeToString(ex.StatusCode));
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
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

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }
        
        public ActionResult ConfirmUser(string confirmationToken)
        {
            var confirmed = false;

            try
            {
                confirmed = userService.ConfirmUser(confirmationToken);
            }
            catch(ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch(Exception ex)
            {
                throw ex;
            }

            if(confirmed)
            {
                return View();
            }
            else
            {
                throw new Exception();
            }
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
