using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialPayments.Domain.Interfaces
{
    public interface ICalendarDate
    {
        int Id { get; set; }
        DateTime SelectedDate { get; set; }
        ICalendar Calendar { get; set; }
    }
}
