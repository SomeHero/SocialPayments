using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class ProfileItemSelectOption
    {
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("ProfileItemId")]
        public ProfileItem ProfileItem { get; set; }
        public int ProfileItemId { get; set; }
        public string OptionValue { get; set; }
        public int SortOrder { get; set; }
    }
}
