﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NLog;
using Quartz;
using Quartz.Impl;
using SocialPayments.Jobs.ProcessSettlementProcessor;
using SocialPayments.WindowsServices.SettlementProcessor.CustomConfigurationSectons;

namespace SocialPayments.WindowsServices.SettlementProcessor
{
    public partial class Service1 : ServiceBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            try
            {
                logger.Log(LogLevel.Info, String.Format("Starting NACHA File Processor"));

                var jobScheduling = JobSchedulingProviderSection.Current;

                // construct a scheduler factory
                ISchedulerFactory schedFact = new StdSchedulerFactory();

                IScheduler sched = schedFact.GetScheduler();
                //Context ctx = new Context();

                //Get Holiday Calendar Days to Not Generate NACHA File
                //HolidayCalendar cal = new HolidayCalendar();
                //var calendar = ctx.Calendars.FirstOrDefault(c => c.CalendarCode == "NACHAHolidayCalendar");
                //foreach (var calendarDate in calendar.CalendarDates)
                //{
                //    cal.AddExcludedDate(calendarDate.SelectedDate);
                //}
                //sched.AddCalendar("myHolidays", cal, true, true);

                JobDetail jobDetail = new JobDetail("myJob", null, typeof(SettlementProcessorJob));
                Trigger trigger = null;

                if (jobScheduling.Type == "Immediate")
                {
                    //Setup trigger for NACHA file generation at 8:00 PM
                    trigger = TriggerUtils.MakeImmediateTrigger(jobScheduling.RepeatCount, new TimeSpan(jobScheduling.HourInternal, jobScheduling.MinuteInterval, jobScheduling.SecondInterval));
                }
                if (jobScheduling.Type == "Daily")
                {
                    trigger = TriggerUtils.MakeDailyTrigger(jobScheduling.Hour, jobScheduling.Minutes);
                }

                trigger.StartTimeUtc = DateTime.UtcNow;
                trigger.Name = "myTrigger";
                //trigger.CalendarName = "myHolidays";
                sched.ScheduleJob(jobDetail, trigger);

                logger.Log(LogLevel.Info, String.Format("Starting Scheduler"));
                sched.Start();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, String.Format("Exception Starting Scheduler. Exception: {0} Stack Trace: {1}", ex.Message, ex.StackTrace));
                throw ex;
            }
        }

        protected override void OnStop()
        {
        }
    }
}