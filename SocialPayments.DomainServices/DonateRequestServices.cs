using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class DonateRequestServices
    {
        public void AddDonateRequest(string userId, string senderAccountId, string organizationId, double amount, string comments)
        {
            using (var ctx = new Context())
            {
                ctx.DonateRequests.Add(new Domain.DonateRequest()
                {
                    Amount = amount,
                    CreateDate = System.DateTime.Now,
                    ExpirationDate = System.DateTime.Now.AddHours(3),
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    SenderAccountId = senderAccountId,
                    Status = Domain.DonateRequestStatus.Pending
                });
            }

        }
    }
}
