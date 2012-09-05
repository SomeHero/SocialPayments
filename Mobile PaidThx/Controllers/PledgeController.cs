using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Services;
using NLog;
using Mobile_PaidThx.Models;

namespace Mobile_PaidThx.Controllers
{
    public class PledgeController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";
        //
        // GET: /Send/

        public ActionResult Index()
        {
            TempData["DataUrl"] = "data-url=/Pledge";

            return View(new PledgeModels.PledgeMoneyModel
            {
                RecipientId = (Session["RecipientId"] != null ? Session["RecipientId"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
                RecipientUri = (Session["RecipientUri"] != null ? Session["RecipientUri"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = (Session["Comments"] != null ? Session["Comments"].ToString() : "")
            });
        }

        [HttpPost]
        public ActionResult Index(PledgeModels.PledgeMoneyModel model)
        {
            return RedirectToAction("PopupPinSwipe");
        }

        public ActionResult AddCause()
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

            var model = new PledgeModels.AddCauseModel
            {
                SortedNonProfits = sortedNonProfits,
                NonProfits = nonProfits,
            };

            TempData["DataUrl"] = "data-url=/Pledge/AddContact";

            return View(model);
        }
        [HttpPost]
        public ActionResult AddCause(PledgeModels.AddCauseModel model)
        {
            Session["RecipientId"] = model.RecipientId;
            Session["RecipientName"] = model.RecipientName;

            TempData["DataUrl"] = "data-url=/Pledge";

            return View("Index", new PledgeModels.PledgeMoneyModel()
            {
                RecipientId = (Session["RecipientId"] != null ? Session["RecipientId"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
                RecipientUri = (Session["RecipientUri"] != null ? Session["RecipientUri"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = ""
            });
        }
        public ActionResult AddContact()
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

            return View(new PledgeModels.AddContactModel()
            {
                SortedContacts = sortedContacts
            });
        }
        [HttpPost]
        public ActionResult AddContact(PledgeModels.AddContactModel model)
        {
            Session["RecipientUri"] = model.RecipientUri;

            TempData["DataUrl"] = "data-url=/Pledge";

            return View("Index", new PledgeModels.PledgeMoneyModel()
            {
                RecipientId = (Session["RecipientId"] != null ? Session["RecipientId"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
                RecipientUri = (Session["RecipientUri"] != null ? Session["RecipientUri"].ToString() : ""),
                Amount = (Session["Amount"] != null ? Convert.ToDouble(Session["Amount"]) : 0),
                Comments = ""
            });
        }
        public ActionResult AmountToSend()
        {
            TempData["DataUrl"] = "data-url=/Pledge/AmountToSend";

            return View();
        }

        [HttpPost]
        public ActionResult AmountToSend(PledgeModels.PledgeMoneyModel model)
        {
            Session["Amount"] = model.Amount;

            TempData["DataUrl"] = "data-url=/Pledge";
            
            return View("Index", new PledgeModels.PledgeMoneyModel()
            {
                RecipientId = (Session["RecipientId"] != null ? Session["RecipientId"].ToString() : ""),
                RecipientName = (Session["RecipientName"] != null ? Session["RecipientName"].ToString() : ""),
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
        public ActionResult PopupPinswipe(Mobile_PaidThx.Models.PledgeModels.PinSwipeModel model)
        {
            //logger.Log(LogLevel.Debug, String.Format("Send Money Posted to {0} of {1} with Comments {2}", model.RecipientUri, model.Amount, model.Comments));

            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            var userId = Session["UserId"].ToString();

            string organizationId = Session["RecipientId"].ToString();
            string recipientUri = Session["RecipientUri"].ToString();
            double amount = Convert.ToDouble(Session["Amount"]);
            string comment = "";

            if (ModelState.IsValid)
            {
                try
                {
                    UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

                    var paystreamMessageServices = new PaystreamMessageServices();
                    paystreamMessageServices.AcceptPledge(_apiKey, organizationId, userId, recipientUri, amount, comment, "0", "0", "", "", "", model.Pincode);
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
            Session["RecipientUri"] = null;
            Session["Amount"] = null;
            Session["Comments"] = null;

            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
        }
    }
}
