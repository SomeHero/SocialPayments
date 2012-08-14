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
            Mobile_PaidThx.Models.SendMoneyModel model = new Models.SendMoneyModel
            {
                Amount = 0,
                Comments = null,
                RecipientUri = null
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult SendMoney(SendMoneyModel model)
        {
            logger.Log(LogLevel.Debug, String.Format("Send Money Posted to {0} of {1} with Comments {2}", model.RecipientUri, model.Amount, model.Comments));

            var applicationService = new SocialPayments.DomainServices.ApplicationService();
            var messageService = new SocialPayments.DomainServices.MessageServices();
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
                    var response = paystreamMessageServices.SendMoney(_apiKey, userId, "", user.userName, user.preferredPaymentAccountId, model.RecipientUri, model.Pincode, model.Amount, model.Comments, "Payment", "0", "0", "", "", "");
                    //messageService.AddMessage(_apiKey, user.UserId.ToString(), "", model.RecipientUri, user.PaymentAccounts[0].Id.ToString(), model.Amount, model.Comments, @"Payment");
                    //ctx.Payments.Add(new Payment()
                    //{
                    //    Id = Guid.NewGuid(),
                    //    ApiKey = new Guid(ConfigurationManager.AppSettings["APIKey"]),
                    //    Comments = model.Comments,
                    //    CreateDate = System.DateTime.Now,
                    //    FromAccountId = paymentAccount.Id,
                    //    FromMobileNumber = mobileNumber,
                    //    PaymentAmount = model.Amount,
                    //    PaymentChannelType = PaymentChannelType.Single,
                    //    PaymentDate = System.DateTime.Now,
                    //    PaymentStatus = PaymentStatus.Submitted,
                    //    StandardEntryClass = StandardEntryClass.Web,
                    //    ToMobileNumber = model.RecipientUri
                    //});
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment. {0}", ex.Message));

                    return View(model);
                }
            }
            else
                return View(model);



            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });

        }

        [HttpPost]
        public ActionResult Index(String index)
        {
            Mobile_PaidThx.Models.SendMoneyModel model = new Models.SendMoneyModel();
            if (index != null && index.Length > 0)
            {
                model.Amount = Double.Parse(index);
            }
            else
            {
                model.Amount = 0;
            }
            model.Comments = null;
            model.RecipientUri = null;
            return View(model);
        }

        //
        // GET: /Send/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult AddContactSend()
        {
            return View();
        }

        public ActionResult PopupPinswipe()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PopupPinswipe(Mobile_PaidThx.Models.SendMoneyModel model)
        {
            return View(model);
        }

        [HttpPut]
        public ActionResult Index(Mobile_PaidThx.Models.SendMoneyModel model)
        {
            return View(model);
        }

        [HttpPost]
        public ActionResult SendData(Mobile_PaidThx.Models.SendMoneyModel model)
        {
            return Json(model);
        }

        //
        // GET: /Send/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Send/Create

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
        public ActionResult AmountToSend()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AmountToSend(String id)
        {
            return View("Index");
        }
        //
        // GET: /Send/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Send/Edit/5

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
        // GET: /Send/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Send/Delete/5

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
