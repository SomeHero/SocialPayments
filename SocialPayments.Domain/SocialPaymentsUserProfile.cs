using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Profile;

namespace SocialPayments.Domain
{
    public class SocialPaymentsUserProfile: ProfileBase
    {
        [SettingsAllowAnonymous(false)]
        [ProfileProvider("SocialPaymentsUserProvider")]
        public string FirstName
        {
            get { return base["FirstName"].ToString(); }
            set { base["FirstName"] = value; }
        }

        [ProfileProvider("SocialPaymentsUserProvider")]
        public string LastName
        {
            get { return base["LastName"].ToString(); }
            set { base["LastName"] = value; }
        }

    }
}
