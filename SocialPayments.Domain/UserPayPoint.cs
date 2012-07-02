using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class UserPayPoint
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public int PayPointTypeId { get; set; }
        [ForeignKey("PayPointTypeId")]
        public PayPointType Type { get; set; }
        [MaxLength(50)]
        public string URI { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
