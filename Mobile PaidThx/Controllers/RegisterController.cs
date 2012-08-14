using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialPayments.DataLayer;
using Mobile_PaidThx.Models;
using NLog;
using SocialPayments.Domain;
using SocialPayments.DomainServices;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Mobile_PaidThx.Controllers.Base;
using System.Web.Security;
using SocialPayments.ThirdPartyServices.FedACHService;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        private string fbState = "pickle"; //TODO: randomly generate this per session
        private string fbAppID = "332189543469634";
        private string fbAppSecret = "628b100a8e6e9fd8278406a4a675ce0c";
        private string fbTokenRedirectURL = ConfigurationManager.AppSettings["fbTokenRedirectURL"];


        private string _userServiceUrl = "http://23.21.203.171/api/internal/api/Users";
        private string _setupACHAccountServiceUrl = "http://23.21.203.171/api/internal/api/Users/{0}/PaymentAccounts";

        public ActionResult Index()
        {
            ModelState.Clear();

            return View("Register", new RegisterModel());
        }
        [HttpPost]
        public ActionResult Index(RegisterModel model)
        {
            logger.Log(LogLevel.Info, String.Format("Register User {0}", model.Email));

            string userId = String.Empty;

            try
            {
                var userServices = new Services.UserServices();
                userId = userServices.RegisterUser(_userServiceUrl, _apiKey, model.Email, model.Password, model.Email, "MobileWeb", "");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, String.Format("Exception Registering User {0}. {1}", model.Email, ex.Message));

                ModelState.AddModelError("", "Error Registering User");

                return View("Index");
            }

            Session["UserId"] = userId;

            return RedirectToAction("Personalize");
        }
        public ActionResult Personalize()
        {
            var model = new PersonalizeModel()
            {
                FirstName = "",
                LastName = "",
                ImageUrl = ""
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult Personalize(PersonalizeModel model)
        {
            if (ModelState.IsValid)
            {
                var userServices = new UserServices();
                var jsonResponse = userServices.PersonalizeUser(Session["UserId"].ToString(), new UserModels.PersonalizeUserRequest()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    ImageUrl = ""
                });

                return RedirectToAction("SetupACHAccount");
            }

            return View(model);
        }
        public ActionResult SetupACHAccount()
        {
            logger.Log(LogLevel.Info, String.Format("Displaying SetupACHAccount View"));

            PaymentModel paymentModel = null;
            if (Session["Payment"] != null)
                paymentModel = (PaymentModel)Session["Payment"];

            TempData["DataUrl"] = "data-url=\"/Register/SetupACHAccount\"";

            return View("SetupACHAccount", new SetupACHAccountModel()
            {
                Payment = paymentModel
            });
        }
        [HttpPost]
        public ActionResult SetupACHAccount(SetupACHAccountModel model)
        {
            logger.Log(LogLevel.Info, String.Format("Setting Up New ACH Account {0}", model.NameOnAccount));

            Session["ACHAccountModel"] = model;

            return RedirectToAction("SetupPinSwipe");
        }

        public ActionResult SetupPinSwipe()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SetupPinSwipe(SetupPinSwipeModel model)
        {
            return RedirectToAction("ConfirmPinSwipe");
        }
        public ActionResult ConfirmPinSwipe()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ConfirmPinSwipe(ConfirmPinSwipeModel model)
        {
            Session["PinCode"] = model.PinCode;

            return RedirectToAction("SecurityQuestion");
        }
        public ActionResult SecurityQuestion()
        {
            var model = new SecurityQuestionModel()
            {
                SecurityQuestionAnswer = "",
                SecurityQuestionId = 1,
                SecurityQuestions = new List<SecurityQuestionModels.SecurityQuestionModel>() {
                    new SecurityQuestionModels.SecurityQuestionModel {
                        Id = 1,
                        Question = "Who is Sebastian?"
                    },
                    new SecurityQuestionModels.SecurityQuestionModel {
                        Id = 2,
                        Question = "What is your first name?"
                    }
                }
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

            var userPaymentAccountService = new Services.UserPaymentAccountServices();

            var userId = Session["UserId"].ToString();
            var nickName = String.Format("{0} {1}", achAccountModel.AccountType, achAccountModel.AccountNumber.Substring(achAccountModel.AccountNumber.Length - 5, 4));

            string paymentAccountId = userPaymentAccountService.SetupACHAccount(String.Format(_setupACHAccountServiceUrl, userId), _apiKey, achAccountModel.NameOnAccount, nickName,
                achAccountModel.RoutingNumber, achAccountModel.AccountNumber, achAccountModel.AccountType, pinCode, model.SecurityQuestionId, model.SecurityQuestionAnswer);

            return RedirectToAction("Index", "Paystream");
        }
        public ActionResult RegisterWithFacebook(string state, string code)
        {
            using (var ctx = new Context())
            {
                var securityService = new SocialPayments.DomainServices.SecurityService();
                var fbAccount = FBauth(state, code);

                //validate fbAccount.Id is not already associated with active user
                var user = ctx.Users
                    .FirstOrDefault(u => u.FacebookUser.FBUserID == fbAccount.id);

                if (user != null)
                {
                    ModelState.AddModelError("", "The Facebook user is already associated with a registered PaidThx account.");

                    return View("Index");
                }

                if (fbAccount == null)
                {
                    ModelState.AddModelError("", "Error registering Facebook user.");

                    return View("Index");
                }

                Role memberRole = null;

                try
                {
                    memberRole = ctx.Roles.FirstOrDefault(r => r.RoleName == "Member");
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Fatal, String.Format("Exception Getting Member Role. {0}", ex.Message));
                    ModelState.AddModelError("", "There was an error registering your account.  Please try again.");

                    return View("Index");
                }

                try
                {

                    user = ctx.Users.Add(new User()
                    {
                        UserId = Guid.NewGuid(),
                        ApiKey = new Guid(_apiKey),
                        CreateDate = System.DateTime.Now,
                        PasswordChangedDate = DateTime.UtcNow,
                        PasswordFailuresSinceLastSuccess = 0,
                        LastPasswordFailureDate = DateTime.UtcNow,
                        EmailAddress = "",
                        //IsLockedOut = isLockedOut,
                        //LastLoggedIn = System.DateTime.Now,
                        MobileNumber = "",
                        Password = "", //hash
                        SecurityPin = securityService.Encrypt("1111"),
                        UserName = fbAccount.email,
                        UserStatus = UserStatus.Submitted,
                        UserStatusValue = (int)UserStatus.Submitted,
                        IsConfirmed = false,
                        LastLoggedIn = System.DateTime.Now,
                        Limit = 0,
                        RegistrationMethod = UserRegistrationMethod.Web,
                        SetupPassword = true,
                        SetupSecurityPin = false,
                        Roles = new Collection<Role>()
                        {
                            memberRole
                        },
                        FacebookUser = new FBUser()
                        {
                            Id = Guid.NewGuid(),
                            FBUserID = fbAccount.id,
                            OAuthToken = "",
                            TokenExpiration = System.DateTime.Now.AddDays(1)
                        }
                    });

                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Exception Registering User. {1}", ex.Message));

                }

                if (user == null)
                {
                    ModelState.AddModelError("", "There was an error registering your account.  Please try again.");

                    return View("Index");
                }

                Session["User"] = user;
                Session["UserId"] = user.UserId;

                return RedirectToAction("ValidateMobileDevice");
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
                HttpWebResponse resp = (HttpWebResponse)wr.GetResponse();

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

        private static HttpWebRequest GetWebRequest(string formattedUri)
        {
            // Create the request’s URI.
            Uri serviceUri = new Uri(formattedUri, UriKind.Absolute);

            // Return the HttpWebRequest.
            return (HttpWebRequest)WebRequest.Create(serviceUri);
        }

        public ActionResult ValidateMobileDevice()
        {
            if (Session["UserId"] == null)
                return View("SignIn");

            logger.Log(LogLevel.Info, String.Format("Displaying ValidateMobileDevice View"));

            using (var ctx = new Context())
            {
                var userService = new SocialPayments.DomainServices.UserService(ctx);
                var formattingService = new SocialPayments.DomainServices.FormattingServices();

                var user = userService.GetUserById(Session["UserId"].ToString());

                PaymentModel paymentModel = null;
                if (Session["Payment"] != null)
                    paymentModel = (PaymentModel)Session["Payment"];

                return View(new MobileDeviceVerificationModel()
                {
                    Payment = paymentModel,
                    MobileNumber = formattingService.FormatMobileNumber(user.MobileNumber)
                });
            }
        }
        [HttpPost]
        public ActionResult ValidateMobileDevice(MobileDeviceVerificationModel model)
        {
            using (var ctx = new Context())
            {
                logger.Log(LogLevel.Info, String.Format("Validating Mobile Device {0}", model.VerificationCode));

                if (Session["UserId"] == null)
                    return View("SignIn");

                Guid userId = (Guid)Session["UserId"];
                User user = null;

                try
                {
                    user = ctx.Users.FirstOrDefault(u => u.UserId == userId);
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Exception getting user {0} from database. {1}", userId, ex.Message));
                }

                if (user == null)
                {
                    logger.Log(LogLevel.Error, String.Format("Unable to find user {0}", userId));
                    ModelState.AddModelError("", "Unable to validate your mobile device.  Please try again.");

                    return View(model);
                }
                if (user.MobileVerificationCode1 == null)
                {
                    logger.Log(LogLevel.Error, String.Format("Mobile Verification is null for user {0}", userId));
                    ModelState.AddModelError("", "Unable to validate your mobile device.  Please try again.");

                    return View(model);
                }

                if (!user.MobileVerificationCode1.ToString().Equals(model.VerificationCode.ToString(), StringComparison.Ordinal))
                {
                    logger.Log(LogLevel.Info, String.Format("Incorrect verification code validating mobile device for {0}.", userId));
                    ModelState.AddModelError("", "Unable to validate your mobile device.  Please try again.");

                    return View(model);
                }

                return RedirectToAction("SetupACHAccount");
            }
        }

    }
}
