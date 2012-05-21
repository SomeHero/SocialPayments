using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;

namespace SocialPayments.DomainServices.UnitTests.Fakes
{
    public class FakeCalendarSet : FakeDbSet<Calendar>
    {
        public override Calendar Find(params object[] keyValues)
        {
            return this.SingleOrDefault(c => c.Id == (int)keyValues.Single());
        }
    }
}
