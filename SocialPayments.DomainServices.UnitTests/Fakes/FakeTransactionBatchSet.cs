using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeTransactionBatchSet : FakeDbSet<TransactionBatch>
    {
        public override TransactionBatch Find(params object[] keyValues)
        {
            return this.SingleOrDefault(a => a.Id == (Guid)keyValues.Single());
        }
    }
}
