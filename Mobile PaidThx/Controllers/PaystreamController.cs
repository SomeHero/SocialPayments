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
using Mobile_PaidThx.Services.CustomExceptions;

namespace Mobile_PaidThx.Controllers
{
    public class PaystreamController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
 
        public ActionResult PopupPinSwipe(string paystreamAction, string messageId)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Index", "SignIn");

            var userId = Session["UserId"].ToString();

            var messageService = new Services.UserPayStreamMessageServices();
            var message = messageService.GetMessage(userId, messageId);
            
            Session["Action"] = paystreamAction;
            Session["Message"] = message;

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

            MessageModels.MessageResponse message = (MessageModels.MessageResponse)Session["Message"];
            string paystreamAction = Session["Action"].ToString();

            try
            {
                switch (paystreamAction)
                {
                    case "CancelPayment":
                        paystreamMessageServices.CancelPayment("", message.Id.ToString(), user.userId.ToString(), model.Pincode);
                        break;
                    case "CancelRequest":
                        paystreamMessageServices.CancelRequest("", message.Id.ToString(), user.userId.ToString(), model.Pincode);
                        break;
                    case "AcceptRequest":
                        paystreamMessageServices.AcceptPaymentRequest("", user.userId.ToString(), model.Pincode, user.preferredReceiveAccountId,
                            message.Id.ToString());
                        break;
                    case "RejectRequest":
                        paystreamMessageServices.RejectPaymentRequest("", message.Id.ToString(), user.userId.ToString(), model.Pincode);
                        break;
                    default:
                        throw new Exception(String.Format("Invalid Action {0}", paystreamAction));

                }
            }
            catch (ErrorException ex)
            {
                if (ex.ErrorCode == 1001)
                {
                    Session["User"] = null;
                    Session["UserId"] = null;

                    TempData["Message"] = "This Account is Locked.  Please Sign In to Unlock Account.";

                    return RedirectToAction("Index", "SignIn");
                }

                ModelState.AddModelError("", ex.Message);

                return View(new PaystreamModels.PinSwipeRequestModel()
                {
                    PaystreamAction = paystreamAction,
                    MessageId = message.Id.ToString(),
                    Message = message
                });
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(new PaystreamModels.PinSwipeRequestModel()
                {
                    PaystreamAction = paystreamAction,
                    MessageId = message.Id.ToString(),
                    Message = message
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
        public ActionResult Details(String messageId)
        {
            try
            {
                if (Session["UserId"] == null)
                    return RedirectToAction("Index", "SignIn", null);

                var userId = Session["UserId"].ToString();

                var messageService = new Services.PaystreamMessageServices();
                var message = messageService.GetMessage(userId, messageId);

                return View(new PaystreamModels.PaystreamDetailModel
                {
                    amount = message.amount,
                    comments = message.comments,
                    createDate = message.createDate,
                    direction = message.direction,
                    Id = message.Id,
                    isAcceptable = message.isAcceptable,
                    isCancellable = message.isCancellable,
                    isExpressable = message.isExpressable,
                    isRejectable = message.isRejectable,
                    isRemindable = message.isRemindable,
                    lastUpdatedDate = message.lastUpdatedDate,
                    latitude = message.latitude,
                    longitutde = message.longitutde,
                    messageStatus = message.messageStatus,
                    messageType = message.messageType,
                    recipient = message.recipient,
                    recipientName = message.recipientName,
                    recipientSeen = message.recipientSeen,
                    recipientUri = message.recipientUri,
                    recipientUriType = message.recipientUriType,
                    senderName = message.senderName,
                    senderSeen = message.senderSeen,
                    senderUri = message.senderUri,
                    transactionImageUri = message.transactionImageUri,
                    senderImageUri = message.senderImageUri,
                    recipientImageUri = message.recipientImageUri
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View();
        }

    }
}
