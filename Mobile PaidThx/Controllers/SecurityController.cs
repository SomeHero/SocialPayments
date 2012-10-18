using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Mobile_PaidThx.CustomAttributes;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.CustomExceptions;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Controllers
{
    [CustomAuthorize]
    public class SecurityController : Controller
    {
        //
        // GET: /Security/

        public ActionResult Index()
        {
            var user = (UserModels.UserResponse)Session["User"];

            return View(new SecurityModels.SecurityPreferencesModel()
            {
                SetupSecurityPin = user.setupSecurityPin
            });
        }

        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(SecurityModels.ChangePasswordModel model)
        {
            var userService = new Services.UserServices();

            var user = (UserModels.UserResponse)Session["User"];
            
            if (String.IsNullOrEmpty(model.NewPassword) || String.IsNullOrEmpty(model.NewPasswordConfirmation))
            {
                ModelState.AddModelError("", "New Password and New Password Confirm are required");

                return View();
            }

            if (model.NewPassword != model.NewPasswordConfirmation)
            {
                ModelState.AddModelError("", "New Passwords Don't Match");

                return View();
            }

            try
            {
                userService.ChangePasssword(user.userId.ToString(), model.OldPassword, model.NewPassword);
            }
            catch (ErrorException ex)
            {
                if (ex.ErrorCode == 1001)
                {
                    Session.Clear();
                    Session.Abandon();

                    FormsAuthentication.SignOut();

                    return RedirectToAction("Index", "SignIn", new { message = "AccountLocked" });
                }

                ModelState.AddModelError("", ex.Message);

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View();
            }

            return RedirectToAction("Index");
        }
        public ActionResult ChangeSecurityPin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangeSecurityPin(SecurityModels.ChangeSecurityPinModel model)
        {
            Session["ChangeSecurityPin_CurrentPinCode"] = model.PinCode;

            return RedirectToAction("ChangeSecurityPinNew");
        }
        public ActionResult ChangeSecurityPinNew()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangeSecurityPinNew(SecurityModels.ConfirmSecurityPinModel model)
        {
            Session["ChangeSecurityPin_NewPinCode"] = model.PinCode;

            return RedirectToAction("ChangeSecurityPinConfirm");
        }
        public ActionResult ChangeSecurityPinConfirm()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangeSecurityPinConfirm(SecurityModels.ConfirmSecurityPinModel model)
        {
            var userService = new Services.UserServices();
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            try
            {
                if (model.PinCode != Session["ChangeSecurityPin_NewPinCode"].ToString())
                {
                    ModelState.AddModelError("", "Your security PINs don't match.  Try Again");

                    return RedirectToAction("ChangeSecurityPinNew");
                }

                userService.ChangeSecurityPin(user.userId.ToString(), Session["ChangeSecurityPin_CurrentPinCode"].ToString(), model.PinCode);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View();
            }


            return RedirectToAction("Index");
        }
        public ActionResult ForgotSecurityPin()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            return View(new SecurityModels.ForgotSecurityPinModel()
            {
                SecurityQuestion = user.securityQuestion,
                SecurityQuestionAnswer = ""
            });
        }
        [HttpPost]
        public ActionResult ForgotSecurityPin(SecurityModels.ForgotSecurityPinModel model)
        {
            Session["ForgotSecurityPin_SecurityQuestionAnswer"] = model.SecurityQuestionAnswer;

            return RedirectToAction("ForgotSecurityPin_PinSwipe");
        }
        public ActionResult ForgotSecurityPin_PinSwipe()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotSecurityPin_PinSwipe(SecurityModels.SecurityPinModel model)
        {
            Session["ForgotSecurityPin_PinCode"] = model.PinCode;

            return RedirectToAction("ForgotSecurityPin_PinSwipeConfirm");
        }
        public ActionResult ForgotSecurityPin_PinSwipeConfirm()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotSecurityPin_PinSwipeConfirm(SecurityModels.SecurityPinModel model)
        {
            var userService = new Services.UserServices();
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            try
            {
                if (model.PinCode != Session["ForgotSecurityPin_PinCode"].ToString())
                {
                    ModelState.AddModelError("", "Your Pin Swipe's Don't Match.  Try Again");

                    return RedirectToAction("ForgotSecurityPin_PinSwipe");
                }

                userService.ResetSecurityPin(user.userId.ToString(), model.PinCode, Session["ForgotSecurityPin_SecurityQuestionAnswer"].ToString());

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View();
            }

            
            return RedirectToAction("Index");
        }
    }
}
