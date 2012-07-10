using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mobile_PaidThx.HtmlHelpers
{

    public static class PaystreamHelperExtensions
    {
        public static MvcHtmlString FormatUri(this HtmlHelper helper, string uri)
        {

            SocialPayments.DomainServices.FormattingServices formattingService = new SocialPayments.DomainServices.FormattingServices();

            var tempUri = formattingService.FormatMobileNumber(uri);

            return new MvcHtmlString(tempUri);
        }

        public static String formatTime(DateTime time)
        {
            String timeAgo = "";
            DateTime currentTime = DateTime.Now;
            //if today, go "minutes ago" / "hours ago"
            if (time.Date == currentTime.Date)
            {
                if (time.Minute == currentTime.Minute)
                {
                    if (currentTime.Second - time.Second == 1)
                    {
                        return "1 second ago";
                    }
                    return timeAgo += currentTime.Second - time.Second + " seconds ago";
                }
                if (time.Hour == currentTime.Hour)
                {
                    if (currentTime.Minute - time.Minute == 1)
                    {
                        return "1 minute ago";
                    }
                    // "minutes ago"
                    return timeAgo += (currentTime.Minute - time.Minute) + " minutes ago";
                }
                else
                {
                    if (currentTime.Hour - time.Hour == 1)
                    {
                        return "1 hr ago";
                    }
                    // hours ago
                    return timeAgo += (currentTime.Hour - time.Hour) + " hours ago";
                }

            }
            else
            {
                // just show date and time
                return time.ToShortDateString() + " @ " + time.ToShortTimeString();
            }
        }

        public static String FormatDate(DateTime sentTime)
        {
            String header = "";
            DateTime currentTime = DateTime.Now;
            // if it is the user's today date
            if (sentTime.Date == currentTime.Date)
            {
                return header += "Today";
            }
            else
            {
                // check within last 7 days
                DateTime aWeekEarlier = currentTime.AddDays(-7);
                if (sentTime.Date.CompareTo(currentTime) < 0 && sentTime.Date.CompareTo(aWeekEarlier) > 0)
                {
                    return header += "This week";
                }
                // check within this month
                else
                {
                    if (sentTime.Month == currentTime.Month)
                    {
                        return header += "This month";
                    }
                    else
                    {
                        // check last month
                        DateTime lastMonth = currentTime.AddMonths(-1);
                        if (sentTime.Month == lastMonth.Month)
                        {
                            return header += "Last month";
                        }
                        else
                        {
                            int month = sentTime.Month;
                            if (month == 1)
                            {
                                return header += "January " + sentTime.Year;

                            }
                            else if (month == 2)
                            {
                                return header += "February " + sentTime.Year;

                            }
                            else if (month == 3)
                            {
                                return header += "March " + sentTime.Year;

                            }
                            else if (month == 4)
                            {
                                return header += "April " + sentTime.Year;

                            }
                            else if (month == 5)
                            {
                                return header += "May " + sentTime.Year;

                            }
                            else if (month == 6)
                            {
                                return header += "June " + sentTime.Year;

                            }
                            else if (month == 7)
                            {
                                return header += "July " + sentTime.Year;

                            }
                            else if (month == 8)
                            {
                                return header += "August " + sentTime.Year;

                            }
                            else if (month == 9)
                            {
                                return header += "September " + sentTime.Year;

                            }
                            else if (month == 10)
                            {
                                return header += "October " + sentTime.Year;
                            }

                            else if (month == 11)
                            {
                                return header += "November " + sentTime.Year;

                            }
                            else
                            {
                                return header += "December " + sentTime.Year;

                            }

                        }
                    }
                }
            }
        }
    }
}
