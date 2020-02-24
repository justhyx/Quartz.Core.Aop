using System;
using System.Collections.Generic;
using System.Text;

namespace Quartz.Core.Aop.Extension.Attributes
{
    public class ScheduleAttribute : Attribute
    {
        public ScheduleCron Cron { get; private set; }

        public ScheduleAttribute(ScheduleCron cron)
        {
            Cron = cron;
        }
    }

    public enum ScheduleCron
    {
#if DEBUG
        Test,
#endif
        HighFrequency, Daily, Weekly
    }
}
