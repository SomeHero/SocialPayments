using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialPayments.DomainServices;
using NLog;
using SocialPayments.DataLayer;
using Mobile_PaidThx.Models;

namespace Mobile_PaidThx.Controllers
{
    public class PaymentAccountController : Controller
    {
        //
        // GET: /PaymentAccount/
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult List()
        {
            using (var ctx = new Context())
            {
                // if (Session["UserId"] == null)
                // return RedirectToAction("SignIn", "Account", null);
                var securityService = new SocialPayments.DomainServices.SecurityService();
                var userId = (Guid)Session["UserId"];
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                //if (Session["User"] == null)
                // return RedirectToAction("SignIn", "Account", null);

                var model = new ListPaymentAccountModel();
                model.PreferredReceiveAccountId = user.PreferredReceiveAccountId.ToString();
                model.PreferredSendAccountId = user.PreferredSendAccountId.ToString();

                foreach (var paymentAccount in user.PaymentAccounts)
                {
                    if (paymentAccount.IsActive == true)
                    {
                        var tempNumber = securityService.Decrypt(paymentAccount.AccountNumber);
                        if (tempNumber.Length > 3)
                        {
                            tempNumber = tempNumber.Substring(tempNumber.Length - 4);
                        }
                        model.PaymentAccounts.Add(new BankAccountModel()
                        {
                             BankName = paymentAccount.BankName,
                            BankIconURL = paymentAccount.BankIconURL,
                            PaymentAccountId = paymentAccount.Id.ToString(),
                            AccountNumber = "******" + tempNumber,
                            AccountType = paymentAccount.AccountType.ToString(),
                            NameOnAccouont = securityService.Decrypt(paymentAccount.NameOnAccount),
                            Nickname = paymentAccount.Nickname,
                            RoutingNumber = securityService.Decrypt(paymentAccount.RoutingNumber)

                           
                        });
                    }
                }

                return View("Index", model);
            }
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
            using (var ctx = new Context())
            {
                var securityService = new SocialPayments.DomainServices.SecurityService();

                if (ModelState.IsValid)
                {
                    // if (Session["UserId"] == null)
                    // return RedirectToAction("SignIn", "Account", null);

                    var userId = (Guid)Session["UserId"];
                    var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);

                    //if (Session["User"] == null)
                    // return RedirectToAction("SignIn", "Account", null);
                    var paymentAccountTypeId = (int)SocialPayments.Domain.PaymentAccountType.Savings;
                    if (model.AccountType.ToLower().Equals("checking"))
                        paymentAccountTypeId = (int)SocialPayments.Domain.PaymentAccountType.Checking;


                    var paymentAccount = new SocialPayments.Domain.PaymentAccount()
                    {
                        Nickname = model.Nickname,
                        AccountNumber = securityService.Encrypt(model.AccountNumber),
                        PaymentAccountTypeId = paymentAccountTypeId,
                        NameOnAccount = securityService.Encrypt(model.NameOnAccount),
                        RoutingNumber = securityService.Encrypt(model.RoutingNumber),
                        UserId = userId,
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
                            user.PreferredReceiveAccountId = paymentAccount.Id;
                        }
                    }
                    if (model.DefaultSend != null)
                    {
                        if (model.DefaultSend.ToLower().Equals("sending"))
                        {
                            user.PreferredSendAccountId = paymentAccount.Id;
                        }
                    }
                    ctx.PaymentAccounts.Add(paymentAccount);

                    try
                    {
                        ctx.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, String.Format("Unhandled Exception Adding Payment Account. {0}", ex.Message));
                    }

