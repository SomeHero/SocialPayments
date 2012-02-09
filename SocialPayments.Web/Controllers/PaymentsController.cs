using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SocialPayments.DataLayer;
using SocialPayments.Web.Models;

namespace SocialPayments.Web.Controllers
{
    public class PaymentsController : Controller
    {
        //
        // GET: /Payment/

        private Context _ctx = new Context();
        public ActionResult Index(int pageIndex, int pageSize)
        {
            Guid userId;
            Guid.TryParse(Session["UserId"].ToString(), out userId);

            var paymentModel = _ctx.Payments 
                .Where(p => p.FromAccount.UserId == userId)
                .OrderByDescending(p => p.CreateDate)
                .Select(payment => new PaymentModel()
                                                              {
                                                                  PaymentAmount = payment.PaymentAmount, 
                                                                  PaymentDate = payment.PaymentDate, 
                                                                  PaymentStatus = "Ok", 
                                                                  ToMobileNumber = payment.ToMobileNumber
                                                              });

            return View(paymentModel.ToPagedList(pageIndex, pageSize));
        }

    }
}
