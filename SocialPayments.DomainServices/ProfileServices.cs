using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class ProfileServices
    {
        public List<Domain.ProfileSection> GetProfileSections()
        {
            List<Domain.ProfileSection> profileSections = null;

            using (var ctx = new Context())
            {
                profileSections = ctx.ProfileSections
                    .Select(u => u)
                    .OrderBy(u => u.SortOrder)
                    .ToList();
            }

            return profileSections;
        }
    }
}
