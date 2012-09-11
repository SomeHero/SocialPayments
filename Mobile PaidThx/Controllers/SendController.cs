using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using System.Web.Routing;
using Mobile_PaidThx.Services;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Controllers
{
    public class SendController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";
        //
        // GET: /Send/

        public ActionResult Index()
        {
            TempData["DataUrl"] = "data-url=/mobile/Send";

            return View(new SendModels.SendMoneyModel()
            {
                RecipientUri = (Session["RecipientUri"] != null ? Session["RecipientUri"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
                RecipientImageUrl = (Session["RecipientImageUrl"] != null ? Session["RecipientImageUrl"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = (Session["Comments"] != null ? Session["Comments"].ToString() : "")
            });
        }

        [HttpPost]
        public ActionResult Index(SendModels.SendMoneyModel model)
        {
            return RedirectToAction("PopupPinSwipe");

        }
        public ActionResult SetupACHAccount()
        {
            //_logger.Log(LogLevel.Info, String.Format("Displaying SetupACHAccount View"));
            
            TempData["DataUrl"] = "data-url=/mobile/SetupACHAccount";

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
            TempData["DataUrl"] = "data-url=/mobile/SetupPinSwipe";

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
            TempData["DataUrl"] = "data-url=/mobile/ConfirmPinSwipe";
            
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
            TempData["DataUrl"] = "data-url=/mobile/SecurityQuestion";
            
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

        public ActionResult AddContactSend()
        {
            TempData["DataUrl"] = "data-url=/mobile/Send/AddContactSend";
            
            if (Session["Friends"] == null)
                Session["Friends"] = new List<FacebookModels.Friend>();

            SortedDictionary<string, List<FacebookModels.Friend>> sortedContacts = new SortedDictionary<string, List<FacebookModels.Friend>>();

            foreach (var friend in (List<FacebookModels.Friend>)Session["Friends"])
            {
                string firstLetter = "#";
                
                if(friend.name.Length > 0)
                    firstLetter = friend.name[0].ToString();

                if (!sortedContacts.ContainsKey(firstLetter))
                    sortedContacts.Add(firstLetter, new List<FacebookModels.Friend>());

                var tempContactList = sortedContacts[firstLetter];
                tempContactList.Add(friend);

            }

            return View(new SendModels.AddContactSendModel()
            {
                SortedContacts = sortedContacts,
            });
        }
        [HttpPost]
        public ActionResult AddContactSend(SendModels.AddContactSendModel model)
        {
            Session["RecipientUri"] = model.RecipientUri;
            Session["RecipientName"] = model.RecipientName;

            TempData["DataUrl"] = "data-url=/mobile/Send";

            string imageUrl = "";
            if (model.RecipientUri.Substring(0, 3) == "fb_")
                imageUrl = String.Format("http://graph.facebook.com/{0}/picture", model.RecipientUri.Substring(3));

            Session["RecipientImageUrl"] = imageUrl;

            return View("Index", new SendModels.SendMoneyModel()
            {
                RecipientUri = (Session["RecipientUri"] != null ? Session["RecipientUri"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
                RecipientImageUrl = (Session["RecipientImageUrl"] != null ? Session["RecipientImageUrl"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = ""
            });
        }
        public ActionResult AmountToSend()
        {
            TempData["DataUrl"] = "data-url=/mobile/Send/AmountToSend";
            
            return View();
        }

        [HttpPost]
        public ActionResult AmountToSend(SendModels.AmountToSendModel model)
        {
            Session["Amount"] = model.Amount;

            TempData["DataUrl"] = "data-url=/mobile/Send";



            return View("Index", new SendModels.SendMoneyModel()
            {
                RecipientUri = (Session["RecipientUri"] != null ? Session["RecipientUri"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
                RecipientImageUrl = (Session["RecipientImageUrl"] != null ? Session["RecipientImageUrl"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = ""
            });
        }
        public ActionResult PopupPinswipe()
        {
            TempData["DataUrl"] = "data-url=/mobile/Send/PinSwipe";
            
            return View(new SendModels.PinSwipModel()
            {
                RecipientUri = (Session["RecipientUri"] != null ? Session["RecipientUri"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
                RecipientImageUrl = (Session["RecipientImageUrl"] != null ? Session["RecipientImageUrl"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
            });
        }

        [HttpPost]
        public ActionResult PopupPinswipe(Mobile_PaidThx.Models.SendModels.PinSwipModel model)
        {
            //logger.Log(LogLevel.Debug, String.Format("Send Money Posted to {0} of {1} with Comments {2}", model.RecipientUri, model.Amount, model.Comments));

            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            var userId = Session["UserId"].ToString();

            string recipientUri = Session["RecipientUri"].ToString();
            double amount = Convert.ToDouble(Session["Amount"]);
            string comment = "";

            if (ModelState.IsValid)
            {
                try
                {
                    UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
                    var paystreamMessageServices = new PaystreamMessageServices();
                    paystreamMessageServices.SendMoney(_apiKey, userId, "", user.userName, user.preferredPaymentAccountId, recipientUri, model.Pincode,
                        amount, comment, "Payment", "0", "0", "", "", "");
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment. {0}", ex.Message));

                    ModelState.AddModelError("", ex.Message);

                    return View(model);
                }
            }
            else
                return View(model);

            TempData["DataUrl"] = "data-url=/mobile/Paystream";

            Session["RecipientUri"] = null;
            Session["RecipientName"] = null;
            Session["RecipientImageUrl"] = null;
            Session["Amount"] = null;
            Session["Comments"] = null;

            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
        }
    }
}
