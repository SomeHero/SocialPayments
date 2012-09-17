using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using System.Data.Entity;

namespace SocialPayments.DomainServices
{
    public class BatchServices
    {
        public List<Domain.TransactionBatch> GetBatches()
        {
            using (var ctx = new Context())
            {
                var formattingService = new DomainServices.FormattingServices();
                var batches = ctx.TransactionBatches.Select(b => b)
                    .OrderByDescending(b => b.CreateDate)
                    .ToList();

                return batches;
            }
        }
        public Domain.TransactionBatch GetBatch(string id)
        {
            using (var ctx = new Context())
            {
                var batch = ctx.TransactionBatches
                    .Include("Transactions")
                    .OrderByDescending(b => b.CreateDate)
                    .FirstOrDefault();

                return batch;
            }
        }
    }
}
