﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialPayments.DataLayer;
using Mobile_PaidThx.Models;
using NLog;
using Domain = SocialPayments.Domain;
using SocialPayments.DomainServices;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Mobile_PaidThx.Controllers.Base;
using System.Text;
using System.Net.Mail;

namespace Mobile_PaidThx.Controllers
{
    public class HomeController : PaidThxBaseController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _apiKey = "BDA11D91-7ADE-4DA1-855D-24ADFE39D174";

        public ActionResult Index(string messageId)
        {
            logger.Log(LogLevel.Info, String.Format("Displaying Register View"));
            logger.Log(LogLevel.Info, String.Format("Retreiving Payment {0}.", messageId));

            if (String.IsNullOrEmpty(messageId))
                return View("Index");

            using (var ctx = new Context())
            {
                Guid id;
                UserService userService = new UserService(ctx);

                logger.Log(LogLevel.Info, String.Format("Parsing GUID {0}.", messageId));
                
                if (!Guid.TryParse(messageId, out id))
                {
                    logger.Log(LogLevel.Info, String.Format("Invalid payment id {0}.", messageId));

                    ModelState.AddModelError("", String.Format("Unable to find the transaction specified {0}.", messageId));

                    return View("Register", new RegisterModel());
                }
                logger.Log(LogLevel.Info, String.Format("Parsed GUID {0}.", messageId));

                logger.Log(LogLevel.Info, String.Format("Found Guid {0}", messageId));

                Domain.Message payment = null;
                try
                {
                    payment = ctx.Messages
                        .FirstOrDefault(p => p.Id == id);
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, String.Format("Exception Occurred retreiving payment record for paymentId {0}. {1}", messageId, ex.Message));
                }

                if (payment == null)
                {
                    logger.Error(String.Format("Invalid payment record {0}", messageId));

                    ModelState.AddModelError("", String.Format("Unable to find the transaction specified {0}."));

                    return View("Register", new RegisterModel());
                }

               // if (payment.ToAccount != null && payment.ToAccount.User != null)
                    //return View("SignIn");

                var model = new RegisterModel()
                {
                    Payment = new PaymentModel()
                    {
                        Sender = userService.GetSenderName(payment.Sender),
                        Amount = payment.Amount,
                        Comments = payment.Comments,
                        MobileNumber = payment.SenderUri
                    }
                };
                Session["Payment"] = model.Payment;

                return View("Register", model);
            }
        }

        public ActionResult About()
        {
            logger.Log(LogLevel.Info, String.Format("Displaying About PaidThx View"));

            var model = new HomeModel()
            {
                ContactUsModel = new ContactUsModel()
            };

            return View(model);
        }
        public ActionResult ContactUs()
        {
            logger.Log(LogLevel.Info, String.Format("Displaying PdThx Contact Us View"));

            var model = new HomeModel()
            {
                ContactUsModel = new ContactUsModel()
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult ContactUs(ContactUsModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    StringBuilder sbBody = new StringBuilder();
                    sbBody.AppendFormat("A message was submitted from {0}.", model.Name);
                    sbBody.Append(Environment.NewLine);
                    sbBody.Append(Environment.NewLine);
                    sbBody.AppendFormat("Name: {0}", model.Name);
                    sbBody.Append(Environment.NewLine);
                    sbBody.AppendFormat("Email Address: {0}", model.Email);
                    sbBody.Append(Environment.NewLine);
                    if (model.Phone.Length > 0)
                    {
                        sbBody.AppendFormat("Phone: {0}", model.Phone);
                        sbBody.Append(Environment.NewLine);
                    }
                    sbBody.Append("Message:");
                    sbBody.Append(Environment.NewLine);
                    sbBody.Append(Environment.NewLine);
                    sbBody.AppendFormat("{0}", model.Message);
                    

                    //Send Email
                    SmtpClient sc = new SmtpClient();
                    sc.EnableSsl = true;

                    sc.Send("admin@paidthx.com", "jrhodes2705@gmail.com", "Inquiry from PaidThx Contact Form", sbBody.ToString());

                    model.MessageSubmitted = true;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, String.Format("Unhandled Exception Sumitting Contact Us Message. {0}", ex.Message));
                ModelState.AddModelError("", "Sorry, unable to submit your message");
            }

            return View(new HomeModel()
            {
                ContactUsModel = model
            });
        }
        public ActionResult PrivacyPolicy()
        {
            logger.Log(LogLevel.Info, String.Format("Displaying PdThx Privacy Policy View"));

            var model = new HomeModel() {
                ContactUsModel = new ContactUsModel()
            };

            return View(model);
        }

        //************* Modals *********************/
        public PartialViewResult GetMobileNumberModal()
        {
            return PartialView("PartialViews/GetMobileNumberModal");
        }

    }
}
