using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using System.Globalization;

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

        public ActionResult Index(String searchString)
        {
            TempData["DataUrl"] = "data-url=/mobile/Paystream";

            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            var userService = new Services.UserServices();
            var userPayStreamServices = new Services.UserPayStreamMessageServices();

            var user = userService.GetUser(Session["UserId"].ToString());

            var model = new PaystreamModels.PaystreamModel()
            {
                ProfileModel = new ProfileModels()
                {
                    FirstName = user.firstName,
                    LastName = user.lastName,
                    AccountType = "Personal",
                    MobileNumber = user.mobileNumber,
                    EmailAddress = user.emailAddress,
                    Address = user.address,
                    City = user.city,
                    State = user.state,
                    Zip = user.zip,
                    SenderName = user.senderName,
                    PaymentAccountsList = new ListPaymentAccountModel()
                    {
                        PaymentAccounts = user.bankAccounts.Select(b => new BankAccountModel() {
                            AccountNumber = b.AccountNumber,
                            AccountType = b.AccountType,
                            BankIconURL = "",
                            BankName = "",
                            NameOnAccount = b.NameOnAccount,
                            Nickname = b.Nickname,
                            RoutingNumber = b.RoutingNumber,
                            PaymentAccountId = b.Id
                        }).ToList(),
                        PreferredReceiveAccountId = user.preferredReceiveAccountId,
                        PreferredSendAccountId = user.preferredReceiveAccountId
                    }
                }
            };

            return View(model);
        }
        private List<PaystreamModels.AlertModel> GetAlerts(Guid userId)
        {
            var results = new List<PaystreamModels.AlertModel>();

            return results;
        }

    }
}
