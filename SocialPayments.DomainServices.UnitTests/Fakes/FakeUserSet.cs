using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeUserSet : FakeDbSet<User>
    {
        public override User Find(params object[] keyValues)
        {
            return this.SingleOrDefault(u => u.UserId == (Guid)keyValues.Single());
        }
    }
}
