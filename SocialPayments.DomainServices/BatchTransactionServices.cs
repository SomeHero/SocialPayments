using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;
using System.Data.Entity;

namespace SocialPayments.DomainServices
{
    public class BatchTransactionsServices
    {
        public Domain.TransactionBatch GetBatchTransactions(string batchId)
        {
            using (var ctx = new Context())
            {
                Guid id;
                Guid.TryParse(batchId, out id);

                if (id == null)
                    throw new CustomExceptions.NotFoundException("Batch {0} Not Found");

                var batch = ctx.TransactionBatches
                    .Include("Transactions")
                    .FirstOrDefault(b => b.Id.Equals(id));

                return batch;
            }
        }
    }
}
