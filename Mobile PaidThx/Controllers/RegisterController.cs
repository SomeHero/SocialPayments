using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using Mobile_PaidThx.Controllers.Base;
using System.Web.Security;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Configuration;
using Mobile_PaidThx.Services;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Controllers
{
    public class RegisterController : PaidThxBaseController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        private string fbState = "pickle"; //TODO: randomly generate this per session
        private string fbAppID = "332189543469634";
        private string fbAppSecret = "628b100a8e6e9fd8278406a4a675ce0c";
        private string fbTokenRedirectURL = ConfigurationManager.AppSettings["fbTokenRedirectURL"];

        public ActionResult Personalize()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            var model = new PersonalizeModel()
            {
                FirstName = user.firstName,
                LastName = user.lastName,
                ImageUrl = user.imageUrl
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult Personalize(PersonalizeModel model)
        {
            if (ModelState.IsValid)
            {
                var userServices = new UserServices();

                try
                {
                    userServices.PersonalizeUser(Session["UserId"].ToString(), new UserModels.PersonalizeUserRequest()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        ImageUrl = ""
                    });
                }
                catch(Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception in Personalize. {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

                    return View();
                }

                TempData["NameOnAccount"] = model.FirstName + " " + model.LastName;
                return RedirectToAction("SetupACHAccount");
            }

            return View(model);
        }
        public ActionResult SetupACHAccount()
        {
            _logger.Log(LogLevel.Info, String.Format("Displaying SetupACHAccount View"));

            MessageModel paymentModel = null;
            if (Session["Payment"] != null)
                paymentModel = (MessageModel)Session["Payment"];

            TempData["DataUrl"] = "data-url=\"/Register/SetupACHAccount\"";

            string nameOnAccount = "";

            if (TempData["NameOnAccount"] != null)
                nameOnAccount = TempData["NameOnAccount"].ToString();

            return View("SetupACHAccount", new SetupACHAccountModel()
            {
                NameOnAccount = nameOnAccount,
                Payment = paymentModel
            });
        }
        [HttpPost]
        public ActionResult SetupACHAccount(SetupACHAccountModel model)
        {
            _logger.Log(LogLevel.Info, String.Format("Setting Up New ACH Account {0}", model.NameOnAccount));

            var routingNumberServices = new RoutingNumberServices();

            if (!routingNumberServices.ValidateRoutingNumber(model.RoutingNumber))
            {
                ModelState.AddModelError("RoutingNumber", "Invalid Routing Number.  Please check your Bank's Routing Number and Try Again");

                return View(model);
            }

            Session["ACHAccountModel"] = model;

            return RedirectToAction("SetupPinSwipe");
        }
        [HttpPost]
        public bool ValidateRoutingNumber(string routingNumber)
        {
            _logger.Log(LogLevel.Info, String.Format("Validating Routing Number {0}", routingNumber));

            var routingNumberServices = new RoutingNumberServices();

            return routingNumberServices.ValidateRoutingNumber(routingNumber);

        }
        public ActionResult SetupPinSwipe()
        {
            TempData["Message"] = "";

            return View();
        }
        [HttpPost]
        public ActionResult SetupPinSwipe(SetupPinSwipeModel model)
        {
            Session["PinCode"] = model.PinCode;
            return RedirectToAction("ConfirmPinSwipe");
        }
        public ActionResult ConfirmPinSwipe()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ConfirmPinSwipe(ConfirmPinSwipeModel model)
        {
            string firstPinCode = Session["PinCode"].ToString();

            if (firstPinCode != model.PinCode)
            {
                TempData["Message"] = "Your confirmation did not match your first PIN, please create a new PIN and try again.";

                return View("SetupPinSwipe");
            }

            Session["PinCode"] = model.PinCode;

            return RedirectToAction("SecurityQuestion");
        }
        public ActionResult SecurityQuestion()
        {
            var securityQuestionServices = new SecurityQuestionServices();
            var securityQuestions = securityQuestionServices.GetSecurityQuestions();
            var questions = securityQuestions.Select(q => new Mobile_PaidThx.Models.SecurityQuestionModels.SecurityQuestionModel
                {
                    Id = q.Id,
                    Question = q.Question
                }).ToList();

            var model = new SecurityQuestionModel() {

                SecurityQuestionAnswer = "",
                SecurityQuestionId = 1,
                SecurityQuestions = questions
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult SecurityQuestion(SecurityQuestionModel model)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn");

            var achAccountModel = (SetupACHAccountModel)Session["ACHAccountModel"];
            var pinCode = (string)Session["PinCode"];

            var userServices = new Services.UserServices();
            var userPaymentAccountService = new Services.UserPaymentAccountServices();

            var userId = Session["UserId"].ToString();
            var nickName = String.Format("{0} {1}", achAccountModel.AccountType, achAccountModel.AccountNumber.Substring(achAccountModel.AccountNumber.Length - 5, 4));

            string paymentAccountId = "";

            try
            {
                paymentAccountId =
                userPaymentAccountService.SetupACHAccount(userId, _apiKey, achAccountModel.NameOnAccount, nickName,
                achAccountModel.RoutingNumber, achAccountModel.AccountNumber, achAccountModel.AccountType, pinCode, model.SecurityQuestionId, model.SecurityQuestionAnswer);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Info, String.Format("Exception Setting Up New ACH Account {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

            }

            UserModels.UserResponse user;

            try
            {
                user = userServices.GetUser(userId);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                _logger.Log(LogLevel.Error, String.Format("Exception Getting User. Exception: {0}. StackTrace: {1}", ex.Message, ex.StackTrace));

                return View();
            }

            Session["User"] = user;

            TempData["DataUrl"] = "data-url=/mobile/Paystream";

            return RedirectToAction("Index", "Paystream");
        }

        //public ActionResult ValidateMobileDevice()
        //{
        //    if (Session["UserId"] == null)
        //        return View("SignIn");

        //    logger.Log(LogLevel.Info, String.Format("Displaying ValidateMobileDevice View"));

        //    using (var ctx = new Context())
        //    {
        //        var userService = new SocialPayments.DomainServices.UserService(ctx);
        //        var formattingService = new SocialPayments.DomainServices.FormattingServices();

        //        var user = userService.GetUserById(Session["UserId"].ToString());

        //        PaymentModel paymentModel = null;
        //        if (Session["Payment"] != null)
        //            paymentModel = (PaymentModel)Session["Payment"];

        //        return View(new MobileDeviceVerificationModel()
        //        {
        //            Payment = paymentModel,
        //            MobileNumber = formattingService.FormatMobileNumber(user.MobileNumber)
        //        });
        //    }
        //}
        //[HttpPost]
        //public ActionResult ValidateMobileDevice(MobileDeviceVerificationModel model)
        //{
        //    using (var ctx = new Context())
        //    {
        //        logger.Log(LogLevel.Info, String.Format("Validating Mobile Device {0}", model.VerificationCode));

        //        if (Session["UserId"] == null)
        //            return View("SignIn");

        //        Guid userId = (Guid)Session["UserId"];
        //        User user = null;

        //        try
        //        {
        //            user = ctx.Users.FirstOrDefault(u => u.UserId == userId);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Log(LogLevel.Error, String.Format("Exception getting user {0} from database. {1}", userId, ex.Message));
        //        }

        //        if (user == null)
        //        {
        //            logger.Log(LogLevel.Error, String.Format("Unable to find user {0}", userId));
        //            ModelState.AddModelError("", "Unable to validate your mobile device.  Please try again.");

        //            return View(model);
        //        }
        //        if (user.MobileVerificationCode1 == null)
        //        {
        //            logger.Log(LogLevel.Error, String.Format("Mobile Verification is null for user {0}", userId));
        //            ModelState.AddModelError("", "Unable to validate your mobile device.  Please try again.");

        //            return View(model);
        //        }

        //        if (!user.MobileVerificationCode1.ToString().Equals(model.VerificationCode.ToString(), StringComparison.Ordinal))
        //        {
        //            logger.Log(LogLevel.Info, String.Format("Incorrect verification code validating mobile device for {0}.", userId));
        //            ModelState.AddModelError("", "Unable to validate your mobile device.  Please try again.");

        //            return View(model);
        //        }

        //        return RedirectToAction("SetupACHAccount");
        //    }
        //}

    }
}
