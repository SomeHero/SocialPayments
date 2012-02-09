using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialPayments.Web.Admin.Models;
using SocialPayments.DataLayer;

namespace SocialPayments.Web.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly Context _ctx = new Context();
        
        public ActionResult Index()
        {
            var model = new OverviewModel();

            var currentBatch = _ctx.TransactionBatches.FirstOrDefault(b => b.IsClosed == false);

            model.CurrentBatchStats.BatchCreated = currentBatch.CreateDate;
            model.CurrentBatchStats.TotalNumberOfPayments = currentBatch.TotalNumberOfDeposits + currentBatch.TotalNumberOfWithdrawals;
            model.CurrentBatchStats.TotalNumberOfDeposits = currentBatch.TotalNumberOfDeposits;
            model.CurrentBatchStats.TotalNumberOfWithdrawals = currentBatch.TotalNumberOfWithdrawals;
            model.CurrentBatchStats.TotalDepositAmount = currentBatch.TotalDepositAmount;
            model.CurrentBatchStats.TotalWithdrawalAmount = currentBatch.TotalWithdrawalAmount;


            var batches = _ctx.TransactionBatches.Where(b => b.IsClosed == true).Take(10);
            model.ClosedBatches = batches.Select(batch => new ClosedBatch() {
                BatchClosed = batch.ClosedDate,
                BatchCreated = batch.CreateDate,
                TotalDepositAmount = batch.TotalDepositAmount,
                TotalWithdrawalAmount = batch.TotalWithdrawalAmount,
                TotalNumberOfDeposits = batch.TotalNumberOfDeposits,
                TotalNumberOfWithdrawals = batch.TotalNumberOfWithdrawals
            }).ToList();

            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
