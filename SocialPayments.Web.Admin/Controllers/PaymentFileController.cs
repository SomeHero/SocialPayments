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
    public class PaymentFileController : Controller
    {
        //
        // GET: /PaymentFile/
        private readonly Context _ctx = new Context();
        
        public ActionResult Index(int pageIndex, int pageSize)
        {
           var model = _ctx.TransactionBatches
                .Where(t => t.IsClosed)
                .OrderByDescending(t => t.CreateDate)
                .Select(batch => new PaymentFileModel()
                                                              {
                                                                Id = batch.Id,
                                                                NumberOfDeposits = batch.TotalNumberOfDeposits,
                                                                NumberOfWithdrawls = batch.TotalNumberOfWithdrawals,
                                                                ClosedDate = System.DateTime.Now,
                                                                TotalDepositAmount =  batch.TotalDepositAmount,
                                                                TotalWithdrawlAmount =  batch.TotalWithdrawalAmount,
                                                                OpenedDate =  batch.CreateDate
                                                              });

            return View(model.ToPagedList(pageIndex, pageSize));
        }

    }
}
