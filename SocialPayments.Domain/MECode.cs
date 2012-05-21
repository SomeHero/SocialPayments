using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class MECode
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string MeCode { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
