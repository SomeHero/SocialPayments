using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mobile_PaidThx.Models;
using NLog;
using Mobile_PaidThx.Controllers.Base;
using System.Text;
using System.Net.Mail;
using Mobile_PaidThx.Services;

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

            if (String.IsNullOrEmpty(messageId) || messageId.Length <= 32 )
                return View("Index", new HomeModels.HomeModel()
                {
                    Payment = null
                });

            var messageServices = new MessageServices();
            var payment = messageServices.GetMessage(messageId);

            if(payment == null)
                return View("Index", new HomeModels.HomeModel()
                {
                    Payment = null
                });

            Session["MessageId"] = payment.Id;

            return View("Index", new HomeModels.HomeModel()
            {
                Payment = new PaymentModel()
                {
                    Amount = payment.amount,
                    Comments = payment.comments,
                    MobileNumber = payment.recipientUri,
                    Sender = payment.senderName
                }
            });
        }

        public void ClaimPayment(string id)
        {
            logger.Info(String.Format("The id is {0}", id));

            Response.Redirect(String.Format("http://goo.gl/{0}", id));
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
