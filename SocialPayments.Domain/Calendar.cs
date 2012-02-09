using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialPayments.Domain
{
    public class Calendar
    {
        public Calendar()
        {
            CalendarDates = new List<CalendarDate>();
        }

        public int Id { get; set; }
        [Column("ApplicationId")]
        public Application Application { get; set; }
        public virtual List<CalendarDate> CalendarDates { get; set; }
        public int CalendarTypeValue { get; set; }
        public CalendarType CalendarType
        {
            get { return (CalendarType)CalendarTypeValue; }
            set { CalendarTypeValue = (int)value; }
        }
        public string CalendarCode { get; set; }
    }
}
