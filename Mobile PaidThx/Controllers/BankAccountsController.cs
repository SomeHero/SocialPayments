using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Models;
using NLog;

namespace Mobile_PaidThx.Controllers
{
    public class BankAccountsController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        public ActionResult Index()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            //if (Session["User"] == null)
            // return RedirectToAction("SignIn", "Account", null);

            var model = new BankAccountModels.BankAccountsModel();
            model.PaymentAccounts = new List<BankAccountModel>();
            model.PreferredReceiveAccountId = user.preferredReceiveAccountId.ToString();
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
                    RoutingNumber = paymentAccount.RoutingNumber
                });

            }

            return View(model);
        }

        public ActionResult Add()
        {

            return View(new BankAccountModels.AddPaymentAccountModel()
            {
                AccountNumber = "",
                AccountType = "Checking",
                NameOnAccount = "",
                Nickname = "",
                RoutingNumber = ""
            });
        }

        [HttpPost]
        public ActionResult Add(BankAccountModels.AddPaymentAccountModel model)
        {
            try
            {
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
                var paymentAccountId = bankAccountServices.AddAccount(_apiKey, user.userId.ToString(), bankAccount.Nickname, bankAccount.NameOnAccount, bankAccount.RoutingNumber, bankAccount.AccountNumber,
                     bankAccount.AccountType, model.PinCode);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View();
            }

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
                RoutingNumber = bankAccount.RoutingNumber
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
        [HttpPost]
        public ActionResult EditPopupPinSwipe(BankAccountModels.PinSwipeModel model)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            BankAccountModels.EditPaymentAccountModel bankAccount = (BankAccountModels.EditPaymentAccountModel)Session["EditBankAccount"];

            var bankAccountServices = new Services.UserPaymentAccountServices();

            try
            {
               bankAccountServices.EditAccount(_apiKey, user.userId.ToString(), bankAccount.PaymentAccountId, bankAccount.Nickname,
                   bankAccount.NameOnAccount, bankAccount.RoutingNumber, bankAccount.AccountType);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View();
            }

            user.bankAccounts = bankAccountServices.GetAccounts(_apiKey, user.userId.ToString());

            return RedirectToAction("Index");
        }

    }
}
