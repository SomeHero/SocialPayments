﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Services;
using System.Web.Routing;
using Mobile_PaidThx.Services.CustomExceptions;
using Mobile_PaidThx.CustomAttributes;
using System.Web.Security;

namespace Mobile_PaidThx.Controllers
{
    [CustomAuthorize]
    public class DonateController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        private class DonateInformation
        {
            public string RecipientId { get; set; }
            public string RecipientName { get; set; }
            public string RecipientImageUrl { get; set; }
            public double Amount { get; set; }
            public string Comments { get; set; }
        }

        public ActionResult Index()
        {

            var donateInformation = (Session["DonateInformation"] != null ? (DonateInformation)Session["DonateInformation"] : new DonateInformation());

            return View(new DonateModels.DonateMoneyModel
            {
                RecipientId = donateInformation.RecipientId,
                RecipientName = donateInformation.RecipientName,
                RecipientImageUrl = donateInformation.RecipientImageUrl,
                Amount = donateInformation.Amount,
                Comments = donateInformation.Comments
            });
        }

        [HttpPost]
        public ActionResult Index(DonateModels.DonateMoneyModel model)
        {
            ModelState.Clear();

            var user = (UserModels.UserResponse)Session["User"];

            if (user.bankAccounts.Count == 0)
            {
                Session["UserSetupReturnUrl"] = "/mobile/Donate/PopupPinSwipe";

                return RedirectToAction("SetupACHAccount", "Register", new RouteValueDictionary() { });
            }

            var donateInformation = (Session["DonateInformation"] != null ? (DonateInformation)Session["DonateInformation"] : new DonateInformation());

            if (String.IsNullOrEmpty(donateInformation.RecipientId))
                ModelState.AddModelError("", "Not Profit is required");
            if (donateInformation.Amount == 0)
                ModelState.AddModelError("", "Amount must be greater than $0.00");

            if (!ModelState.IsValid)
            {
                return View(new DonateModels.DonateMoneyModel()
                {
                    RecipientId = donateInformation.RecipientId,
                    RecipientName = donateInformation.RecipientName,
                    RecipientImageUrl = donateInformation.RecipientImageUrl,
                    Amount = donateInformation.Amount,
                    Comments = donateInformation.Comments
                });
            }

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

            return View(model);
        }
        [HttpPost]
        public ActionResult AddContact(DonateModels.AddContactModel model)
        {
            var donateInformation = (Session["DonateInformation"] != null ? (DonateInformation)Session["DonateInformation"] : new DonateInformation());

            donateInformation.RecipientId = model.RecipientId;
            donateInformation.RecipientName= model.RecipientName;
            donateInformation.RecipientImageUrl = model.RecipientImageUrl;


            Session["DonateInformation"] = donateInformation;

            return View("Index", new DonateModels.DonateMoneyModel()
            {
                RecipientId = donateInformation.RecipientId,
                RecipientName = donateInformation.RecipientName,
                RecipientImageUrl = donateInformation.RecipientImageUrl,
                Amount = donateInformation.Amount,
                Comments = donateInformation.Comments
            });
        }
        public ActionResult AmountToSend()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AmountToSend(DonateModels.SelectAmountModel model)
        {
            var donateInformation = (Session["DonateInformation"] != null ? (DonateInformation)Session["DonateInformation"] : new DonateInformation());

            donateInformation.Amount = model.Amount;

            Session["DonateInformation"] = donateInformation;

            return View("Index", new DonateModels.DonateMoneyModel()
            {
                RecipientId = donateInformation.RecipientId,
                RecipientName = donateInformation.RecipientName,
                RecipientImageUrl = donateInformation.RecipientImageUrl,
                Amount = donateInformation.Amount,
                Comments = donateInformation.Comments
            });
        }
        public ActionResult SetupACHAccount()
        {
            //_logger.Log(LogLevel.Info, String.Format("Displaying SetupACHAccount View"));
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
            return View();
        }
        [HttpPost]
        public ActionResult ConfirmPinSwipe(ConfirmPinSwipeModel model)
        {
            string firstPinCode = Session["PinCode"].ToString();

            if (firstPinCode != model.PinCode)
            {
                TempData["Message"] = "Your confirmation did not match your first PIN, please create a new PIN and try again.";

                return View("SetupPinSwipe");
            }

            Session["PinCode"] = model.PinCode;

            return RedirectToAction("SecurityQuestion");
        }
        public ActionResult SecurityQuestion()
        {
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

        public ActionResult PopupPinswipe()
        {
            var donateInformation = (Session["DonateInformation"] != null ? (DonateInformation)Session["DonateInformation"] : new DonateInformation());

            return View(new DonateModels.PinSwipeModel()
            {
                RecipientId = donateInformation.RecipientId,
                RecipientName = donateInformation.RecipientName,
                RecipientImageUrl = donateInformation.RecipientImageUrl,
                Amount = donateInformation.Amount
            });
        }

        [HttpPost]
        public ActionResult PopupPinswipe(Mobile_PaidThx.Models.DonateModels.PinSwipeModel model)
        {
            //logger.Log(LogLevel.Debug, String.Format("Send Money Posted to {0} of {1} with Comments {2}", model.RecipientUri, model.Amount, model.Comments));
            var userId = Session["UserId"].ToString();

            var donateInformation = (Session["DonateInformation"] != null ? (DonateInformation)Session["DonateInformation"] : new DonateInformation());

            if (ModelState.IsValid)
            {
                try
                {
                    UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

                    var paystreamMessageServices = new PaystreamMessageServices();
                    paystreamMessageServices.SendDonation(_apiKey, userId, donateInformation.RecipientId, user.preferredPaymentAccountId, model.Pincode,
                        donateInformation.Amount, donateInformation.Comments, "0", "0", "", "", "");
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

                    return View(new DonateModels.PinSwipeModel()
                    {
                        RecipientId = donateInformation.RecipientId,
                        RecipientName = donateInformation.RecipientName,
                        RecipientImageUrl = donateInformation.RecipientImageUrl,
                        Amount = donateInformation.Amount
                    });
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Donation. {0}", ex.Message));

                    ModelState.AddModelError("", ex.Message);

                    return View(new DonateModels.PinSwipeModel()
                    {
                        RecipientId = donateInformation.RecipientId,
                        RecipientName = donateInformation.RecipientName,
                        RecipientImageUrl = donateInformation.RecipientImageUrl,
                        Amount = donateInformation.Amount
                    });
                }
            }
            else
            {
                return View(new DonateModels.PinSwipeModel()
                {
                    RecipientId = donateInformation.RecipientId,
                    RecipientName = donateInformation.RecipientName,
                    RecipientImageUrl = donateInformation.RecipientImageUrl,
                    Amount = donateInformation.Amount
                });
            }

            Session["DonateInformation"] = null;

            return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });
        }

       
    }
}
