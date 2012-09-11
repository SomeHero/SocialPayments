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
            var paystreamResponse = userPayStreamServices.GetMessages(Session["UserId"].ToString());
            var sortedPayments = new Dictionary<string, List<PaystreamModels.PaymentModel>>();

            var user = userService.GetUser(Session["UserId"].ToString());

            CultureInfo ciCurr = CultureInfo.CurrentCulture;

            int currentWeekNum = ciCurr.Calendar.GetWeekOfYear(System.DateTime.Now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday);
            int previousWeekNum = ciCurr.Calendar.GetWeekOfYear(System.DateTime.Now.AddDays(-1), CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday);

            string key = "";
            foreach(var payment in paystreamResponse)
            {
                var paymentDate = DateTime.ParseExact(payment.createDate, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture);
                  
                int paymentDateWeekNum =ciCurr.Calendar.GetWeekOfYear(paymentDate, CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday);

                //compare to current date

                //is is today
                if (System.DateTime.Now.Date.Equals(paymentDate.Date))
                {
                    key = "Today";
                    if (!sortedPayments.ContainsKey(key))
                        sortedPayments.Add(key, new List<PaystreamModels.PaymentModel>());
                }

                //is it yesterday
                else if (System.DateTime.Now.Date.AddDays(-1).Equals(paymentDate.Date))
                {
                    key = "Yesterday";
                    if (!sortedPayments.ContainsKey(key))
                        sortedPayments.Add(key, new List<PaystreamModels.PaymentModel>());
                }


                //is it this week
                else if (paymentDateWeekNum  == currentWeekNum)
                {
                    key = "This Week";
                    if (!sortedPayments.ContainsKey(key))
                        sortedPayments.Add(key, new List<PaystreamModels.PaymentModel>());
                }

                //is it last week
                else if (paymentDateWeekNum == previousWeekNum)
                {
                    key = "Last Week";
                    if (!sortedPayments.ContainsKey(key))
                        sortedPayments.Add(key, new List<PaystreamModels.PaymentModel>());
                }


                //is it this month
                else if (System.DateTime.Now.Date.Month.Equals(paymentDate.Date.Month))
                {
                    key = "This Month";
                    if (!sortedPayments.ContainsKey(key))
                        sortedPayments.Add(key, new List<PaystreamModels.PaymentModel>());
                }


                //is it last month
                else if (System.DateTime.Now.Date.AddMonths(-1).Month.Equals(paymentDate.Date.Month))
                {
                    key = "Last Month";
                    if (!sortedPayments.ContainsKey(key))
                        sortedPayments.Add(key, new List<PaystreamModels.PaymentModel>());
                }


                //is it this year
                else if (System.DateTime.Now.Date.Year.Equals(paymentDate.Date.Year))
                {
                    key = "This Year";
                    if (!sortedPayments.ContainsKey(key))
                        sortedPayments.Add(key, new List<PaystreamModels.PaymentModel>());
                }

                //is it last year
                else 
                {
                    key = "Last Year";
                    if (!sortedPayments.ContainsKey(key))
                        sortedPayments.Add(key, new List<PaystreamModels.PaymentModel>());
                }


                sortedPayments[key].Add(new PaystreamModels.PaymentModel()
                {
                    Amount = payment.amount,
                    Comments = payment.comments,
                    Direction = payment.direction,
                    Id = payment.Id.ToString(),
                    MessageType = payment.messageType,
                    RecipientUri = payment.recipientUri,
                    RecipientName = payment.recipientName,
                    SenderUri = payment.senderUri,
                    SenderName = payment.senderName,
                    TransactionDate = paymentDate,
                    TransactionImageUri = payment.transactionImageUri,
                    TransactionStatus = payment.messageStatus,
                    TransactionType = payment.messageType
                });
            }

            var model = new PaystreamModels.PaystreamModel()
            {
                SortedPayments =sortedPayments,
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
