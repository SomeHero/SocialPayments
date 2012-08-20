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

namespace Mobile_PaidThx.Controllers
{
    public class RequestController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        Mobile_PaidThx.Models.RequestMoneyModel savedData = new Models.RequestMoneyModel();
        //
        // GET: /Request/

        public ActionResult Index()
        {
            TempData["DataUrl"] = "data-url=./Request";

            return View(new RequestModels.RequestMoneyModel()
            {
                RecipientUri = (Session["RecipientUri"] != null ? Session["RecipientUri"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = (Session["Comments"] != null ? Session["Comments"].ToString() : "")
            });
        }
        [HttpPost]
        public ActionResult Index(RequestModels.RequestMoneyModel model)
        {
            return RedirectToAction("PopupPinSwipe");
        }
        public ActionResult AddContactRequest()
        {
            if (Session["Friends"] == null)
                Session["Friends"] = new List<FacebookModels.Friend>();

            return View(new RequestModels.AddContactRequestModel()
            {
                Friends = (List<FacebookModels.Friend>)Session["Friends"]
            });
        }
        [HttpPost]
        public ActionResult AddContactrequest(RequestModels.AddContactRequestModel model)
        {
            Session["RecipientUri"] = model.RecipientUri;

            TempData["DataUrl"] = "data-url=./";

            return View("Index", new RequestModels.RequestMoneyModel()
            {
                RecipientUri = (Session["RecipientUri"] != null ? Session["RecipientUri"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = ""
            });
        }
        public ActionResult AmountToRequest()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AmountToRequest(RequestModels.AmountToSendModel model)
        {
            Session["Amount"] = model.Amount;

            TempData["DataUrl"] = "data-url=./";

            return View("Index", new RequestModels.RequestMoneyModel()
            {
                RecipientUri = (Session["RecipientUri"] != null ? Session["RecipientUri"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = ""
            });
        }
        public ActionResult PopupPinswipe()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PopupPinswipe(Mobile_PaidThx.Models.RequestModels.PinSwipeModel model)
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
                    paystreamMessageServices.RequestMoney(_apiKey, userId, "", user.userName, user.preferredPaymentAccountId, recipientUri, model.Pincode,
                        amount, comment, "PaymentRequest", "0", "0", "", "", "");
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

            TempData["DataUrl"] = "data-url=.Paystream";

            Session["RecipientUri"] = null;
            Session["Amount"] = null;
            Session["Comments"] = null;

            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
        }
    }
}
