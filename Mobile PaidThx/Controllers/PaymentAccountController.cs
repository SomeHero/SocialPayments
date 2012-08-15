using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using Mobile_PaidThx.Models;
using Mobile_PaidThx.Services.ResponseModels;
using Mobile_PaidThx.Services;

namespace Mobile_PaidThx.Controllers
{
    public class PaymentAccountController : Controller
    {
        //
        // GET: /PaymentAccount/ 
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        public ActionResult Index()
        {

            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            //if (Session["User"] == null)
            // return RedirectToAction("SignIn", "Account", null);

            var model = new ListPaymentAccountModel();
            model.PreferredReceiveAccountId = user.preferredReceiveAccountId.ToString();
            model.PreferredSendAccountId = user.preferredPaymentAccountId.ToString();

            foreach (var paymentAccount in user.bankAccounts)
            {
                model.PaymentAccounts.Add(new BankAccountModel()
                {
                    // BankName = paymentAccount.BankName,
                    // BankIconURL = paymentAccount.BankIconUrl,
                    PaymentAccountId = paymentAccount.Id.ToString(),
                    AccountNumber = paymentAccount.AccountNumber,
                    AccountType = paymentAccount.AccountType.ToString(),
                    NameOnAccount = paymentAccount.NameOnAccount,
                    Nickname = paymentAccount.Nickname,
                    RoutingNumber = paymentAccount.RoutingNumber


                });

            }

            return View("Index", model);

        }
        public ActionResult List()
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            var model = new ListPaymentAccountModel();
            model.PreferredReceiveAccountId = user.preferredReceiveAccountId.ToString();
            model.PreferredSendAccountId = user.preferredPaymentAccountId.ToString();

            foreach (var paymentAccount in user.bankAccounts)
            {
                model.PaymentAccounts.Add(new BankAccountModel()
                {
                    BankName = paymentAccount.Nickname,
                    BankIconURL = paymentAccount.BankIconUrl,
                    PaymentAccountId = paymentAccount.Id.ToString(),
                    AccountNumber = paymentAccount.AccountNumber,
                    AccountType = paymentAccount.AccountType.ToString(),
                    NameOnAccount = paymentAccount.NameOnAccount,
                    Nickname = paymentAccount.Nickname,
                    RoutingNumber = paymentAccount.RoutingNumber

                });
            }

            return View("Index", model);

        }
        public PartialViewResult Add()
        {
            return PartialView("PartialViews/Add", new AddPaymentAccountModel()
            {

            });
        }
        [HttpPost]
        public ActionResult Add(AddPaymentAccountModel model)
        {
            if (ModelState.IsValid)
            {
                // if (Session["UserId"] == null)
                // return RedirectToAction("SignIn", "Account", null);
                UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];

                //if (Session["User"] == null)
                // return RedirectToAction("SignIn", "Account", null);
                var paymentAccountTypeId = (int)SocialPayments.Domain.PaymentAccountType.Savings;
                if (model.AccountType.ToLower().Equals("checking"))
                    paymentAccountTypeId = (int)SocialPayments.Domain.PaymentAccountType.Checking;


                var paymentAccount = new SocialPayments.Domain.PaymentAccount()
                {
                    Nickname = model.Nickname,
                    AccountNumber = model.AccountNumber,
                    PaymentAccountTypeId = paymentAccountTypeId,
                    NameOnAccount = model.NameOnAccount,
                    RoutingNumber = model.RoutingNumber,
                    UserId = user.userId,
                    Id = Guid.NewGuid(),
                    CreateDate = System.DateTime.Now,
                    AccountStatus = SocialPayments.Domain.AccountStatusType.Submitted,
                    IsActive = true,
                    BankIconURL = "http://images.PaidThx.com/BankIcons/bank.png",
                    BankName = "Temp"
                };

                if (model.DefaultRecieve != null)
                {
                    if (model.DefaultRecieve.ToLower().Equals("recieve"))
                    {
                        user.preferredReceiveAccountId = paymentAccount.Id.ToString();
                    }
                }
                if (model.DefaultSend != null)
                {
                    if (model.DefaultSend.ToLower().Equals("sending"))
                    {
                        user.preferredPaymentAccountId = paymentAccount.Id.ToString();
                    }
                }
                UserPaymentAccountServices service = new UserPaymentAccountServices();
                service.AddAccount(_apiKey, user.userId.ToString(), model.Nickname, model.NameOnAccount, model.RoutingNumber, model.AccountNumber, model.AccountType, "", "", "");
                return RedirectToAction("List");
            }
            else
            {
                return PartialView("ParitalViews/Add", model);
            }

        }

        public PartialViewResult Edit(string Id)
        {
            //    if (Session["UserId"] == null)
            //    return RedirectToAction("SignIn", "Account", null);
            //if (Session["User"] == null)
            // return RedirectToAction("SignIn", "Account", null);

            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            var paymentAccount = user.bankAccounts.FirstOrDefault(a => a.Id == Id);

            return PartialView("PartialViews/Edit", new EditPaymentAccountModel()
            {
                AccountNumber = paymentAccount.AccountNumber,
                AccountType = paymentAccount.AccountType.ToString(),
                NameOnAccount = paymentAccount.NameOnAccount,
                Nickname = paymentAccount.Nickname,
                RoutingNumber = paymentAccount.RoutingNumber,
                PaymentAccountId = paymentAccount.Id.ToString(),
                AccountTypeOptions = new SelectListItem[] 
                        {
                            new SelectListItem() { Text = "Savings", Value = "Savings" },
                            new SelectListItem() { Text = "Checking", Value= "Checking" }
                        }
            });

        }

        [HttpPost]
        public ActionResult Edit(EditPaymentAccountModel model, string Id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);


            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            var paymentAccount = user.bankAccounts.FirstOrDefault(a => a.Id == Id);


            if (paymentAccount == null)
            {
                ModelState.AddModelError("", "Unable to edit payment account");

                return View(model);
            }

            UserPaymentAccountServices service = new UserPaymentAccountServices();
            service.EditAccount(_apiKey, user.userId.ToString(), Id, model.Nickname, model.NameOnAccount, model.RoutingNumber, model.AccountType);

            return RedirectToAction("List");



        }
        public ActionResult Dialog()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult defaultReceiving(string Id)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            var paymentAccount = user.bankAccounts.FirstOrDefault(a => a.Id == Id);

            UserPaymentAccountServices service = new UserPaymentAccountServices();
            service.SetReceiveAccount(_apiKey, user.userId.ToString(), paymentAccount.Id, "");

            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult defaultSending(string Id, EditPaymentAccountModel model)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            var paymentAccount = user.bankAccounts.FirstOrDefault(a => a.Id == Id);

            UserPaymentAccountServices service = new UserPaymentAccountServices();
            service.SetSendAccount(_apiKey, user.userId.ToString(), paymentAccount.Id, "");

            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult Remove(string Id, EditPaymentAccountModel model)
        {
            UserModels.UserResponse user = (UserModels.UserResponse)Session["User"];
            var paymentAccount = user.bankAccounts.FirstOrDefault(a => a.Id == Id);

            UserPaymentAccountServices service = new UserPaymentAccountServices();
            service.DeleteAccount(_apiKey, user.userId.ToString(), paymentAccount.Id);

            return RedirectToAction("List");
        }
    }

}
