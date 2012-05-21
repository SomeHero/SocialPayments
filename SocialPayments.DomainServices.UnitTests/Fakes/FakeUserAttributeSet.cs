using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeUserAttributeSet : FakeDbSet<UserAttribute>
    {
        public override UserAttribute Find(params object[] keyValues)
        {
            return this.SingleOrDefault(u => u.Id == (Guid)keyValues.Single());
        }
    }
}

