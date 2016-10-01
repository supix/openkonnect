using System;
using System.Diagnostics;
using log4net;
using Quartz;

namespace OpenKonnect.Scheduler
{
    [DisallowConcurrentExecution]
    class JobGarbageCollect : IJob
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Execute(IJobExecutionContext context)
        {
            var sw = new Stopwatch();
            try
            {
                sw.Start();
                GC.Collect();
            }
            catch (Exception ex)
            {
                log.Error(ex.GetType().ToString(), ex);
                var e2 = new JobExecutionException(ex, false);
                throw e2;
            }

            sw.Stop();
            log.Debug(string.Format("Garbage collection execute in {0} msec", sw.ElapsedMilliseconds));
        }
    }
}
