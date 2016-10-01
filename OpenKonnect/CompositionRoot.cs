using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenKonnect.Scheduler;
using System.IO;
using log4net;
using OpenKonnect.Conf;
using System.Configuration;
using OpenKonnect.Configuration;

namespace OpenKonnect
{
    public class CompositionRoot
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SchedulerScarichi sched;

        public void Start()
        {
            var confFileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "openkonnect.conf");
            var confParser = new Parser();
            log.Info(string.Format("Parsing conf file {0}", confFileName));
            var entries = confParser.Parse(confFileName);
            var appConfig = new AppConfig();

            sched = new SchedulerScarichi(
                appConfig.FetchDefaultInterval_sec,
                appConfig.GarbageCollectorInterval_sec,
                appConfig.UpdateClocks_Active,
                appConfig.UpdateClocks_TimeOfDay,
                appConfig.UpdateClocks_Interval_sec,
                appConfig.UpdateClocks_WithinTime_msec);
            var totalJobsScheduled = sched.ScheduleJobs(entries);

            log.Info(string.Format("Configuration file read. Jobs scheduled: {0}", totalJobsScheduled));
        }

        public void Stop()
        {
            sched.StopScheduler();
        }
    }
}
