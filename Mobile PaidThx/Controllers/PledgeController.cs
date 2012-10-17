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
using Mobile_PaidThx.Services.CustomExceptions;
using Mobile_PaidThx.CustomAttributes;
using System.Web.Security;

namespace Mobile_PaidThx.Controllers
{
    [CustomAuthorize]
    public class PledgeController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";
        //
        // GET: /Send/
        private class PledgeInformation
        {
            public string RecipientId { get; set; }
            public string PledgeToName { get; set; }
            public string PledgeToImageUrl { get; set; }
            public string RecipientName { get; set; }
            public string RecipientUri { get; set; }
            public string RecipientImageUrl { get; set; }
            public double Amount { get; set; }
            public string Comments { get; set; }
        }

        public ActionResult Index()
        {
            var pledgeInformation = (Session["PledgeInformation"] != null ? (PledgeInformation)Session["PledgeInformation"] : new PledgeInformation());

            return View(new PledgeModels.PledgeMoneyModel
            {
                RecipientId = pledgeInformation.RecipientId,
                PledgeToImageUrl = pledgeInformation.PledgeToImageUrl,
                PledgeToName = pledgeInformation.PledgeToName,
                RecipientName = pledgeInformation.RecipientName,
                RecipientUri = pledgeInformation.RecipientUri,
                RecipientImageUri = pledgeInformation.RecipientImageUrl,
                Amount = pledgeInformation.Amount,
                Comments = pledgeInformation.Comments
            });
        }

        [HttpPost]
        public ActionResult Index(PledgeModels.PledgeMoneyModel model)
        {
            ModelState.Clear();

            var pledgeInformation = (Session["PledgeInformation"] != null ? (PledgeInformation)Session["PledgeInformation"] : new PledgeInformation());

            if (String.IsNullOrEmpty(pledgeInformation.RecipientId))
                ModelState.AddModelError("", "Non Profit is required");
            if (String.IsNullOrEmpty(pledgeInformation.RecipientUri))
                ModelState.AddModelError("", "Recipient is required");
            if (pledgeInformation.Amount== 0)
                ModelState.AddModelError("", "Amount must be greater than $0.00");
            pledgeInformation.Comments = model.Comments;

            if (!ModelState.IsValid)
            {
                return View(new PledgeModels.PledgeMoneyModel()
                {
                    RecipientId = pledgeInformation.RecipientId,
                    PledgeToImageUrl = pledgeInformation.PledgeToImageUrl,
                    PledgeToName = pledgeInformation.PledgeToName,
                    RecipientName = pledgeInformation.RecipientName,
                    RecipientUri = pledgeInformation.RecipientUri,
                    RecipientImageUri = pledgeInformation.RecipientImageUrl,
                    Amount = pledgeInformation.Amount,
                    Comments = pledgeInformation.Comments
                });
            }

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

            return View(model);
        }
        [HttpPost]
        public ActionResult AddCause(PledgeModels.AddCauseModel model)
        {
            var pledgeInformation = (Session["PledgeInformation"] != null ? (PledgeInformation)Session["PledgeInformation"] : new PledgeInformation());

            pledgeInformation.RecipientId = model.RecipientId;
            pledgeInformation.PledgeToName = model.PledgeToName;
            pledgeInformation.PledgeToImageUrl = model.PledgeToImageUrl;

            Session["PledgeInformation"] = pledgeInformation;

            return View("Index", new PledgeModels.PledgeMoneyModel()
            {
                RecipientId = pledgeInformation.RecipientId,
                PledgeToImageUrl = pledgeInformation.PledgeToImageUrl,
                PledgeToName = pledgeInformation.PledgeToName,
                RecipientName = pledgeInformation.RecipientName,
                RecipientUri = pledgeInformation.RecipientUri,
                RecipientImageUri = pledgeInformation.RecipientImageUrl,
                Amount = pledgeInformation.Amount,
                Comments = pledgeInformation.Comments
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
            var pledgeInformation = (Session["PledgeInformation"] != null ? (PledgeInformation)Session["PledgeInformation"] : new PledgeInformation());
            
            pledgeInformation.RecipientUri = model.RecipientUri;
            pledgeInformation.RecipientName = model.RecipientName;
            pledgeInformation.RecipientImageUrl = model.RecipientImageUrl;

            Session["PledgeInformation"] = pledgeInformation;

            return View("Index", new PledgeModels.PledgeMoneyModel()
            {
                RecipientId = pledgeInformation.RecipientId,
                PledgeToImageUrl = pledgeInformation.PledgeToImageUrl,
                PledgeToName = pledgeInformation.PledgeToName,
                RecipientName = pledgeInformation.RecipientName,
                RecipientUri = pledgeInformation.RecipientUri,
                RecipientImageUri = pledgeInformation.RecipientImageUrl,
                Amount = pledgeInformation.Amount,
                Comments = pledgeInformation.Comments
            });
        }
        public ActionResult AmountToSend()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AmountToSend(PledgeModels.PledgeMoneyModel model)
        {
            var pledgeInformation = (Session["PledgeInformation"] != null ? (PledgeInformation)Session["PledgeInformation"] : new PledgeInformation());
            pledgeInformation.Amount = model.Amount;

            Session["PledgeInformation"] = pledgeInformation;

            return View("Index", new PledgeModels.PledgeMoneyModel()
            {
                RecipientId = pledgeInformation.RecipientId,
                PledgeToImageUrl = pledgeInformation.PledgeToImageUrl,
                PledgeToName = pledgeInformation.PledgeToName,
                RecipientName = pledgeInformation.RecipientName,
                RecipientUri = pledgeInformation.RecipientUri,
                RecipientImageUri = pledgeInformation.RecipientImageUrl,
                Amount = pledgeInformation.Amount,
                Comments = pledgeInformation.Comments
            });
        }
        public ActionResult PopupPinswipe()
        {
            var pledgeInformation = (Session["PledgeInformation"] != null ? (PledgeInformation)Session["PledgeInformation"] : new PledgeInformation());
  
            return View(new PledgeModels.PinSwipeModel()
            {
                RecipientName = pledgeInformation.RecipientName,
                RecipientUri = pledgeInformation.RecipientUri,
                PledgeToImageUrl = pledgeInformation.PledgeToImageUrl,
                PledgeToName = pledgeInformation.PledgeToName,
                Amount = pledgeInformation.Amount,
                RecipientImageUrl = pledgeInformation.RecipientImageUrl
            });
        }

        [HttpPost]
        public ActionResult PopupPinswipe(Mobile_PaidThx.Models.PledgeModels.PinSwipeModel model)
        {
            //logger.Log(LogLevel.Debug, String.Format("Send Money Posted to {0} of {1} with Comments {2}", model.RecipientUri, model.Amount, model.Comments));

            var userId = Session["UserId"].ToString();
            var pledgeInformation = (Session["PledgeInformation"] != null ? (PledgeInformation)Session["PledgeInformation"] : new PledgeInformation());

            if (ModelState.IsValid)
            {
                try
                {
                    UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

                    var paystreamMessageServices = new PaystreamMessageServices();
                    paystreamMessageServices.AcceptPledge(_apiKey, userId, pledgeInformation.RecipientId,  pledgeInformation.RecipientUri, 
                        pledgeInformation.Amount, pledgeInformation.Comments, "0", "0", "", "", "", model.Pincode);
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

                    return View(model);
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

            Session["PledgeInformation"] = null;

            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
        }
    }
}
