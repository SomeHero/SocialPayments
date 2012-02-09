using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NLog;
using Quartz.Impl.Calendar;
using Quartz;
using SocialPayments.DataLayer;
using Quartz.Impl;
using SocialPayments.Jobs.ProcessOpenPaymentsJob;

namespace SocialPayments.WindowsServices.OpenPaymentsScheduler
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
            logger.Log(LogLevel.Info, String.Format("Starting Open Payments Processor"));

            // construct a scheduler factory
            ISchedulerFactory schedFact = new StdSchedulerFactory();

            IScheduler sched = schedFact.GetScheduler();
            Context ctx = new Context();

            JobDetail jobDetail = new JobDetail("myJob", null, typeof(ProcessOpenPaymentJob));

            Trigger trigger = TriggerUtils.MakeDailyTrigger(5, 00);

            trigger.StartTimeUtc = DateTime.UtcNow;
            trigger.Name = "myTrigger1";
            sched.ScheduleJob(jobDetail, trigger);

            try
            {
                logger.Log(LogLevel.Info, String.Format("Starting Scheduler"));
                sched.Start();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Info, String.Format("Exception Starting Scheduler {0}", ex.Message));
                throw;
            }
        }

        protected override void OnStop()
        {
        }
    }
}
