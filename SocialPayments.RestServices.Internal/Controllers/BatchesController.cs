using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using SocialPayments.RestServices.Internal.Models;
using SocialPayments.DataLayer;
using System.Data.Entity;

namespace SocialPayments.RestServices.Internal.Controllers
{
    public class BatchesController : ApiController
    {

        // GET /api/batch
        public IEnumerable<BatchModels.BatchResponse> Get()
        {
            using (var ctx = new Context())
            {
                var formattingService = new DomainServices.FormattingServices();
                var batches = ctx.TransactionBatches.Select(b => b)
                    .OrderByDescending(b => b.CreateDate)
                    .ToList();

                return batches.Select(b =>
                    new BatchModels.BatchResponse()
                    {
                        ClosedDate = formattingService.FormatDateTimeForJSON(b.ClosedDate),
                        CreateDate = formattingService.FormatDateTimeForJSON(b.CreateDate),
                        Id = b.Id,
                        IsClosed = b.IsClosed,
                        LastDateUpdated = formattingService.FormatDateTimeForJSON(b.LastDateUpdated),
                        TotalDepositAmount = b.TotalDepositAmount,
                        TotalNumberOfDeposits = b.TotalNumberOfDeposits,
                        TotalWithdrawalAmount = b.TotalWithdrawalAmount,
                        TotalNumberOfWithdrawals = b.TotalNumberOfWithdrawals
                    });
            }
        }

        public BatchModels.BatchResponse Get(string id)
        {
            using (var ctx = new Context())
            {
                var securityService = new DomainServices.SecurityService();
                var formattingService = new DomainServices.FormattingServices();
                var batch = ctx.TransactionBatches
                    .Include("Transactions")
                    .OrderByDescending(b => b.CreateDate)
                    .FirstOrDefault();

                return new BatchModels.BatchResponse()
                {
                    ClosedDate = formattingService.FormatDateTimeForJSON(batch.ClosedDate),
                    CreateDate = formattingService.FormatDateTimeForJSON(batch.CreateDate),
                    Id = batch.Id,
                    IsClosed = batch.IsClosed,
                    LastDateUpdated = formattingService.FormatDateTimeForJSON(batch.LastDateUpdated),
                    TotalDepositAmount = batch.TotalDepositAmount,
                    TotalNumberOfDeposits = batch.TotalNumberOfDeposits,
                    TotalWithdrawalAmount = batch.TotalWithdrawalAmount,
                    TotalNumberOfWithdrawals = batch.TotalNumberOfWithdrawals
                };
            }
        }

        // POST /api/batches
        // addes transactions to the current open batch
        public void Post(string value)
        {

        }

        // PUT /api/batch/5
        public void Put(int id, string value)
        {
        }

        // DELETE /api/batch/5
        public void Delete(int id)
        {
        }
    }
}
