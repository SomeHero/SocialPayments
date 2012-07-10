using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class UserPayPoint
    {
        public virtual Guid Id { get; set; }
        public virtual Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public virtual int PayPointTypeId { get; set; }
        [ForeignKey("PayPointTypeId")]
        public virtual PayPointType Type { get; set; }
        [MaxLength(50)]
        public virtual string URI { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual DateTime CreateDate { get; set; }
    }
}
