using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SocialPayments.Services.ServiceContracts;
using System.ServiceModel.Activation;

namespace SocialPayments.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class CalendarService : ICalendarService
    {
        DomainServices.CalendarService calendarDomainService = new DomainServices.CalendarService();

        public DataContracts.Calendar.CalendarResponse AddCalendar(DataContracts.Calendar.CalendarRequest calendarRequest)
        {
            var calendar = calendarDomainService.AddCalender(calendarRequest.CalendarCode, calendarRequest.CalendarDates.Select(x => new Domain.CalendarDate { SelectedDate = x.SelectedDate }).ToList(),
                Domain.CalendarType.ExcludeDays);

            return new DataContracts.Calendar.CalendarResponse()
            {
                CalendarCode = calendar.CalendarCode,
                CalendarDates = calendar.CalendarDates.Select(x => new DataContracts.Calendar.CalendarDateResponse { CalendarDateId = x.Id, SelectedDate = x.SelectedDate }).ToList(),
                CalendarType = calendar.CalendarType.ToString()
            };
        }

        public List<DataContracts.Calendar.CalendarResponse> GetCalendars()
        {
            var calendars = calendarDomainService.GetCalendars();
            return calendars.Select(x => new DataContracts.Calendar.CalendarResponse { CalendarId = x.Id, CalendarCode = x.CalendarCode, CalendarType = x.CalendarType.ToString(), CalendarDates = x.CalendarDates.Select(d => new DataContracts.Calendar.CalendarDateResponse { CalendarDateId = d.Id, SelectedDate = d.SelectedDate }).ToList() }).ToList();
        }

        public DataContracts.Calendar.CalendarResponse GetCalendar(string id)
        {
            var calendar = calendarDomainService.GetCalendar(Convert.ToInt32(id));

            return new DataContracts.Calendar.CalendarResponse()
            {
                CalendarId = calendar.Id,
                CalendarCode = calendar.CalendarCode,
                CalendarType = calendar.CalendarType.ToString(),
                CalendarDates = calendar.CalendarDates.Select(x => new DataContracts.Calendar.CalendarDateResponse { CalendarDateId = x.Id, SelectedDate = x.SelectedDate }).ToList()
            };
        }

        public void UpdateCalendar(DataContracts.Calendar.CalendarRequest calendarRequest)
        {
            var calendar = calendarDomainService.GetCalendar(calendarRequest.CalendarId);

            calendar.CalendarCode = calendarRequest.CalendarCode;
            calendar.CalendarType = Domain.CalendarType.ExcludeDays;
            calendar.CalendarDates = calendarRequest.CalendarDates.Select(x => new Domain.CalendarDate { Id = x.CalendarDateId, SelectedDate = x.SelectedDate, Calendar = calendar }).ToList();
        }

        public void DeleteCalendar(DataContracts.Calendar.CalendarRequest calendarRequest)
        {
            calendarDomainService.DeleteCalendar(calendarRequest.CalendarId);
        }
    }
}