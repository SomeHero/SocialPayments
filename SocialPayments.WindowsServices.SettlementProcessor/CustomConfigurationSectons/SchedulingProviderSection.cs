using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SocialPayments.WindowsServices.SettlementProcessor.CustomConfigurationSectons
{
    public class JobSchedulingProviderSection : ConfigurationSection
    {
        public static readonly JobSchedulingProviderSection Current = (JobSchedulingProviderSection)ConfigurationManager.GetSection("jobSchedulingProvider");

        [ConfigurationProperty("SchedulingType", DefaultValue="Daily")]
        public string Type
        {
            get { return (String)base["SchedulingType"]; }
            set { base["SchedulingType"] = value; }
        }

        [ConfigurationProperty("SpecificHour", DefaultValue="22")]
        public int Hour
        {
            get { return (int)base["SpecificHour"]; }
            set { base["SpecificHour"] = value; }
        }
        [ConfigurationProperty("SpecificMinutes", DefaultValue = "30")]
        public int Minutes
        {
            get { return (int)base["SpecificMinutes"]; }
            set { base["SpecificMinutes"] = value; }
        }

        [ConfigurationProperty("RepeatCount", DefaultValue = "0")]
        public int RepeatCount
        {
            get { return (int)base["RepeatCount"]; }
            set { base["RepeatCount"] = value; }
        }
        [ConfigurationProperty("HourInterval", DefaultValue = "0")]
        public int HourInternal
        {
            get { return (int)base["HourInterval"]; }
            set { base["HourInterval"] = value; }
        }
        [ConfigurationProperty("MinuteInterval", DefaultValue = "0")]
        public int MinuteInterval
        {
            get { return (int)base["MinuteInterval"]; }
            set { base["MinuteInterval"] = value; }
        }
        [ConfigurationProperty("SecondInterval", DefaultValue = "0")]
        public int SecondInterval
        {
            get { return (int)base["SecondInterval"]; }
            set { base["SecondInterval"] = value; }
        }

    }
}
