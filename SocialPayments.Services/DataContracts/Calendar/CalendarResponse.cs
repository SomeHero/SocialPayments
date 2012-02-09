using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Calendar
{
    [DataContract]
    public class CalendarResponse
    {
        [DataMember(Name = "calendarId")]
        public int CalendarId { get; set; }

        [DataMember(Name = "calendarCode")]
        public string CalendarCode { get; set; }

        [DataMember(Name = "calendarType")]
        public string CalendarType { get; set; }

        [DataMember(Name = "calendarDates")]
        public List<CalendarDateResponse> CalendarDates { get; set; }
    }
}