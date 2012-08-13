using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialPayments.DataLayer;
using Mobile_PaidThx.Models;
using NLog;
using SocialPayments.Domain;
using SocialPayments.DomainServices;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Mobile_PaidThx.Controllers.Base;
using System.Configuration;
using System.Web.Helpers;
using System.Web.Script.Serialization;
using System.Web.Routing;

namespace Mobile_PaidThx.Controllers
{
    public class ProfileController : PaidThxBaseController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        public ActionResult Index()
        {
            logger.Log(LogLevel.Info, String.Format("Displaying Profile View"));

            using (var ctx = new Context())
            {

                if (Session["UserId"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var userId = (Guid)Session["UserId"];
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var model = new ProfileModels()
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
                    SenderName = user.SenderName
                };



                var paymentAccountId = user.PaymentAccounts[0].Id;

                logger.Log(LogLevel.Info, paymentAccountId);

                var messages = ctx.Messages
                    .Where(m => m.SenderId == user.UserId || m.RecipientId.Value == user.UserId)
                    .OrderByDescending(m => m.CreateDate)
                    .ToList<Message>();

                var transactionStatus = Mobile_PaidThx.Models.TransactionStatus.Submitted;

                logger.Log(LogLevel.Info, messages.Count());

                try
                {

                    foreach (var transaction in messages)
                    {
                        model.TransactionReceipts.Add(new PaystreamModels.PaymentModel()
                        {
                            Id = transaction.Id.ToString(),
                            Amount = transaction.Amount,
                            SenderUri = transaction.SenderUri,
                            TransactionDate = transaction.CreateDate,
                            TransactionType = Mobile_PaidThx.Models.TransactionType.Deposit,
                            TransactionStatus = transactionStatus,
                            TransactionImageUri = transaction.TransactionImageUrl,
                            Comments = transaction.Comments
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Info, String.Format("Unhandled Exception {0}", ex.Message));
                    var innerException = ex.InnerException;

                    while (innerException != null)
                    {
                        logger.Log(LogLevel.Info, String.Format("Unhandled Exception {0}", innerException.Message));
                        innerException = innerException.InnerException;
                    }

                }

                return View(model);
            }
        }
        [HttpPost]
        public ActionResult Index(UpdateProfileModel model)
        {
            logger.Log(LogLevel.Info, String.Format("Updating Profile"));

            using (var ctx = new Context())
            {
                var userId = (Guid)Session["UserId"];

                if (Session["UserId"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);


                if (user == null)
                {
                    logger.Log(LogLevel.Error, String.Format("Invalid User."));

                    return RedirectToAction("SignIn", "Account", null);
                }

                var editUser = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (editUser == null)
                {
                    logger.Log(LogLevel.Error, String.Format("Invalid User."));

                    return RedirectToAction("SignIn", "Account", null);
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.SenderName = model.SenderName;
                user.Address = model.Address;
                user.City = model.City;
                user.State = model.State;
                user.Zip = model.Zip;

                ctx.SaveChanges();

                Session["User"] = editUser;

                return RedirectToAction("Index");
            }
        }

        public ActionResult PayStream(PaymentAttributes paymentAttributes)
        {
            using (var ctx = new Context())
            {
                paymentAttributes = new PaymentAttributes()
                {
                    Complete = true,
                    Pending = true,
                    Credits = true,
                    Debits = true
                };
                logger.Log(LogLevel.Info, String.Format("Displaying Profile View"));

                if (Session["UserId"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var userId = (Guid)Session["UserId"];
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var model = new ProfileModels()
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
                    SenderName = user.SenderName
                };

                if (user.PaymentAccounts.Count > 0)
                {

                    var paymentAccountId = user.PaymentAccounts[0].Id;

                    logger.Log(LogLevel.Info, paymentAccountId);

                    var messages = ctx.Messages
                        .Where(m => m.SenderId == user.UserId || m.RecipientId.Value == user.UserId)
                        .OrderByDescending(m => m.CreateDate)
                        .ToList<Message>();

                    try
                    {
                        foreach (var transaction in messages)
                        {

                            model.TransactionReceipts.Add(new PaystreamModels.PaymentModel()
                            {
                                Id = transaction.Id.ToString(),
                                Amount = transaction.Amount,
                                SenderUri = transaction.SenderUri,
                                TransactionDate = transaction.CreateDate,
                                TransactionType = Mobile_PaidThx.Models.TransactionType.Withdrawal,
                                TransactionStatus = Models.TransactionStatus.Pending,
                                TransactionImageUri = transaction.TransactionImageUrl,
                                Comments = transaction.Comments
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Info, String.Format("Unhandled Exception {0}", ex.Message));
                        var innerException = ex.InnerException;

                        while (innerException != null)
                        {
                            logger.Log(LogLevel.Info, String.Format("Unhandled Exception {0}", innerException.Message));
                            innerException = innerException.InnerException;
                        }

                    }
                }

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult UpdatePayStreamRequests(PaymentAttributes model)
        {
            logger.Log(LogLevel.Debug, String.Format("Updating PayStream"));

            using (var ctx = new Context())
            {

                var userId = (Guid)Session["UserId"];

                if (Session["UserId"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var messageServices = new MessageServices();
                var messages = messageServices.GetMessages(user.UserId);

                var payments = messages.Select(m => new PaystreamModels.PaymentModel()
                {
                    Id = m.Id.ToString(),
                    Amount = m.Amount,
                    RecipientUri = m.RecipientUri,
                    SenderUri = m.SenderUri,
                    TransactionDate = m.CreateDate,
                    TransactionStatus = Models.TransactionStatus.Pending,
                    TransactionType = Models.TransactionType.Deposit,
                    MessageType = (m.MessageType == SocialPayments.Domain.MessageType.Payment ? Models.MessageType.Payment : Models.MessageType.PaymentRequest),
                    Direction = m.Direction,
                    TransactionImageUri = m.TransactionImageUrl,
                    Comments = m.Comments
                }).ToList();

                payments = payments.Where(p => p.Direction == "In" && p.MessageType == Models.MessageType.Payment).ToList();

                if (!String.IsNullOrEmpty(model.OtherUri))
                {
                    payments = payments.Where(p => p.RecipientUri.ToUpper().Contains(model.OtherUri.ToUpper()) || p.SenderUri.ToUpper().Contains(model.OtherUri.ToUpper())).ToList();
                }

                if (model.Debits && !model.Credits) // sent
                {
                    payments = payments.Where(p => p.Direction == "Out").ToList();
                }

                if (model.Credits && !model.Debits) // received
                {
                    payments = payments.Where(p => p.Direction == "In").ToList();
                }

                if (model.Complete && !model.Pending) // filter only completed
                {
                    payments = payments.Where(p => p.TransactionStatus == Models.TransactionStatus.Complete).ToList();
                }

                if (!model.Complete && model.Pending)
                {
                    payments = payments.Where(p => p.TransactionStatus == Models.TransactionStatus.Pending).ToList();
                }

                logger.Log(LogLevel.Info, "Serializing Transaction Receipts");
                return Json(payments);
            }

        }

        [HttpPost]
        public ActionResult UpdatePayStreamPayments(PaymentAttributes model)
        {
            logger.Log(LogLevel.Debug, String.Format("Updating PayStream"));

            using (var ctx = new Context())
            {

                var userId = (Guid)Session["UserId"];

                if (Session["UserId"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var messageServices = new MessageServices();
                var messages = messageServices.GetMessages(user.UserId);

                var payments = messages.Select(m => new PaystreamModels.PaymentModel()
                {
                    Id = m.Id.ToString(),
                    Amount = m.Amount,
                    RecipientUri = m.RecipientUri,
                    SenderUri = m.SenderUri,
                    TransactionDate = m.CreateDate,
                    TransactionStatus = Models.TransactionStatus.Pending,
                    TransactionType = Models.TransactionType.Deposit,
                    MessageType = (m.MessageType == SocialPayments.Domain.MessageType.Payment ? Models.MessageType.Payment : Models.MessageType.PaymentRequest),
                    Direction = m.Direction,
                    TransactionImageUri = m.TransactionImageUrl,
                    Comments = m.Comments
                }).ToList();

                payments = payments.Where(p => p.Direction == "Out" && p.MessageType == Models.MessageType.Payment).ToList();

                if (!String.IsNullOrEmpty(model.OtherUri))
                {
                    payments = payments.Where(p => p.RecipientUri.ToUpper().Contains(model.OtherUri.ToUpper()) || p.SenderUri.ToUpper().Contains(model.OtherUri.ToUpper())).ToList();
                }

                if (model.Debits && !model.Credits) // sent
                {
                    payments = payments.Where(p => p.Direction == "Out").ToList();
                }

                if (model.Credits && !model.Debits) // received
                {
                    payments = payments.Where(p => p.Direction == "In").ToList();
                }

                if (model.Complete && !model.Pending) // filter only completed
                {
                    payments = payments.Where(p => p.TransactionStatus == Models.TransactionStatus.Complete).ToList();
                }

                if (!model.Complete && model.Pending)
                {
                    payments = payments.Where(p => p.TransactionStatus == Models.TransactionStatus.Pending).ToList();
                }

                logger.Log(LogLevel.Info, "Serializing Transaction Receipts");
                return Json(payments);
            }

        }

        [HttpPost]
        public ActionResult UpdatePayStreamAlerts(PaymentAttributes model)
        {
            logger.Log(LogLevel.Debug, String.Format("Updating PayStream"));

            using (var ctx = new Context())
            {

                var userId = (Guid)Session["UserId"];

                if (Session["UserId"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var messageServices = new MessageServices();
                var messages = messageServices.GetMessages(user.UserId);

                var payments = messages.Select(m => new PaystreamModels.PaymentModel()
                {
                    Id = m.Id.ToString(),
                    Amount = m.Amount,
                    RecipientUri = m.RecipientUri,
                    SenderUri = m.SenderUri,
                    TransactionDate = m.CreateDate,
                    TransactionStatus = Models.TransactionStatus.Pending,
                    TransactionType = Models.TransactionType.Deposit,
                    MessageType = (m.MessageType == SocialPayments.Domain.MessageType.Payment ? Models.MessageType.Payment : Models.MessageType.PaymentRequest),
                    Direction = m.Direction,
                    TransactionImageUri = m.TransactionImageUrl,
                    Comments = m.Comments
                }).ToList();

                payments = payments.Where(p => p.MessageType == Models.MessageType.PaymentRequest).ToList();

                if (!String.IsNullOrEmpty(model.OtherUri))
                {
                    payments = payments.Where(p => p.RecipientUri.ToUpper().Contains(model.OtherUri.ToUpper()) || p.SenderUri.ToUpper().Contains(model.OtherUri.ToUpper())).ToList();
                }

                if (model.Debits && !model.Credits) // sent
                {
                    payments = payments.Where(p => p.Direction == "Out").ToList();
                }

                if (model.Credits && !model.Debits) // received
                {
                    payments = payments.Where(p => p.Direction == "In").ToList();
                }

                if (model.Complete && !model.Pending) // filter only completed
                {
                    payments = payments.Where(p => p.TransactionStatus == Models.TransactionStatus.Complete).ToList();
                }

                if (!model.Complete && model.Pending)
                {
                    payments = payments.Where(p => p.TransactionStatus == Models.TransactionStatus.Pending).ToList();
                }

                logger.Log(LogLevel.Info, "Serializing Transaction Receipts");
                return Json(payments);
            }

        }

        public ActionResult UpdatePayStreamDialog(string id)
        {
            logger.Log(LogLevel.Debug, String.Format("Updating PayStream"));

            using (var ctx = new Context())
            {

                var userId = (Guid)Session["UserId"];

                if (Session["UserId"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var messageServices = new MessageServices();
                var message = messageServices.GetMessage(id);

                message.Direction = "In";
                if (message.Sender.UserId == user.UserId)
                    message.Direction = "Out";

                var messageType = Models.MessageType.Payment;
                if (message.MessageType == SocialPayments.Domain.MessageType.PaymentRequest)
                    messageType = Models.MessageType.PaymentRequest;

                PaystreamModels.PaymentModel reference = new PaystreamModels.PaymentModel()
                {
                    Amount = message.Amount,
                    Direction = message.Direction,
                    MessageType = messageType,
                    Comments = message.Comments,
                    RecipientUri = message.RecipientUri,
                    SenderUri = message.SenderUri,
                    TransactionDate = message.CreateDate,
                    TransactionStatus = Models.TransactionStatus.Pending,
                    TransactionType = Models.TransactionType.Deposit,
                    TransactionImageUri = message.TransactionImageUrl
                };
                
                return Json(reference, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdatePayStream(PaymentAttributes model)
        {
            logger.Log(LogLevel.Debug, String.Format("Updating PayStream"));

            using (var ctx = new Context())
            {

                var userId = (Guid)Session["UserId"];

                if (Session["UserId"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var messageServices = new MessageServices();
                var messages = messageServices.GetMessages(user.UserId);

                var payments = messages.Select(m => new PaystreamModels.PaymentModel()
                {
                    Id = m.Id.ToString(),
                    Amount = m.Amount,
                    RecipientUri = m.RecipientUri,
                    SenderUri = m.SenderUri,
                    TransactionDate = m.CreateDate,
                    TransactionStatus = Models.TransactionStatus.Pending,
                    TransactionType = Models.TransactionType.Deposit,
                    MessageType = (m.MessageType == SocialPayments.Domain.MessageType.Payment ? Models.MessageType.Payment : Models.MessageType.PaymentRequest),
                    Direction = m.Direction,
                    TransactionImageUri = m.TransactionImageUrl,
                    Comments = m.Comments
                }).ToList();

                if (!String.IsNullOrEmpty(model.OtherUri))
                {
                    payments = payments.Where(p => p.RecipientUri.ToUpper().Contains(model.OtherUri.ToUpper()) || p.SenderUri.ToUpper().Contains(model.OtherUri.ToUpper())).ToList();
                }

                if (model.Debits) // sent
                {
                    payments = payments.Where(p => p.Direction == "Out").ToList();
                }

                if (model.Credits) // received
                {
                    payments = payments.Where(p => p.Direction == "In").ToList();
                }

                if (model.Complete && !model.Pending) // filter only completed
                {
                    payments = payments.Where(p => p.TransactionStatus == Models.TransactionStatus.Complete).ToList();
                }

                if (!model.Complete && model.Pending)
                {
                    payments = payments.Where(p => p.TransactionStatus == Models.TransactionStatus.Pending).ToList();
                }

                logger.Log(LogLevel.Info, "Serializing Transaction Receipts");
                return Json(payments);
            }

        }
        public ActionResult SendMoney()
        {
            logger.Log(LogLevel.Debug, String.Format("Display SendMoney View"));

            using (var ctx = new Context())
            {

                var userId = (Guid)Session["UserId"];

                if (Session["UserId"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                return View();
            }
        }
        [HttpPost]
        public ActionResult SendMoney(SendMoneyModel model)
        {
            logger.Log(LogLevel.Debug, String.Format("Send Money Posted to {0} of {1} with Comments {2}", model.RecipientUri, model.Amount, model.Comments));

            var 
            using (var ctx = new Context())
            {
                var applicationService = new SocialPayments.DomainServices.ApplicationService();
                var messageService = new SocialPayments.DomainServices.MessageServices();
                var userId = (Guid)Session["UserId"];

                if (Session["UserId"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                if (user.PaymentAccounts == null || user.PaymentAccounts.Count == 0)
                {
                    logger.Log(LogLevel.Error, String.Format("Invalid Payment Account"));

                    return RedirectToAction("SignIn", "Account", null);
                }

                var paymentAccount = user.PaymentAccounts[0];
                var mobileNumber = user.MobileNumber;
                logger.Log(LogLevel.Debug, String.Format("Found user and payment account"));

                if (ModelState.IsValid)
                {
                    try
                    {
                        messageService.AddMessage(_apiKey, user.UserId.ToString(), "", model.RecipientUri, user.PaymentAccounts[0].Id.ToString(), model.Amount, model.Comments, @"Payment");
                        //ctx.Payments.Add(new Payment()
                        //{
                        //    Id = Guid.NewGuid(),
                        //    ApiKey = new Guid(ConfigurationManager.AppSettings["APIKey"]),
                        //    Comments = model.Comments,
                        //    CreateDate = System.DateTime.Now,
                        //    FromAccountId = paymentAccount.Id,
                        //    FromMobileNumber = mobileNumber,
                        //    PaymentAmount = model.Amount,
                        //    PaymentChannelType = PaymentChannelType.Single,
                        //    PaymentDate = System.DateTime.Now,
                        //    PaymentStatus = PaymentStatus.Submitted,
                        //    StandardEntryClass = StandardEntryClass.Web,
                        //    ToMobileNumber = model.RecipientUri
                        //});
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment. {0}", ex.Message));

                        return View(model);
                    }
                }
                else
                    return View(model);

                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Sending Payment {0}", ex.Message));
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Sending Payment. {0}", ex.StackTrace));

                    var innerException = ex.InnerException;
                    while (innerException != null)
                    {
                        logger.Log(LogLevel.Error, String.Format("Unhandled Exception Sending Payment {0}", innerException.Message));
                        logger.Log(LogLevel.Error, String.Format("Unhandled Exception Sending Payment. {0}", innerException.StackTrace));

                        innerException = innerException.InnerException;
                    }

                    return View(model);
                }

                return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });

            }
        }
        public ActionResult RequestMoney()
        {
            return View();
        }
        [HttpPost]
        public ActionResult RequestMoney(RequestMoneyModel model)
        {
            logger.Log(LogLevel.Debug, String.Format("Payment Request Posted to {0} of {1} with Comments {2}", model.RecipientUri, model.Amount, model.Comments));

            using (var ctx = new Context())
            {
                var applicationService = new SocialPayments.DomainServices.ApplicationService();
                var amazonNotificationService = new AmazonNotificationService();
                var messageService = new SocialPayments.DomainServices.MessageServices(ctx, amazonNotificationService);

                var userId = (Guid)Session["UserId"];

                if (Session["UserId"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);
                if (user.PaymentAccounts == null || user.PaymentAccounts.Count == 0)
                {
                    logger.Log(LogLevel.Error, String.Format("Invalid Payment Account"));

                    return RedirectToAction("SignIn", "Account", null);
                }

                var paymentAccount = user.PaymentAccounts[0];

                logger.Log(LogLevel.Debug, String.Format("Found user and payment account"));

                if (ModelState.IsValid)
                {
                    try
                    {
                        //Request Money
                        messageService.AddMessage(_apiKey, user.UserId.ToString(), "", model.RecipientUri, user.PaymentAccounts[0].Id.ToString(), model.Amount, model.Comments, @"PaymentRequest");

                        //ctx.PaymentRequests.Add(new PaymentRequest()
                        //{
                        //    Amount = model.Amount,
                        //    ApiKey = application.ApiKey,
                        //    Comments = model.Comments,
                        //    CreateDate = System.DateTime.Now,
                        //    PaymentRequestId = Guid.NewGuid(),
                        //    PaymentRequestStatus = PaymentRequestStatus.Submitted,
                        //    RecipientUri = model.RecipientUri,
                        //    RequestorId = user.UserId
                        //});
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment Request. {0}", ex.Message));

                        return View(model);
                    }
                }
                else
                    return View(model);

                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment Request {0}", ex.Message));
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment Request. {0}", ex.StackTrace));

                    var innerException = ex.InnerException;
                    while (innerException != null)
                    {
                        logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment Request {0}", innerException.Message));
                        logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment Request. {0}", innerException.StackTrace));

                        innerException = innerException.InnerException;
                    }

                    return View(model);
                }

                return RedirectToAction("Index", "Paystream", new RouteValueDictionary() { });

            }
        }
        public ActionResult CompleteProfile()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            using (var ctx = new Context())
            {
                var userId = (Guid)Session["UserId"];
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);
                var messageServices = new MessageServices(ctx);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var messages = messageServices.GetMessages(user.UserId);

                var payments = messages.Select(m => new PaystreamModels.PaymentModel()
                {
                    Amount = m.Amount,
                    RecipientUri = m.RecipientUri,
                    SenderUri = m.SenderUri,
                    TransactionDate = m.CreateDate,
                    TransactionStatus = Mobile_PaidThx.Models.TransactionStatus.Pending,
                    TransactionType = Mobile_PaidThx.Models.TransactionType.Deposit,
                    TransactionImageUri = m.TransactionImageUrl,
                    Comments = m.Comments
                }).ToList();

                var alerts = GetAlerts(user.UserId);
                logger.Log(LogLevel.Debug, String.Format("Getting Payment Accounts"));
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
                    AllReceipts = null,
                    //PaymentReceipts = paymentReceipts,
                    //RequestReceipts = paymentRequests,
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
                            PaymentAccounts = bankAccounts,
                            PreferredReceiveAccountId = user.PreferredReceiveAccountId.ToString(),
                            PreferredSendAccountId = user.PreferredSendAccountId.ToString()
                        }
                    }
                };
                logger.Log(LogLevel.Debug, String.Format("Return Paystream View"));

                return View(model);
            }
        }
        [HttpPost]
        public ActionResult CompleteProfile(CompleteProfileModel model)
        {
            var userId = (Guid)Session["UserId"];

            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            using (var ctx = new Context())
            {
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                user.SenderName = model.SenderName;

                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Exception Saving Changes. {0}", ex.Message));
                }

                return RedirectToAction("Index", "Profile");
            }
        }
        private List<PaystreamModels.AlertModel> GetAlerts(Guid userId)
        {
            var results = new List<PaystreamModels.AlertModel>();

            return results;
        }
        public ActionResult CompleteProfileDialog()
        {

            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            var userId = (Guid)Session["UserId"];

            using (var ctx = new Context())
            {
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var model = GetProfileModel(user);

                return PartialView("PartialViews/CompleteProfile", model);
            }
        }
        public ActionResult Settings()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);

            var userId = (Guid)Session["UserId"];
            using (var ctx = new Context())
            {
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                if (Session["User"] == null)
                    return RedirectToAction("SignIn", "Account", null);

                var model = GetProfileModel(user);

                return PartialView("PartialViews/Settings", model);
            }
        }

        public ActionResult ListSettings()
        {
            return PartialView("PartialViews/ListSettings");
        }
        private ProfileModels GetProfileModel(User user)
        {
            using (var ctx = new Context())
            {

                var model = new ProfileModels()
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
                    SenderName = user.SenderName
                };

                if (user.PaymentAccounts.Count > 0)
                {

                    var paymentAccountId = user.PaymentAccounts[0].Id;

                    logger.Log(LogLevel.Info, paymentAccountId);

                    var messages = ctx.Messages
    .Where(m => m.SenderId == user.UserId || m.RecipientId.Value == user.UserId)
    .OrderByDescending(m => m.CreateDate)
    .ToList<Message>();

                    var transactionStatus = Mobile_PaidThx.Models.TransactionStatus.Submitted;

                    logger.Log(LogLevel.Info, messages.Count());

                    try
                    {
                        foreach (var transaction in messages)
                        {
                            model.TransactionReceipts.Add(new PaystreamModels.PaymentModel()
                            {
                                Amount = transaction.Amount,
                                SenderUri = transaction.SenderUri,
                                TransactionDate = transaction.CreateDate,
                                TransactionType = Mobile_PaidThx.Models.TransactionType.Withdrawal,
                                TransactionStatus = transactionStatus,
                                TransactionImageUri = transaction.TransactionImageUrl,
                                Comments = transaction.Comments
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Info, String.Format("Unhandled Exception {0}", ex.Message));
                        var innerException = ex.InnerException;

                        while (innerException != null)
                        {
                            logger.Log(LogLevel.Info, String.Format("Unhandled Exception {0}", innerException.Message));
                            innerException = innerException.InnerException;
                        }

                    }

                }

                return model;
            }
        }
    }
}
