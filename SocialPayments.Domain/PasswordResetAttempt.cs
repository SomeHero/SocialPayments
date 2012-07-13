using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class PasswordResetAttempt
    {
        public virtual Guid Id { get; set; }
        public virtual Guid UserId { get; set; }
        public virtual DateTime ExpiresDate { get; set; }
        public virtual bool Clicked { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
