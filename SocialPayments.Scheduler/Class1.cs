using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;

namespace SocialPayments.Scheduler
{
    public class NachaFileScheduler
    {
        private IScheduler scheduler;

        public NachaFileScheduler()
        {
            // construct a scheduler factory
            ISchedulerFactory schedFact = new StdSchedulerFactory();

            // get a scheduler
            scheduler = schedFact.GetScheduler("NachFileScheduler");
            scheduler.Start();

            HolidayCalendar cal = new HolidayCalendar();
            scheduler.AddCalendar("myHolidays", cal, true, true);


        }
        public void AddJobToScheduler(JobDetail jobDetail)
        {
            // construct job info
            JobDetail jobDetail = new JobDetail("myJob", null, typeof(HelloJob));
            // fire every hour
            Trigger trigger = TriggerUtils.
            // start on the next even hour
            trigger.StartTimeUtc = TriggerUtils.GetEvenHourDate(DateTime.UtcNow);
            trigger.Name = "myTrigger";
            scheduler.ScheduleJob(jobDetail, trigger); 
        }
    }
}
