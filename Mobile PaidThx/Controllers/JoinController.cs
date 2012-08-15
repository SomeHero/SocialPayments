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

        private string _userServiceUrl = "http://23.21.203.171/api/internal/api/Users";
        private string _setupACHAccountServiceUrl = "http://23.21.203.171/api/internal/api/Users/{0}/PaymentAccounts";

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

            string userId = String.Empty;

            try
            {
                var userServices = new Services.UserServices();
                userId = userServices.RegisterUser(_userServiceUrl, _apiKey, model.Email, model.Password, model.Email, "MobileWeb", "",
                    (Session["MessageId"] != null ? Session["MessageId"].ToString() : ""));

                Session["UserId"] = userId;

                var user = userServices.GetUser(userId);
                Session["User"] = user;

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Registering User {0}. {1}", model.Email, ex.Message));

                ModelState.AddModelError("", "Error Registering User");

                return View("Index");
            }

            return RedirectToAction("Personalize", "Register");
        }

        public ActionResult SignInWithFacebook(string state, string code)
        {
            var faceBookServices = new FacebookServices();
            var userServices = new UserServices();

            var redirect = String.Format(fbTokenRedirectURL, "Join/SignInWithFacebook/");

            var fbAccount = faceBookServices.FBauth(state, code, redirect);
            var response = userServices.SignInWithFacebook(_apiKey, fbAccount.id, fbAccount.first_name, fbAccount.last_name, fbAccount.email, "", fbAccount.accessToken, System.DateTime.Now.AddDays(30),
                (Session["MessageId"] != null ? Session["MessageId"].ToString() : ""));

            //validate fbAccount.Id is associated with active user
            //if (user == null)
            //{
            //    ModelState.AddModelError("", "Error. Try again..");

            //    return View("SignIn");
            //}

            if(response.StatusCode != System.Net.HttpStatusCode.Created && response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                ModelState.AddModelError("", response.Description);

                return View("Index");
            }

            JavaScriptSerializer js = new JavaScriptSerializer();

            var facebookSignInResponse = js.Deserialize<UserModels.FacebookSignInResponse>(response.JsonResponse);

            Session["UserId"] = facebookSignInResponse.userId;

            var user = userServices.GetUser(facebookSignInResponse.userId);
            Session["User"] = user;

            if(response.StatusCode == System.Net.HttpStatusCode.Created)
                return RedirectToAction("Personalize", "Register", new RouteValueDictionary() { });
            else
                return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
            
        }
    }
}
