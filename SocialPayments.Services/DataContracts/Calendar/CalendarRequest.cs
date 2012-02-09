using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SocialPayments.Services.DataContracts.Calendar
{
    [DataContract]
    public class CalendarRequest
    {
        [DataMember(Name = "calendarId")]
        public int CalendarId { get; set; }

        [DataMember(Name = "calendarCode")]
        public string CalendarCode { get; set; }

        [DataMember(Name = "calendarType")]
        public string CalendarType { get; set; }

        [DataMember(Name = "calendarDates")]
        public List<CalendarDateRequest> CalendarDates { get; set; }
    }
}