using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Security;
using SocialPayments.DataLayer;

namespace CodeFirstMembershipDemoSharp.Code
{
    public class CodeFirstContextInit : DropCreateDatabaseAlways<Context>
    {

        protected override void Seed(Context context)
        {
            MembershipCreateStatus createStatus = MembershipCreateStatus.ProviderError;
            CodeFirstSecurity.CreateAccount("Demo", "Demo", "demo@demo.com", "804-387-9693", "12341234", "12312134", 1, out createStatus,
                                            false);

        }

    }
} ;