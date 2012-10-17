using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using System.Web.Routing;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Services;
using Mobile_PaidThx.Services.CustomExceptions;
using Mobile_PaidThx.CustomAttributes;
using System.Web.Security;

namespace Mobile_PaidThx.Controllers
{
    [CustomAuthorize]
    public class RequestController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        Mobile_PaidThx.Models.RequestMoneyModel savedData = new Models.RequestMoneyModel();
        //
        // GET: /Request/

        private class RequestInformation
        {
            public string RecipientUri { get; set; }
            public string RecipientName { get; set; }
            public string RecipientFirstName { get; set; }
            public string RecipientLastName { get; set; }
            public string RecipientImageUrl { get; set; }
            public double Amount { get; set; }
            public string Comments { get; set; }
        }
        public ActionResult Index()
        {
            var requestInformation = (Session["RequestInformation"] != null ? (RequestInformation)Session["RequestInformation"] : new RequestInformation());

            return View(new RequestModels.RequestMoneyModel()
            {
                RecipientUri = requestInformation.RecipientUri,
                RecipientName = requestInformation.RecipientName,
                RecipientImageUrl = requestInformation.RecipientImageUrl,
                Amount = requestInformation.Amount,
                Comments = requestInformation.Comments
            });
        }
        [HttpPost]
        public ActionResult Index(RequestModels.RequestMoneyModel model)
        {
            ModelState.Clear();

            var user = (UserModels.UserResponse)Session["User"];
            
            var requestInformation = (Session["RequestInformation"] != null ? (RequestInformation)Session["RequestInformation"] : new RequestInformation());

            if (String.IsNullOrEmpty(requestInformation.RecipientUri))
                ModelState.AddModelError("", "Recipient is required");
            if (requestInformation.Amount == 0)
                ModelState.AddModelError("", "Amount must be greater than $0.00");

            requestInformation.Comments = model.Comments;

            if (user.bankAccounts.Count == 0)
            {
                Session["UserSetupReturnUrl"] = "/mobile/Request/PopupPinSwipe";

                return RedirectToAction("SetupACHAccount", "Register", new RouteValueDictionary() { });
            }

            if (!ModelState.IsValid)
            {
                return View(new RequestModels.RequestMoneyModel()
                {
                    RecipientUri = requestInformation.RecipientUri,
                    RecipientName = requestInformation.RecipientName,
                    RecipientImageUrl = requestInformation.RecipientImageUrl,
                    Amount = requestInformation.Amount,
                    Comments = requestInformation.Comments
                });
            }

            return RedirectToAction("PopupPinSwipe");
        }
        public ActionResult AddContactRequest()
        {
            if (Session["Friends"] == null)
                Session["Friends"] = new List<FacebookModels.Friend>();

            SortedDictionary<string, List<FacebookModels.Friend>> sortedContacts = new SortedDictionary<string, List<FacebookModels.Friend>>();

            foreach (var friend in (List<FacebookModels.Friend>)Session["Friends"])
            {
                string firstLetter = "#";

                if (friend.name.Length > 0)
                    firstLetter = friend.name[0].ToString();

                if (!sortedContacts.ContainsKey(firstLetter))
                    sortedContacts.Add(firstLetter, new List<FacebookModels.Friend>());

                var tempContactList = sortedContacts[firstLetter];
                tempContactList.Add(friend);

            }
            return View(new RequestModels.AddContactRequestModel()
            {
                SortedContacts = sortedContacts,
            });
        }
        [HttpPost]
        public ActionResult AddContactrequest(RequestModels.AddContactRequestModel model)
        {
            var requestInformation = (Session["RequestInformation"] != null ? (RequestInformation)Session["RequestInformation"] : new RequestInformation());

            requestInformation.RecipientUri = model.RecipientUri;
            requestInformation.RecipientName = model.RecipientName;

            if (model.RecipientType.ToLower() == "facebook")
            {
                if (model.RecipientName.Contains(' '))
                {
                    requestInformation.RecipientFirstName = model.RecipientName.Substring(0, model.RecipientName.IndexOf(' '));
                    requestInformation.RecipientLastName = model.RecipientName.Substring(model.RecipientName.IndexOf(' ') + 1);
                }
                else
                {
                    requestInformation.RecipientFirstName = model.RecipientName;
                    requestInformation.RecipientLastName = "";
                }
            }

            string imageUrl = "";
            if (model.RecipientUri.Substring(0, 3) == "fb_")
                imageUrl = String.Format("http://graph.facebook.com/{0}/picture", model.RecipientUri.Substring(3));

            requestInformation.RecipientImageUrl = imageUrl;

            Session["RequestInformation"] = requestInformation;

            return View("Index", new RequestModels.RequestMoneyModel()
            {
                RecipientUri = requestInformation.RecipientUri,
                RecipientName = requestInformation.RecipientName,
                RecipientImageUrl = requestInformation.RecipientImageUrl,
                Amount = requestInformation.Amount,
                Comments = requestInformation.Comments
            });
        }
        public ActionResult AmountToRequest()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AmountToRequest(RequestModels.AmountToSendModel model)
        {
            var requestInformation = (Session["RequestInformation"] != null ? (RequestInformation)Session["RequestInformation"] : new RequestInformation());

            requestInformation.Amount = model.Amount;
            Session["RequestInformation"] = requestInformation;

            return View("Index", new RequestModels.RequestMoneyModel()
            {
                RecipientUri = requestInformation.RecipientUri,
                RecipientName = requestInformation.RecipientName,
                RecipientImageUrl = requestInformation.RecipientImageUrl,
                Amount = requestInformation.Amount,
                Comments = requestInformation.Comments
            });
        }
        public ActionResult SetupACHAccount()
        {
            //_logger.Log(LogLevel.Info, String.Format("Displaying SetupACHAccount View"));
           
            return View("SetupACHAccount", new SetupACHAccountModel()
            {
                Payment = null
            });
        }
        [HttpPost]
        public ActionResult SetupACHAccount(SetupACHAccountModel model)
        {
            //_logger.Log(LogLevel.Info, String.Format("Setting Up New ACH Account {0}", model.NameOnAccount));

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
            //_logger.Log(LogLevel.Info, String.Format("Validating Routing Number {0}", routingNumber));

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
                TempData["Message"] = "Confirmation Security Pin Does Not Match Your First Security Pin. Try Again.";

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

            var model = new SecurityQuestionModel()
            {

                SecurityQuestionAnswer = "",
                SecurityQuestionId = 1,
                SecurityQuestions = questions
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult SecurityQuestion(SecurityQuestionModel model)
        {
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
                //_logger.Log(LogLevel.Info, String.Format("Exception Setting Up New ACH Account {0}. Stack Trace {1}", ex.Message, ex.StackTrace));

            }

            UserModels.UserResponse user;

            try
            {
                user = userServices.GetUser(userId);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                //_logger.Log(LogLevel.Error, String.Format("Exception Getting User. Exception: {0}. StackTrace: {1}", ex.Message, ex.StackTrace));

                return View();
            }
            return RedirectToAction("PopupPinSwipe");
        }

        public ActionResult PopupPinswipe()
        {
            var requestInformation = (Session["RequestInformation"] != null ? (RequestInformation)Session["RequestInformation"] : new RequestInformation());

            return View(new RequestModels.PinSwipeModel()
            {
                RecipientUri = requestInformation.RecipientUri,
                RecipientName = requestInformation.RecipientName,
                RecipientImageUrl = requestInformation.RecipientImageUrl,
                Amount = requestInformation.Amount
            });
        }

        [HttpPost]
        public ActionResult PopupPinswipe(Mobile_PaidThx.Models.RequestModels.PinSwipeModel model)
        {
            //logger.Log(LogLevel.Debug, String.Format("Send Money Posted to {0} of {1} with Comments {2}", model.RecipientUri, model.Amount, model.Comments));

            var userId = Session["UserId"].ToString();

            var requestInformation = (Session["RequestInformation"] != null ? (RequestInformation)Session["RequestInformation"] : new RequestInformation());

            if (ModelState.IsValid)
            {
                try
                {
                    UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
                    var paystreamMessageServices = new PaystreamMessageServices();
                    paystreamMessageServices.RequestMoney(_apiKey, userId, "", user.userName, user.preferredPaymentAccountId, requestInformation.RecipientUri, model.Pincode,
                        requestInformation.Amount, requestInformation.Comments, "PaymentRequest", "0", "0", requestInformation.RecipientFirstName, requestInformation.RecipientLastName, requestInformation.RecipientImageUrl,
                        "Standard");
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

                    return View(new RequestModels.PinSwipeModel()
                    {
                        RecipientUri = requestInformation.RecipientUri,
                        RecipientName = requestInformation.RecipientName,
                        RecipientImageUrl = requestInformation.RecipientImageUrl,
                        Amount = requestInformation.Amount
                    });
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment. {0}", ex.Message));

                    ModelState.AddModelError("", ex.Message);

                    return View(new RequestModels.PinSwipeModel()
                    {
                        RecipientUri = requestInformation.RecipientUri,
                        RecipientName = requestInformation.RecipientName,
                        RecipientImageUrl = requestInformation.RecipientImageUrl,
                        Amount = requestInformation.Amount
                    });
                }
            }
            else
            {
                return View(new RequestModels.PinSwipeModel()
                {
                    RecipientUri = requestInformation.RecipientUri,
                    RecipientName = requestInformation.RecipientName,
                    RecipientImageUrl = requestInformation.RecipientImageUrl,
                    Amount = requestInformation.Amount
                });
            }


            Session["RequestInformation"] = null;

            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
        }
    }
}
