using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Calendar
{
    [DataContract]
    public class CalendarDateRequest
    {
        [DataMember(Name = "calendarDateId")]
        public int CalendarDateId { get; set; }

        [DataMember(Name = "SelectedDate")]
        public DateTime SelectedDate { get; set; }
    }
}
