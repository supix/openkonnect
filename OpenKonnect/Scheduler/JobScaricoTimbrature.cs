using System;
using System.Configuration;
using System.Diagnostics;
using log4net;
using OpenKonnect.Domain;
using OpenKonnect.Persistence.Concrete;
using OpenKonnect.ProtocolloLettore.Abstract;
using OpenKonnect.ProtocolloLettore.Concrete;
using Quartz;

namespace OpenKonnect.Scheduler
{
    [DisallowConcurrentExecution]
    class JobScaricoTimbrature : IJob
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Execute(IJobExecutionContext context)
        {
            var sw = new Stopwatch();
            try
            {
                sw.Start();
                var idLettore = (string)context.JobDetail.JobDataMap["lettore"];
                ThreadContext.Properties["ID"] = idLettore;

                log.Debug(string.Format("Starting Task: {0}", context.JobDetail.Description));
            
                var connectionString = ConfigurationManager.AppSettings["ConnectionString"];
                var fakeMode = Convert.ToBoolean(ConfigurationManager.AppSettings["FakeMode"]);
                var safeMode = Convert.ToBoolean(ConfigurationManager.AppSettings["SafeMode"]);

                var dbAppender = new MySqlDbAppender(connectionString);
                var lettore = fakeMode ?
                    (ILettoreTimbrature)new LettoreFake() :
                    (ILettoreTimbrature)new LettoreKronotech((string)context.JobDetail.JobDataMap["ip"], idLettore, safeMode);
                using (lettore)
                {
                    Timbratura timb;
                    while ((timb = lettore.GetProssimaTimbratura()) != null)
                        dbAppender.Insert(timb);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.GetType().ToString(), ex);
                var e2 = new JobExecutionException(ex, false);
                throw e2;
            }

            sw.Stop();
            log.Debug(string.Format("Task executed in {0} msec", sw.ElapsedMilliseconds));
        }
    }
}