                    return RedirectToAction("List");
                }
                else
                {
                    return PartialView("ParitalViews/Add", model);
                }
            }
        }
   
        public PartialViewResult Edit(string Id)
        {
         //    if (Session["UserId"] == null)
       //    return RedirectToAction("SignIn", "Account", null);
            using (var ctx = new Context())
            {
                var userId = (Guid)Session["UserId"];
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);
                //if (Session["User"] == null)
                // return RedirectToAction("SignIn", "Account", null);
                

                    var paymentAccount = user.PaymentAccounts.FirstOrDefault(a => a.Id == new Guid(Id));
                    var securityService = new SocialPayments.DomainServices.SecurityService();

                    return PartialView("PartialViews/Edit", new EditPaymentAccountModel()
                    {
                        AccountNumber = securityService.Decrypt(paymentAccount.AccountNumber),
                        AccountType = paymentAccount.AccountType.ToString(),
                        NameOnAccount = securityService.Decrypt(paymentAccount.NameOnAccount),
                        Nickname = paymentAccount.Nickname,
                        RoutingNumber = securityService.Decrypt(paymentAccount.RoutingNumber),
                        PaymentAccountId = paymentAccount.Id.ToString(),
                        AccountTypeOptions = new SelectListItem[] 
                        {
                            new SelectListItem() { Text = "Savings", Value = "Savings" },
                            new SelectListItem() { Text = "Checking", Value= "Checking" }
                        }
                    });
            }
        }

        [HttpPost]
        public ActionResult Edit(EditPaymentAccountModel model, string Id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Account", null);
            
            using (var ctx = new Context())
            {               
                var userId = (Guid)Session["UserId"];
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);
                var securityService = new SocialPayments.DomainServices.SecurityService();
                var paymentAccount = user.PaymentAccounts.FirstOrDefault(a => a.Id == new Guid(Id));
                var paymentAccountTypeId = (int)SocialPayments.Domain.PaymentAccountType.Savings;           //Payment account Id always set as Savings!  Change logic here.

                if (model.AccountType != null)
                {
                    if (model.AccountType.ToLower().Equals("checking"))
                        paymentAccountTypeId = (int)SocialPayments.Domain.PaymentAccountType.Checking;
                }

                if (paymentAccount == null)
                {
                    ModelState.AddModelError("", "Unable to edit payment account");

                    return View(model);
                }
                
                paymentAccount.AccountNumber = securityService.Encrypt(model.AccountNumber);
                paymentAccount.NameOnAccount = securityService.Encrypt(model.NameOnAccount);
                paymentAccount.Nickname = model.Nickname;
                paymentAccount.RoutingNumber = securityService.Encrypt(model.RoutingNumber);
                paymentAccount.PaymentAccountTypeId = paymentAccountTypeId;
       
                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Editing Payment Account. {0}", ex.Message));
                }
               
                return RedirectToAction("List");
                
            }

        }
        public ActionResult Dialog()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult defaultReceiving(string Id)
        {
            using (var ctx = new Context())
            {
                var userId = (Guid)Session["UserId"];
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);
                var paymentAccount = user.PaymentAccounts.FirstOrDefault(a => a.Id == new Guid(Id));

                user.PreferredReceiveAccountId = paymentAccount.Id;
                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Editing Payment Account. {0}", ex.Message));
                }
            }

            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult defaultSending(string Id, EditPaymentAccountModel model)
        {
            using (var ctx = new Context())
            {
                var userId = (Guid)Session["UserId"];
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);
                var paymentAccount = user.PaymentAccounts.FirstOrDefault(a => a.Id == new Guid(Id));

                user.PreferredSendAccountId = paymentAccount.Id;
                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Editing Payment Account. {0}", ex.Message));
                }
            }

            return RedirectToAction("List");
        }
        [HttpPost]
        public ActionResult Remove(string Id, EditPaymentAccountModel model)
        {
            using (var ctx = new Context())
            {
                var userId = (Guid)Session["UserId"];
                var user = ctx.Users.FirstOrDefault(u => u.UserId == userId);
                var paymentAccount = user.PaymentAccounts.FirstOrDefault(a => a.Id == new Guid(Id));
                if (user.PreferredReceiveAccountId == paymentAccount.Id)
                {                  
                   
                }
                if (user.PreferredSendAccountId == paymentAccount.Id)
                {
                  //  Response.Write("Cannot remove account:  This is set as a Preferred Sending Account");
                }
                paymentAccount.IsActive = false;
                try
                {
                    ctx.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Unhandled Exception Deleting account. {0}", ex.Message));
                }
                return RedirectToAction("List");
            }

        }
    }
    
}
