using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;

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
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            var userService = new Services.UserServices();
            var userPayStreamServices = new Services.UserPayStreamMessageServices();
            var paystreamResponse = userPayStreamServices.GetMessages(Session["UserId"].ToString());

            var user = userService.GetUser(Session["UserId"].ToString());

            var payments = paystreamResponse.Select(p => new PaystreamModels.PaymentModel()
            {
                Amount = p.amount,
                Comments = p.comments,
                Direction = p.direction,
                Id = p.Id.ToString(),
                MessageType = p.messageType,
                RecipientUri = p.recipientUri,
                SenderUri = p.senderUri,
                TransactionDate = System.DateTime.Now, //p.createDate,
                TransactionImageUri = p.transactionImageUri,
                TransactionStatus = p.messageStatus,
                TransactionType = p.messageType
            }).ToList();

            var model = new PaystreamModels.PaystreamModel()
            {
                AllReceipts = payments,
                PaymentReceipts = payments.Where(p => p.Direction == "Out" && p.MessageType == "Payment").ToList(),
                RequestReceipts = payments.Where(p => p.Direction == "In" && p.MessageType ==  "Payment").ToList(),
                Alerts = payments.Where(p => p.MessageType != "Payment").ToList(),
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
                            NameOnAccouont = b.NameOnAccount,
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
