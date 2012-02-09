using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class CalendarDate
    {
        public int Id { get; set; }
        public DateTime SelectedDate { get; set; }

        public virtual Calendar Calendar { get; set; }
    }
}
