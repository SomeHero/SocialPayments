using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class UserPayPointVerification
    {
        public Guid Id { get; set; }
        public Guid UserPayPointId { get; set; }
        [ForeignKey("UserPayPointId")]
        public virtual UserPayPoint UserPayPoint { get; set; }
        public string VerificationCode { get; set; }
        public bool Confirmed { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
