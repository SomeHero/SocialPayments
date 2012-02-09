using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SocialPayments.DataLayer;
using SocialPayments.DomainServices;
using SocialPayments.Web.Admin.Models;

namespace SocialPayments.Web.Admin.Controllers
{
    public class PaymentController : Controller
    {
        //
        // GET: /Payment/
        private readonly Context _ctx = new Context();
        
        public ActionResult Index(int pageIndex, int pageSize)
        {
            var model = _ctx.Payments
                .OrderByDescending(p => p.CreateDate)
                .Select(payment => new PaymentModel()
                                                       {
                                                           Amount = payment.PaymentAmount,
                                                           FromMobileNumber = payment.FromMobileNumber,
                                                           ToMobileNumber = payment.ToMobileNumber,
                                                           Id = payment.Id,
                                                           Status = "Ok",
                                                           SubmittedDate = payment.CreateDate
                                                       });

            return View(model.ToPagedList(pageIndex, pageSize));
        }

    }
}
