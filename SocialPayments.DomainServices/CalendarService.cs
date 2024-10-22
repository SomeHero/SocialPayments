﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocialPayments.Domain;
using SocialPayments.DataLayer;
using System.Data.Entity;
using SocialPayments.DataLayer.Interfaces;

namespace SocialPayments.DomainServices
{
    public class CalendarService
    {
        private IDbContext _ctx;

        public CalendarService()
        {}

        public CalendarService(IDbContext context)
        {
            _ctx = context;
        }
            
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
        public void UpdateCalendar(Calendar calendar)
        {
            _ctx.SaveChanges();
        }
        public void DeleteCalendar(int id)
        {
            _ctx.Calendars.Remove(GetCalendar(id));

            _ctx.SaveChanges();
        }
    }
}
