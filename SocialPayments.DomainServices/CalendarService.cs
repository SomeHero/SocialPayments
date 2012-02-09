using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;

namespace SocialPayments.DomainServices
{
    public class CalendarService
    {
        private readonly Context _ctx = new Context();
            
        public Calendar AddCalender(string calendarCode, List<CalendarDate> calendarDates, CalendarType calendarType)
        {
            var calendar = _ctx.Calendars.Add(new Calendar()
            {
                CalendarCode = calendarCode,
                CalendarDates = calendarDates,
                CalendarType = calendarType
            });

            _ctx.SaveChanges();

            return calendar;
        }
        public List<Calendar> GetCalendars()
        {
            var calendars = _ctx.Calendars.Select(c => c);

            return calendars.ToList<Calendar>();
        }
        public Calendar GetCalendar(int id)
        {
            return _ctx.Calendars.FirstOrDefault(c => c.Id == id);
        }
        public void UpdateCalendar(int id, string calendarCode, List<CalendarDate> calendarDates, CalendarType calendarType)
        {
            var calendar = GetCalendar(id);

            calendar.CalendarCode = calendarCode;
            calendar.CalendarDates = calendarDates;
            calendar.CalendarType = calendarType;

            _ctx.SaveChanges();
        }
        public void DeleteCalendar(int id)
        {
            _ctx.Calendars.Remove(GetCalendar(id));

            _ctx.SaveChanges();
        }
    }
}
