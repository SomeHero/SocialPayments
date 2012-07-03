﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;

using SocialPayments.DataLayer;
using SocialPayments.DomainServices;
using NLog;

namespace Mobile_PaidThx.Controllers
{
    public class PaystreamController : Controller
    {
        private ApplicationService applicationService = new ApplicationService();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private FormattingServices formattingService = new FormattingServices();

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);
            
            using (var ctx = new Context())
            {
                var messageServices = new MessageServices();
                var userId = (Guid)Session["UserId"];
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var alerts = GetAlerts(user.UserId);

                logger.Log(LogLevel.Debug, String.Format("Getting Payment Accounts"));
                
                var messages = messageServices.GetMessages(user.UserId);

                var payments = messages.Select(m => new PaystreamModels.PaymentModel()
                {
                   Amount = m.Amount,
                   RecipientUri = m.RecipientUri,
                   SenderUri = m.SenderUri,
                   TransactionDate = m.CreateDate,
                   TransactionStatus = TransactionStatus.Pending,
                   TransactionType = TransactionType.Deposit,
                   MessageType = (m.MessageType == SocialPayments.Domain.MessageType.Payment ? MessageType.Payment : MessageType.PaymentRequest),
                   Direction = m.Direction
                }).ToList();

                var bankAccounts = new List<BankAccountModel>();

                foreach (var paymentAccount in user.PaymentAccounts)
                {
                    bankAccounts.Add(new BankAccountModel()
                    {
                        PaymentAccountId = paymentAccount.Id.ToString(),
                        AccountNumber = paymentAccount.AccountNumber,
                        AccountType = paymentAccount.AccountType.ToString(),
                        NameOnAccouont = paymentAccount.NameOnAccount,
                        Nickname = "Nickname",
                        RoutingNumber = paymentAccount.RoutingNumber
                    });
                }

                var model = new PaystreamModels.PaystreamModel()
                {
                    AllReceipts = payments,
                    PaymentReceipts = payments.Where(p => p.MessageType == MessageType.Payment).ToList(),
                    RequestReceipts = payments.Where(p => p.MessageType == MessageType.PaymentRequest).ToList(),
                    Alerts = alerts,
                    ProfileModel = new ProfileModels()
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        AccountType = "Personal",
                        MobileNumber = user.MobileNumber,
                        EmailAddress = user.EmailAddress,
                        Address = user.Address,
                        City = user.City,
                        State = user.State,
                        Zip = user.Zip,
                        SenderName = user.SenderName,
                        PaymentAccountsList = new ListPaymentAccountModel()
                        {
                            PaymentAccounts = bankAccounts
                        }
                    }
                };
                logger.Log(LogLevel.Debug, String.Format("Return Paystream View"));

                return View(model);
            }
        }
        private List<PaystreamModels.AlertModel> GetAlerts(Guid userId)
        {
            var results = new List<PaystreamModels.AlertModel>();

            return results;
        }

    }
}