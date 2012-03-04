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
                                                           PaymentStatusValue = payment.PaymentStatusValue,
                                                           SubmittedDate = payment.CreateDate
                                                       });

            return View(model.ToPagedList(pageIndex, pageSize));
        }
        public ActionResult CancelPayment(string paymentId)
        {
            var payment = _ctx.Payments
                .FirstOrDefault(p => p.Id == new Guid(paymentId));

            if (payment == null)
                throw new Exception(String.Format("Unable to cancel payment {0}. Payment not found.", paymentId));

            if (payment.PaymentStatus != Domain.PaymentStatus.Submitted && payment.PaymentStatus != Domain.PaymentStatus.ReturnedNSF)
                throw new Exception(String.Format("Unable to cancel payment {0}. Payment not in a valid status.", paymentId));
            
            payment.PaymentStatus = Domain.PaymentStatus.Cancelled;
            payment.LastUpdatedDate = System.DateTime.Now;

            foreach (var transaction in payment.Transactions)
            {
                transaction.Status = Domain.TransactionStatus.Cancelled;
                transaction.LastUpdatedDate = System.DateTime.Now;
            }

            _ctx.SaveChanges();

            return RedirectToAction("Index");

        }
        public ActionResult UpdatePayment(UpdateModel model)
        {
            var payment = _ctx.Payments
                .FirstOrDefault(p => p.Id == new Guid(model.PaymentId));

            if(payment == null)
                throw new Exception(String.Format("Unable to update payment {0}.  Payment not found.", model.PaymentId));

            if(!(payment.PaymentStatus == Domain.PaymentStatus.Submitted || payment.PaymentStatus == Domain.PaymentStatus.Pending))
                throw new Exception(String.Format("Unable to update payment {0}. Payment not in a valid status.", model.PaymentId));

            payment.PaymentAmount = model.PaymentAmount;
            payment.LastUpdatedDate = System.DateTime.Now;

            foreach(var transaction in payment.Transactions)
            {
                transaction.Amount = model.PaymentAmount;
                transaction.LastUpdatedDate = System.DateTime.Now;
            }

            _ctx.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
