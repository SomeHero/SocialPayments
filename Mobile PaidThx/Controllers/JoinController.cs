using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services;
using System.Web.Script.Serialization;
using Mobile_PaidThx.Services.ResponseModels;
using System.Web.Routing;
using System.Configuration;

namespace Mobile_PaidThx.Controllers
{
    public class JoinController : Controller
    {
        //
        // GET: /Join/
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        private string fbTokenRedirectURL = ConfigurationManager.AppSettings["fbTokenRedirectURL"];

        public ActionResult Index(string messageId)
        {
            if (String.IsNullOrEmpty(messageId) || messageId.Length <= 32)
                return View("Index", new JoinModels.JoinModel()
                {
                    UserName = "",
                    Payment = null
                });

            var messageServices = new MessageServices();
            var payment = messageServices.GetMessage(messageId);

            if (payment == null)
                return View("Index", new JoinModels.JoinModel()
                {
                    UserName = "",
                    Payment = null
                });

            Session["MessageId"] = payment.Id;

            return View("Index", new JoinModels.JoinModel()
            {
                UserName = (payment.recipientUriType == "EmailAddress" ? payment.recipientUri : ""),
                Payment = new PaymentModel()
                {
                    Amount = payment.amount,
                    Comments = payment.comments,
                    MobileNumber = payment.recipientUri,
                    Sender = payment.senderName,
                    SenderImageUrl = payment.transactionImageUri
                }
            });

        }
        [HttpPost]
        public ActionResult Index(RegisterModel model)
        {
            _logger.Log(LogLevel.Info, String.Format("Register User {0}", model.Email));

            var userServices = new Services.UserServices();
            UserModels.UserResponse user;

            try
            {
                user = userServices.RegisterUser(_apiKey, model.Email, model.Password, model.Email, "MobileWeb", "",
                    (Session["MessageId"] != null ? Session["MessageId"].ToString() : ""));
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Registering User {0}. {1} Stack Trace: {2}", model.Email, ex.Message, ex.StackTrace));

                ModelState.AddModelError("", ex.Message);

                return View("Index",  new JoinModels.JoinModel()
                {
                    UserName = "",
                    Payment = null
                });
            }


            Session["UserId"] = user.userId;
            Session["User"] = user;

            return RedirectToAction("Personalize", "Register");
        }

        public ActionResult SignInWithFacebook(string state, string code)
        {
            var faceBookServices = new FacebookServices();
            var userServices = new UserServices();

            var redirect = String.Format(fbTokenRedirectURL, "Join/SignInWithFacebook/");

            var fbAccount = faceBookServices.FBauth(state, code, redirect);

            UserModels.FacebookSignInResponse facebookSignInResponse;
            bool isNewUser = false;

            try
            {
                facebookSignInResponse = userServices.SignInWithFacebook(_apiKey, fbAccount.id, fbAccount.first_name, fbAccount.last_name, fbAccount.email, "", fbAccount.accessToken, System.DateTime.Now.AddDays(30),
                (Session["MessageId"] != null ? Session["MessageId"].ToString() : ""), out isNewUser);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Signing In with Facebook. {0} Stack Trace: {1}", ex.Message));

                ModelState.AddModelError("", ex.Message);

                return View("Index");
            }

            UserModels.UserResponse userResponse;

            try
            {
                userResponse = userServices.GetUser(facebookSignInResponse.userId);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Getting User. {0} Stack Trace: {1}", ex.Message));

                ModelState.AddModelError("", ex.Message);

                return View("Index");
            }

            Session["UserId"] = facebookSignInResponse.userId;
            Session["User"] = userResponse;

            if(isNewUser)
                return RedirectToAction("Personalize", "Register", new RouteValueDictionary() { });
            else
                return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
            
        }
    }
}
