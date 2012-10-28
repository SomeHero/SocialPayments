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

            if(Request.QueryString["Message"] == "AccountLocked")
                TempData["Message"] = "This Account is Locked.  Please Sign In to Unlock Account.";

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
                    Session["ReturnUrl"] = returnUrl;

                    TempData["SecurityQuestion"] = validateResponse.securityQuestion;
                    return RedirectToAction("SecurityQuestionChallenge");
                }

                return CompleteSignIn(validateResponse.userId, returnUrl);
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

            var userId = Session["UserId"].ToString();
            var returnUrl = (Session["ReturnUrl"] != null ? Session["ReturnUrl"].ToString() : "");

            bool results = false;
           
            try
            {
                results = userSecurityQuestionService.ValidateSecurityQuestion(userId, model.SecurityQuestionAnswer);

                if (!results)
                {
                    ModelState.AddModelError("", "Your answer did not match our records. Try again");

                    return View(model);
                }

                return CompleteSignIn(userId, returnUrl);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(model);
            }

        }
        public ActionResult 
            SignInWithFacebook(string state, string code)
        {
            var faceBookServices = new FacebookServices();
            var userServices = new UserServices();
            var applicationServices = new ApplicationServices();

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

                return View("Index", new SignInModels.SignInModel()
                {
                    FBState = Session["SignInFBState"].ToString()
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

                return View("Index", new SignInModels.SignInModel()
                {
                    FBState = Session["SignInFBState"].ToString()
                });
            }
            
            Session["Friends"] = friends;

            if (userResponse.isLockedOut)
            {
                Session["UserId"] = userResponse.userId;
                Session["ReturnUrl"] = "";

                TempData["SecurityQuestion"] = userResponse.securityQuestion;
                
                return RedirectToAction("SecurityQuestionChallenge");
            }

            return CompleteSignIn(userResponse.userId.ToString(), "");

        }

        private ActionResult CompleteSignIn(string userId, string returnUrl)
        {
            var userService = new UserServices();
            var applicationServices = new ApplicationServices();

            UserModels.UserResponse user;

            try
            {
                user = userService.GetUser(userId);
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

            FormsAuthentication.SetAuthCookie(user.emailAddress, false);

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
            Session["UserId"] = user.userId;
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
                return RedirectToAction("Index", "Dashboard", new RouteValueDictionary() { });
            }
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
