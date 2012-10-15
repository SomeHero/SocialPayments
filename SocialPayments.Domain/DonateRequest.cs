using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain
{
    public class DonateRequest
    {
        public Guid Id { get; set; }
        public String SenderAccountId { get; set; }
        public String OrganizationId { get; set; }
        public Double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public String MessageId { get; set; }
        public int StatusValue { get; set; }
        public DonateRequestStatus Status
        {
            get { return (DonateRequestStatus)StatusValue; }
            set { StatusValue = (int)value; }
        }
    }
}
