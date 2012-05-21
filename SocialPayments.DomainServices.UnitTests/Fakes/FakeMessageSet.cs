using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeMessageSet : FakeDbSet<Message>
    {
        public override Message Find(params object[] keyValues)
        {
            return this.SingleOrDefault(m => m.Id == (Guid)keyValues.Single());
        }
    }
}
