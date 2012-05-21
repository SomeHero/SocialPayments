using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeRoleSet : FakeDbSet<Role>
    {
        public override Role Find(params object[] keyValues)
        {
            return this.SingleOrDefault(a => a.RoleId == (Guid)keyValues.Single());
        }
    }
}
