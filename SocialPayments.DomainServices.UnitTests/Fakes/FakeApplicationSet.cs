using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeApplicationSet : FakeDbSet<Application>
    {
        public override Application Find(params object[] keyValues)
        {
            return this.SingleOrDefault(a => a.ApiKey == (Guid)keyValues.Single());
        }
    }
}
