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
        public Domain.TransactionBatch GetPagedTransactions(string batchId, string type, int take, int skip, int page, int pageSize, out int totalRecords)
        {
            using (var ctx = new Context())
            {
                Guid id;
                Guid.TryParse(batchId, out id);

                if (id == null)
                    throw new CustomExceptions.NotFoundException("Batch {0} Not Found");

                var batch = ctx.TransactionBatches
                    .FirstOrDefault(b => b.Id.Equals(id));

                if (batch == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Batch {0} Not Valid", batchId));

                totalRecords = batch.Transactions
                    .Count();

                batch.Transactions = ctx.Transactions
                    .Where(b => b.TransactionBatchId == id)
                        .OrderByDescending(b => b.CreateDate)
                        .Skip(skip)
                        .Take(take)
                        .ToList<Domain.Transaction>();

                return batch;
            }
        }
        
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

                if (batch == null)
                    throw new CustomExceptions.NotFoundException(String.Format("Batch {0} Not Valid", batchId));

                return batch;
            }
        }
    }
}
