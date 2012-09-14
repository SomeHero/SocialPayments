using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using System.Globalization;
using System.Web.Routing;

namespace Mobile_PaidThx.Controllers
{
    public class PaystreamController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
 
        public ActionResult ChooseAmount()
        {
            return PartialView("PartialViews/ChooseAmount");
        }

        public ActionResult ChooseAmountRequest()
        {
            return PartialView("PartialViews/ChooseMoneyRequest");
        }

        public ActionResult SendMoney()
        {
            return PartialView("PartialViews/SendMoneyCopy");
        }

        public ActionResult RequestMoney()
        {
            return PartialView("PartialViews/RequestMoneyCopy");
        }
        public ActionResult PopupPinSwipe()
        {
            return View();
        }
        [HttpPost]
        public ActionResult PopupPinSwipe(Mobile_PaidThx.Models.PaystreamModels.PinSwipeModel model)
        {
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
