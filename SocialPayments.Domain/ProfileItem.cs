using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class ProfileItem
    {
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Label { get; set; }
        public int SortOrder { get; set; }
        public int ProfileSectionId { get; set; }
        [ForeignKey("ProfileSectionId")]
        public ProfileSection ProfileSection { get; set; }

        //ProfileItemType
        //UserAttribute
    }
}
