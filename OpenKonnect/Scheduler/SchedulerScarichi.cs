using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using OpenKonnect.Conf;
using Quartz;
using Quartz.Impl;

namespace OpenKonnect.Scheduler
{
    public class SchedulerScarichi
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IScheduler sched;
        private readonly int defaultSecondsInterval;
        private readonly int garbageCollectorInterval;
        private readonly bool updateClocks_Active;
        private readonly DateTime updateClocks_TimeOfDay;
        private readonly int updateClocks_Interval_sec;
        private readonly int updateClocks_WithinTime_msec;

        public SchedulerScarichi(int defaultSecondsInterval,
            int garbageCollectorInterval,
            bool updateClocks_Active,
            DateTime updateClocks_TimeOfDay,
            int updateClocks_Interval_sec,
            int updateClocks_WithinTime_msec
            )
        {
            this.defaultSecondsInterval = defaultSecondsInterval;
            this.garbageCollectorInterval = garbageCollectorInterval;
            this.updateClocks_Active = updateClocks_Active;
            this.updateClocks_TimeOfDay = updateClocks_TimeOfDay;
            this.updateClocks_Interval_sec = updateClocks_Interval_sec;
            this.updateClocks_WithinTime_msec = updateClocks_WithinTime_msec;
        }
        
        public int ScheduleJobs(IEnumerable<ConfEntry> entries)
        {
            var rand = new Random();
            int totalScheduledJobs = 0;

            log.Debug("Starting scheduler.");
            
            ISchedulerFactory sf = new StdSchedulerFactory();
            sched = sf.GetScheduler();

            foreach (var e in entries)
            {
                { //fetch task
                    string taskName = "F" + e.Name;
                    IJobDetail jobScarico = JobBuilder.Create<JobScaricoTimbrature>()
                        .WithIdentity(taskName)
                        .WithDescription(taskName)
                        .UsingJobData("ip", e.IP)
                        .UsingJobData("lettore", e.Name)
                        .Build();

                    var secondsInterval = e.SecondsInterval;
                    if (secondsInterval == 0)
                        secondsInterval = defaultSecondsInterval;

                    var startDate = DateTimeOffset.Now.AddSeconds(rand.Next(e.SecondsInterval));                    
                    ITrigger triggerScarico = TriggerBuilder.Create()
                        .WithIdentity(taskName)
                        .WithDescription(taskName)
                        .StartAt(startDate)
                        .WithSchedule(
                            SimpleScheduleBuilder.Create()
                                .WithIntervalInSeconds(secondsInterval)
                                .WithMisfireHandlingInstructionNextWithRemainingCount()
                                .RepeatForever())
                        .Build();

                    log.Debug(string.Format("Scheduling fetch job {0} at {1} every {2} secs", taskName, startDate.ToString(), secondsInterval));
                    sched.ScheduleJob(jobScarico, triggerScarico);
                    totalScheduledJobs++;
                }

                if (updateClocks_Active) //clock update task
                {
                    string taskName = "C" + e.Name;
                    IJobDetail jobOrologi = JobBuilder.Create<JobAggiornamentoOrologi>()
                        .WithIdentity(taskName)
                        .WithDescription(taskName)
                        .UsingJobData("ip", e.IP)
                        .UsingJobData("lettore", e.Name)
                        .Build();

                    var startDate = DateTime.Now.Date.Add(updateClocks_TimeOfDay.TimeOfDay).AddMilliseconds(rand.Next(updateClocks_WithinTime_msec));
                    ITrigger triggerOrologi = TriggerBuilder.Create()
                        .WithIdentity(taskName)
                        .WithDescription(taskName)
                        .StartAt(startDate)
                        .WithSchedule(
                            SimpleScheduleBuilder.Create()
                                .WithIntervalInSeconds(updateClocks_Interval_sec)
                                .WithMisfireHandlingInstructionNextWithRemainingCount()
                                .RepeatForever())
                        .Build();

                    log.Debug(string.Format("Scheduling clock update job {0} at {1} every {2} secs", taskName, startDate.ToString(), updateClocks_Interval_sec));
                    sched.ScheduleJob(jobOrologi, triggerOrologi);
                    totalScheduledJobs++;
                }
            }

            sched.Start();

            log.Info(string.Format("Scheduler started. Active devices: {0}", entries.Count()));

            return totalScheduledJobs;
        }

        private void ScheduleGarbageCollection(IScheduler sched)
        {
            log.Debug(string.Format("Scheduling garbage collection job"));

            var job = JobBuilder.Create<JobGarbageCollect>()
                .WithIdentity("GC")
                .WithDescription("GC")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("GC")
                .WithDescription("GC")
                .StartNow()
                .WithSchedule(
                    SimpleScheduleBuilder.Create()
                        .WithIntervalInSeconds(garbageCollectorInterval)
                        .WithMisfireHandlingInstructionNextWithRemainingCount()
                        .RepeatForever())
                .Build();

            sched
                .ScheduleJob(job, trigger);
        }

        public void StopScheduler()
        {
            log.Info("Stopping scheduler.");
            sched.Shutdown(true);
            log.Info("Scheduler stopped.");
        }
    }
}
