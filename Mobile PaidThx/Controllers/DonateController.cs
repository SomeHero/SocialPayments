using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Services;
using System.Web.Routing;

namespace Mobile_PaidThx.Controllers
{
    public class DonateController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";
        //
        // GET: /Send/

        public ActionResult Index()
        {
            TempData["DataUrl"] = "data-url=/Donate";

            return View(new DonateModels.DonateMoneyModel
            {
                RecipientId = (Session["RecipientId"] != null ? Session["RecipientId"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
                RecipientImageUrl = (Session["RecipientImageUrl"] != null ? Session["RecipientImageUrl"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = (Session["Comments"] != null ? Session["Comments"].ToString() : "")
            });
        }

        [HttpPost]
        public ActionResult Index(DonateModels.DonateMoneyModel model)
        {
            return RedirectToAction("PopupPinSwipe");
        }

        public ActionResult AddContact()
        {
            var merchantServices = new MerchantServices();
            List<MerchantModels.MerchantResponseModel> merchants = new List<MerchantModels.MerchantResponseModel>();

            try
            {
                merchants = merchantServices.GetMerchants("NonProfits");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, String.Format("Exception Getting Organizations."));

                ModelState.AddModelError("", ex.Message);

                return View("Index");
            }

            var nonProfits = merchants.Select(m => new OrganizationModels.OrganizationModel()
            {
                HasInfo = (m.Listings.Count > 0 ? true : false),
                Id = m.Id.ToString(),
                ImageUri = m.MerchantImageUrl,
                Name = m.Name,
                Slogan = "Slogan goes here"
            }).ToList();

            SortedDictionary<string, List<OrganizationModels.OrganizationModel>> sortedNonProfits = new SortedDictionary<string, List<OrganizationModels.OrganizationModel>>();

            foreach (var nonProfit in nonProfits)
            {
                var firstLetter = nonProfit.Name[0].ToString();

                if (!sortedNonProfits.ContainsKey(firstLetter))
                    sortedNonProfits.Add(firstLetter, new List<OrganizationModels.OrganizationModel>());

                var tempMerchantList = sortedNonProfits[firstLetter];
                tempMerchantList.Add(nonProfit);

            }

            var model = new DonateModels.AddContactModel()
            {
                SortedNonProfits = sortedNonProfits,
                NonProfits = nonProfits,
            };

            TempData["DataUrl"] = "data-url=/Donate/AddContact";

            return View(model);
        }
        [HttpPost]
        public ActionResult AddContact(DonateModels.AddContactModel model)
        {
            Session["RecipientId"] = model.RecipientId;
            Session["RecipientName"] = model.RecipientName;
            Session["RecipientImageUrl"] = model.RecipientImageUrl;

            return View("Index", new DonateModels.DonateMoneyModel()
            {
                RecipientId = (Session["RecipientId"] != null ? Session["RecipientId"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
                RecipientImageUrl = (Session["RecipientImageUrl"] != null ? Session["RecipientImageUrl"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = ""
            });
        }
        public ActionResult AmountToSend()
        {
            TempData["DataUrl"] = "data-url=/Donate/AmountToSend";

            return View();
        }

        [HttpPost]
        public ActionResult AmountToSend(DonateModels.SelectAmountModel model)
        {
            Session["Amount"] = model.Amount;

            TempData["DataUrl"] = "data-url=/Donate";
            
            return View("Index", new DonateModels.DonateMoneyModel()
            {
                RecipientId = (Session["RecipientId"] != null ? Session["RecipientId"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
                RecipientImageUrl = (Session["RecipientImageUrl"] != null ? Session["RecipientImageUrl"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = ""
            });
        }
        public ActionResult PopupPinswipe()
        {
            return View(new DonateModels.PinSwipeModel()
            {
                RecipientId = (Session["RecipientId"] != null ? Session["RecipientId"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
                RecipientImageUrl = (Session["RecipientImageUrl"] != null ? Session["RecipientImageUrl"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
            });
        }

        [HttpPost]
        public ActionResult PopupPinswipe(Mobile_PaidThx.Models.DonateModels.PinSwipeModel model)
        {
            //logger.Log(LogLevel.Debug, String.Format("Send Money Posted to {0} of {1} with Comments {2}", model.RecipientUri, model.Amount, model.Comments));

            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            var userId = Session["UserId"].ToString();

            string recipientId = Session["RecipientId"].ToString();
            double amount = Convert.ToDouble(Session["Amount"]);
            string comment = "";

            if (ModelState.IsValid)
            {
                try
                {
                    UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

                    var paystreamMessageServices = new PaystreamMessageServices();
                    paystreamMessageServices.SendDonation(_apiKey, userId, recipientId, user.preferredPaymentAccountId, model.Pincode,
                        amount, comment, "0", "0", "", "", "");
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Donation. {0}", ex.Message));

                    ModelState.AddModelError("", ex.Message);

                    return View(model);
                }
            }
            else
                return View(model);

            TempData["DataUrl"] = "data-url=/Paystream";

            Session["RecipientId"] = null;
            Session["RecipientName"] = null;
            Session["RecipientImageUrl"] = null;
            Session["Amount"] = null;
            Session["Comments"] = null;

            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
        }

       
    }
}
