﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Models;
using NLog;
using Mobile_PaidThx.Services;
using Mobile_PaidThx.Services.CustomExceptions;
using Mobile_PaidThx.CustomAttributes;
using System.Web.Security;

namespace Mobile_PaidThx.Controllers
{
    [CustomAuthorize]
    public class BankAccountsController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        public ActionResult Index()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            var model = new BankAccountModels.BankAccountsModel();
            model.PaymentAccounts = new List<BankAccountModel>();
            if(!String.IsNullOrEmpty(user.preferredPaymentAccountId))
                model.PreferredReceiveAccountId = user.preferredReceiveAccountId.ToString();
            if(!String.IsNullOrEmpty(user.preferredReceiveAccountId))
                model.PreferredSendAccountId = user.preferredPaymentAccountId.ToString();

            foreach (var paymentAccount in user.bankAccounts)
            {
                model.PaymentAccounts.Add(new BankAccountModel()
                {
                    // BankName = paymentAccount.BankName,
                    BankIconURL = paymentAccount.BankIconUrl,
                    PaymentAccountId = paymentAccount.Id.ToString(),
                    AccountNumber = paymentAccount.AccountNumber,
                    AccountType = paymentAccount.AccountType.ToString(),
                    NameOnAccount = paymentAccount.NameOnAccount,
                    Nickname = paymentAccount.Nickname,
                    RoutingNumber = paymentAccount.RoutingNumber,
                    Status = paymentAccount.Status
                });

            }

