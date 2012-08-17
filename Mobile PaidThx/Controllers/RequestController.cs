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
            return View(savedData);
        }

        [HttpPost]
        public ActionResult Index(String index)
        {
            if (index != null && index.Length > 0)
            {
                savedData.Amount = Double.Parse(index);
            }
            else
            {
                savedData.Amount = 0;
            }

            return View(savedData);
        }

        [HttpPut]
        public ActionResult Index(Mobile_PaidThx.Models.RequestMoneyModel model)
        {
            return View(model);
        }

        [HttpPost]
        public ActionResult RequestMoney(RequestMoneyModel model)
        {
            logger.Log(LogLevel.Debug, String.Format("Payment Request Posted to {0} of {1} with Comments {2}", model.RecipientUri, model.Amount, model.Comments));

            var applicationService = new SocialPayments.DomainServices.ApplicationService();
            var userId = Session["UserId"].ToString();

            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            logger.Log(LogLevel.Debug, String.Format("Found user and payment account"));

            if (ModelState.IsValid)
            {
                try
                {

                    UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
                    var paystreamMessageServices = new PaystreamMessageServices();
                    paystreamMessageServices.RequestMoney(_apiKey, userId, "", user.userName, user.preferredReceiveAccountId, model.RecipientUri, model.Pincode, model.Amount, model.Comments, "PaymentRequest", "0", "0", "", "", "");

                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment Request. {0}", ex.Message));

                    return View(model);
                }
            }
            else
                return View(model);

            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
        }

        [HttpPost]
        public ActionResult RequestData(Mobile_PaidThx.Models.RequestMoneyModel model)
        {
            return Json(model);
        }

        public ActionResult AmountToRequest()
        {
            return View();
        }

        public ActionResult AddContactRequest()
        {
            return View();
        }

        public ActionResult RequestPinswipe()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RequestPinswipe(Mobile_PaidThx.Models.RequestMoneyModel model)
        {
            return View(model);
        }

        //
        // GET: /Request/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Request/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Request/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Request/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Request/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Request/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Request/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
