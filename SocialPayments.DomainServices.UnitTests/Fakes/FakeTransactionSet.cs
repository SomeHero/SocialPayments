using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeTransactionSet : FakeDbSet<Transaction>
    {
        public override Transaction Find(params object[] keyValues)
        {
            return this.SingleOrDefault(a => a.Id == (Guid)keyValues.Single());
        }
    }
}
