using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using SocialPayments.Services.DataContracts.Calendar;
using System.ServiceModel.Web;

namespace SocialPayments.Services.ServiceContracts
{
    [ServiceContract]
    public interface ICalendarService
    {
        [OperationContract]
        [WebInvoke(Method = "Post", UriTemplate = "/Calendars", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        CalendarResponse AddCalendar(CalendarRequest calendarRequest);

        [OperationContract]
        [WebGet( UriTemplate = "/Calendars", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        List<CalendarResponse> GetCalendars();

        [OperationContract]
        [WebGet(UriTemplate = "/Calendars/{id}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        CalendarResponse GetCalendar(string id);

        [OperationContract]
        [WebInvoke(Method="Put", UriTemplate = "/Calendars", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        void UpdateCalendar(CalendarRequest calendarRequest);

        [OperationContract]
        [WebInvoke(Method = "Delete", UriTemplate = "/Calendars", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        void DeleteCalendar(CalendarRequest calendarRequest);
    }
}