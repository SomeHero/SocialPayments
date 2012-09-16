using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using System.Globalization;
using System.Web.Routing;
using Mobile_PaidThx.Services.ResponseModels;

namespace Mobile_PaidThx.Controllers
{
    public class PaystreamController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
 
        public ActionResult PopupPinSwipe(string paystreamAction, string messageId)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Index", "SignIn");

            Session["Action"] = paystreamAction;
            Session["MessageId"] = messageId;

            var userId = Session["UserId"].ToString();

            var messageService = new Services.UserPayStreamMessageServices();
            var message = messageService.GetMessage(userId, messageId);

            return View(new PaystreamModels.PinSwipeRequestModel()
            {
                PaystreamAction = paystreamAction,
                MessageId = messageId,
                Message = message
            });
        }
        [HttpPost]
        public ActionResult PopupPinSwipe(Mobile_PaidThx.Models.PaystreamModels.PinSwipeModel model)
        {
            var paystreamMessageServices = new Services.PaystreamMessageServices();

            
            if (Session["UserId"] == null)
                return RedirectToAction("Index", "SignIn", null);

            var user = (UserModels.UserResponse)Session["User"];

            string messageId = Session["MessageId"].ToString();
            string paystreamAction = Session["Action"].ToString();

            try
            {
                switch(paystreamAction)
                {
                    case "CancelPayment":
                        paystreamMessageServices.CancelPayment("", messageId);
                        break;
                    case "CancelRequest":
                        paystreamMessageServices.CancelRequest("", messageId);
                        break;
                    case "AcceptRequest":
                        paystreamMessageServices.AcceptPaymentRequest("", user.userId.ToString(), model.Pincode, user.preferredReceiveAccountId,
                            messageId);

                        break;
                    case "RejectRequest":
                        paystreamMessageServices.RejectPaymentRequest("", messageId);
                        break;
                    default:
                        throw new Exception(String.Format("Invalid Action {0}", paystreamAction));

                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(new PaystreamModels.PinSwipeRequestModel()
                {
                    PaystreamAction = paystreamAction,
                    MessageId = messageId
                });
            }

            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
        }
        public ActionResult Index(String searchString)
        {
            TempData["DataUrl"] = "data-url=/mobile/Paystream";

            if (Session["UserId"] == null)
                return RedirectToAction("Index", "SignIn", null);

            var model = new PaystreamModels.PaystreamModel()
            {
                UserId = Session["UserId"].ToString()
            };

            return View(model);
        }

    }
}
