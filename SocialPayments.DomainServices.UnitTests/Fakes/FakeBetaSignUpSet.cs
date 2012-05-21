using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeBetaSignUpSet : FakeDbSet<BetaSignup>
    {
        public override BetaSignup Find(params object[] keyValues)
        {
            return this.SingleOrDefault(a => a.Id == (Guid)keyValues.Single());
        }
    }
}
