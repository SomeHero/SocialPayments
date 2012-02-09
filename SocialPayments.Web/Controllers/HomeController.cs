using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PaymentRequestDomain = SocialPayments.Services.DataContracts.Payment;
using SocialPayments.Services;

namespace SocialPayments.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection formCollection)
        {
            PaymentRequestDomain.PaymentRequest paymentRequest = new PaymentRequestDomain.PaymentRequest();

            paymentRequest.Amount = Convert.ToDouble(formCollection["txtAmount"]);
            paymentRequest.FromMobileNumber = formCollection["txtYourMobileNumber"];
            paymentRequest.ToMobileNumber = formCollection["txtToMobileNumber"];
            paymentRequest.Comment = formCollection["txtComment"];

            PaymentService paymentService = new PaymentService();
            paymentService.AddPayment(paymentRequest);

            return View();
        }
        public ActionResult About()
        {
            return View();
        }
    }
}
