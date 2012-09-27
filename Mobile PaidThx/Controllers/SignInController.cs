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
            if (Session["SignInFBState"] == null)
                Session["SignInFBState"] = RandomString(8, false);

            return View(new SignInModels.SignInModel()
            {
                FBState = Session["SignInFBState"].ToString()
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
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);

                    _logger.Log(LogLevel.Error, String.Format("Exception Signing In User. Exception: {0} StackTrace: {1}", ex.Message, ex.StackTrace));

                    return View(new SignInModels.SignInModel()
                    {
                        FBState = Session["SignInFBState"].ToString()
                    });
                }

                if (validateResponse.isLockedOut)
                {
                    Session["UserId"] = validateResponse.userId;

                    TempData["SecurityQuestion"] = validateResponse.securityQuestion;
                    return RedirectToAction("SecurityQuestionChallenge");
                }

                UserModels.UserResponse user;

                try
                {
                    user = userService.GetUser(validateResponse.userId);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);

                    _logger.Log(LogLevel.Error, String.Format("Exception Getting User. Exception: {0}. StackTrace: {1}", ex.Message, ex.StackTrace));

                    return View(new SignInModels.SignInModel()
                    {
                        FBState = Session["SignInFBState"].ToString()
                    });
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

                    return View(new SignInModels.SignInModel()
                    {
                        FBState = Session["SignInFBState"].ToString()
                    });
                }

                var facebookServices = new FacebookServices();
                foreach (var socialNetwork in user.userSocialNetworks)
                {
                    if (socialNetwork.SocialNetwork == "Facebook")
                        Session["Friends"] = facebookServices.GetFriendsList(socialNetwork.SocialNetworkUserToken);

                }

                Session["Application"] = application;
                Session["UserId"] = validateResponse.userId;
                Session["User"] = user;

                if (String.IsNullOrEmpty(user.firstName) || String.IsNullOrEmpty(user.lastName))
                {
                    return RedirectToAction("Personalize", "Register");
                }
                else if (user.bankAccounts.Count() == 0)
                {
                    if (!String.IsNullOrEmpty(user.firstName) && !String.IsNullOrEmpty(user.lastName))
                        TempData["NameOnAccount"] = user.firstName + " " + user.lastName;

                    return RedirectToAction("SetupACHAccount", "Register");
                }

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

            // If we got this far, something failed, redisplay form
            return View(new SignInModels.SignInModel()
            {
                FBState = Session["SignInFBState"].ToString()
            });
        }
        public ActionResult SecurityQuestionChallenge()
        {
            return View(new SignInModels.SecurityQuestionChallengeModel()
            {
                SecurityQuestion = TempData["SecurityQuestion"].ToString()
            });
        }
        [HttpPost]
        public ActionResult SecurityQuestionChallenge(SignInModels.SecurityQuestionChallengeModel model)
        {
            var userSecurityQuestionService = new UserSecurityQuestionService();
            var userService = new UserServices();

            if (Session["UserId"] == null)
                return RedirectToAction("Index");

            var userId = Session["UserId"].ToString();
            bool results = false;
            UserModels.UserResponse user = null;
            try
            {
                results = userSecurityQuestionService.ValidateSecurityQuestion(userId, model.SecurityQuestionAnswer);

                if (!results)
                {
                    ModelState.AddModelError("", "Your answer did not match our records. Try again");

                    return View(model);
                }

                user = userService.GetUser(userId);

                Session["User"] = user;

                return RedirectToAction("Index", "Paystream");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(model);
            }

            return View(model);

        }
        public ActionResult SignInWithFacebook(string state, string code)
        {
            var faceBookServices = new FacebookServices();
            var userServices = new UserServices();

            var redirect = String.Format(fbTokenRedirectURL, "SignIn/SignInWithFacebook/");

            if (Session["SignInFBState"] == null)
            {
                Session["SignInFBState"] = RandomString(8, false);
                ModelState.AddModelError("", "Unable to sign in with Facebook.  Please try again");

                return View("Index", new SignInModels.SignInModel()
                {
                    FBState = Session["SignInFBState"].ToString()
                });
            }

            string fbState = Session["SignInFBState"].ToString();

            if (state != fbState)
            {
                Session["SignInFBState"] = RandomString(8, false);
                ModelState.AddModelError("", "Unable to sign in with Facebook.  Please try again");

                return View("Index",new SignInModels.SignInModel()
                {
                    FBState = Session["SignInFBState"].ToString()
                });
            }

            var fbAccount = faceBookServices.FBauth(code, redirect);

            UserModels.FacebookSignInResponse facebookSignInResponse;
            bool isNewUser = false;

            try
            {
                facebookSignInResponse = userServices.SignInWithFacebook(_apiKey, fbAccount.id, fbAccount.first_name, fbAccount.last_name, fbAccount.email, "", fbAccount.accessToken, System.DateTime.Now.AddDays(30),
                (Session["MessageId"] != null ? Session["MessageId"].ToString() : ""), out isNewUser);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Signing In with Facebook. {0} Stack Trace: {1}", ex.Message, ex.StackTrace));

                ModelState.AddModelError("", ex.Message);

                return View("Index", new JoinModels.JoinModel()
                {
                    UserName = "",
                    FBState = Session["SignInFBState"].ToString(),
                    Message = null
                });
            }

            List<FacebookModels.Friend> friends;

            try
            {
                _logger.Log(LogLevel.Error, String.Format("Getting Facebook Friends. Access Token {0}", fbAccount.accessToken));

                friends = faceBookServices.GetFriendsList(fbAccount.accessToken);
            }
            catch (Exception ex)
            {
                friends = new List<FacebookModels.Friend>();

                _logger.Log(LogLevel.Error, String.Format("Exception Getting Facebook Friends. Access Token {0}. Exception: {1}", fbAccount.accessToken, ex.Message));
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

                return View("Index", new JoinModels.JoinModel()
                {
                    UserName = "",
                    FBState = Session["SignInFBState"].ToString(),
                    Message = null
                });
            }

            Session["UserId"] = facebookSignInResponse.userId;
            Session["User"] = userResponse;
            Session["Friends"] = friends;

            if (isNewUser)
                return RedirectToAction("Personalize", "Register", new RouteValueDictionary() { });
            else
                return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });

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
