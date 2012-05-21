using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using SocialPayments.DataLayer;

namespace SocialPayments.SMSified.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Context _ctx = new Context();
            Logger logger = LogManager.GetCurrentClassLogger();

            DomainServices.ApplicationService applicationServices = new DomainServices.ApplicationService();
            DomainServices.SMSLogService smsLogService = new DomainServices.SMSLogService(_ctx);
            DomainServices.SMSService smsService = new DomainServices.SMSService(applicationServices, smsLogService, _ctx, logger);

            try
            {
                smsService.SendSMS(Guid.Parse("BDA11D91-7ADE-4DA1-855D-24ADFE39D174"), "2892100266", "0BE0EFAA-13F1-4830-9547-CF1203A681C1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            }
    }
}