            return View(model);
        }

        public ActionResult Add()
        {
            var user = (UserModels.UserResponse)Session["User"];

            string nameOnAccount = "";
            if (!String.IsNullOrEmpty(user.firstName) || !String.IsNullOrEmpty(user.lastName))
                nameOnAccount = user.firstName + " " + user.lastName;

            return View(new BankAccountModels.AddPaymentAccountModel()
            {
                AccountNumber = "",
                AccountType = "Checking",
                NameOnAccount = nameOnAccount,
                Nickname = "",
                RoutingNumber = ""
            });
        }

        [HttpPost]
        public ActionResult Add(BankAccountModels.AddPaymentAccountModel model)
        {
            try
            {
                var user = (UserModels.UserResponse)Session["User"];

                var routingNumberServices = new RoutingNumberServices();

                if (!routingNumberServices.ValidateRoutingNumber(model.RoutingNumber))
                {
                    ModelState.AddModelError("RoutingNumber", "Invalid Routing Number.  Please check your Bank's Routing Number and Try Again");

                    return View(model);
                }

               
                if (!user.setupSecurityPin)
                {
                    Session["UserSetupReturnUrl"] = "/mobile/BankAccounts/SetupACHAccountComplete";
                    Session["ACHAccountModel"] = new SetupACHAccountModel()
                    {
                        AccountNumber = model.AccountNumber,
                        AccountType = model.AccountType,
                        ConfirmAccountNumber = model.AccountNumber,
                        NameOnAccount = model.NameOnAccount,
                        Payment = null,
                        RoutingNumber = model.RoutingNumber
                    };
                    return RedirectToAction("SetupPinSwipe", "Register");
                }

                Session["NewBankAccount"] = model;


                return RedirectToAction("AddPopUpPinswipe");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult AddPopupPinswipe()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddPopupPinSwipe(BankAccountModels.PinSwipeModel model)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            BankAccountModels.AddPaymentAccountModel bankAccount = (BankAccountModels.AddPaymentAccountModel)Session["NewBankAccount"];
            var bankAccountServices = new Services.UserPaymentAccountServices();

            try
            {
                bankAccountServices.AddAccount(_apiKey, user.userId.ToString(), bankAccount.Nickname, bankAccount.NameOnAccount, bankAccount.RoutingNumber, bankAccount.AccountNumber,
                     bankAccount.AccountType, model.PinCode);

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

                return View();

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View();
            }

            user.bankAccounts = bankAccountServices.GetAccounts(_apiKey, user.userId.ToString());
            TempData["Success"] = String.Format("added");

            return RedirectToAction("Index");
        }
        public ActionResult SetupACHAccountComplete()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            var bankAccountServices = new Services.UserPaymentAccountServices();
            
            user.bankAccounts = bankAccountServices.GetAccounts(_apiKey, user.userId.ToString());

            return RedirectToAction("Index");
        }
        public ActionResult Edit(string id)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

            var bankAccount = user.bankAccounts.FirstOrDefault(b => b.Id == id);

            if (bankAccount == null)
                return RedirectToAction("BankAccounts");

            return View(new BankAccountModels.EditPaymentAccountModel()
            {
                AccountNumber = bankAccount.AccountNumber,
                AccountType = bankAccount.AccountType,
                NameOnAccount = bankAccount.NameOnAccount,
                Nickname = bankAccount.Nickname,
                PaymentAccountId = bankAccount.Id,
                RoutingNumber = bankAccount.RoutingNumber,
                AccountTypeOptions = new SelectList(new Dictionary<string, string>() {
                    { "Checking", "Checking" },
                    { "Savings", "Savings" }
                }, "Key", "Value")
            });
        }

        [HttpPost]
        public ActionResult Edit(BankAccountModels.EditPaymentAccountModel model)
        {
 
            try
            {
                Session["EditBankAccount"] = model;

                return RedirectToAction("EditPopUpPinswipe");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult EditPopupPinswipe()
        {
            return View();
        }

        public ActionResult Delete(BankAccountModels.DeletePaymentAccountModel model)
        {
            try
            {
                Session["DeleteBankAccount"] = model.PaymentAccountId;

                return RedirectToAction("DeletePopUpPinswipe");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult DeletePopupPinSwipe()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DeletePopupPinSwipe(BankAccountModels.PinSwipeModel model)
        {

            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            string paymentAccountId = Session["DeleteBankAccount"].ToString();

            var bankAccountServices = new Services.UserPaymentAccountServices();

            try
            {
                bankAccountServices.DeleteAccount(_apiKey, user.userId.ToString(), paymentAccountId, model.PinCode);

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

                return View();

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View();
            }

            user.bankAccounts = bankAccountServices.GetAccounts(_apiKey, user.userId.ToString());

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult EditPopupPinSwipe(BankAccountModels.PinSwipeModel model)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            BankAccountModels.EditPaymentAccountModel bankAccount = (BankAccountModels.EditPaymentAccountModel)Session["EditBankAccount"];

            var bankAccountServices = new Services.UserPaymentAccountServices();

            try
            {
               bankAccountServices.EditAccount(_apiKey, user.userId.ToString(), bankAccount.PaymentAccountId, bankAccount.Nickname,
                   bankAccount.NameOnAccount, bankAccount.RoutingNumber, bankAccount.AccountType, model.PinCode);

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

                return View();

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View();
            }

            user.bankAccounts = bankAccountServices.GetAccounts(_apiKey, user.userId.ToString());

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult SetPreferredSendAccount(BankAccountModels.SetPreferredSendAccountModel model)
        {
            Session["ChangedPreferredSendAccount"] = model.PaymentAccountId;

            return RedirectToAction("SetPreferredSendAccountPinSwipe");
        }
        public ActionResult SetPreferredSendAccountPinSwipe()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SetPreferredSendAccountPinSwipe(BankAccountModels.PinSwipeModel model)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            
            var bankAccountServices = new Services.UserPaymentAccountServices();

            try
            {
                bankAccountServices.SetSendAccount(_apiKey, user.userId.ToString(), Session["ChangedPreferredSendAccount"].ToString(), model.PinCode);
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

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View();
            }

            user.preferredPaymentAccountId = Session["ChangedPreferredSendAccount"].ToString();

            Session["ChangedPreferredSendAccount"] = null;

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult SetPreferredReceiveAccount(BankAccountModels.SetPreferredReceiveAccountModel model)
        {
            Session["ChangedPreferredReceiveAccount"] = model.PaymentAccountId;

            return RedirectToAction("SetPreferredReceiveAccountPinSwipe");
        }
        public ActionResult SetPreferredReceiveAccountPinSwipe()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SetPreferredReceiveAccountPinSwipe(BankAccountModels.PinSwipeModel model)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
           
            var bankAccountServices = new Services.UserPaymentAccountServices();

            try
            {
                bankAccountServices.SetReceiveAccount(_apiKey, user.userId.ToString(), Session["ChangedPreferredReceiveAccount"].ToString(), model.PinCode);
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

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View();
            }

            user.preferredReceiveAccountId = Session["ChangedPreferredReceiveAccount"].ToString();

            Session["ChangedPreferredReceiveAccount"] = null;

            return RedirectToAction("Index");
        }
        public ActionResult VerifyAccount(string id)
        {

            return View(new BankAccountModels.VerifyAccountModel()
            {
                Amount1 = "",
                Amount2 = ""
            });
        }
        [HttpPost]
        public ActionResult VerifyAccount(string id, BankAccountModels.VerifyAccountModel model)
        {
             UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
           
            var userPaymentAccountServices = new UserPaymentAccountServices();

            double amount1;
            double amount2;

            Double.TryParse(model.Amount1, out amount1);
            Double.TryParse(model.Amount2, out amount2);

            try
            {
                userPaymentAccountServices.VerifyACHAccount(user.userId.ToString(), id, amount1, amount2);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(model);
            }

            return RedirectToAction("Index");
        }
    }
}
