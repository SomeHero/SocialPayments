using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface ICalendar
    {
        int Id { get; set; }
        IApplication Application { get; set; }
        ICollection<CalendarDate> CalendarDates { get; set; }
        int CalendarTypeValue { get; set; }
        CalendarType CalendarType { get; set; }
        string CalendarCode { get; set; }
    }
}
