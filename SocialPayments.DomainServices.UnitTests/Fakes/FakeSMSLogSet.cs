using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
   public class FakeSMSLogSet : FakeDbSet<SMSLog>
    {
        public override SMSLog Find(params object[] keyValues)
        {
            return this.SingleOrDefault(l => l.Id == (Guid)keyValues.Single());
        }
    }
}
