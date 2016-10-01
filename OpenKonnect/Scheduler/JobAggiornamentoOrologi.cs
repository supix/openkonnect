using System;
using System.Diagnostics;
using log4net;
using OpenKonnect.ProtocolloLettore.Concrete;
using Quartz;
using System.Configuration;
using OpenKonnect.ProtocolloLettore.Abstract;

namespace OpenKonnect.Scheduler
{
    [DisallowConcurrentExecution]
    class JobAggiornamentoOrologi : IJob
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

                log.Debug(string.Format("Updating clock: {0}", context.JobDetail.Description));

                var fakeMode = Convert.ToBoolean(ConfigurationManager.AppSettings["FakeMode"]);
                var safeMode = Convert.ToBoolean(ConfigurationManager.AppSettings["SafeMode"]);

                var lettore = fakeMode ?
                    (IImpostazioneOrario)new LettoreFake() :
                    (IImpostazioneOrario)new LettoreKronotech((string)context.JobDetail.JobDataMap["ip"], idLettore, safeMode);
                using (lettore)
                {
                    lettore.ImpostaOrario();
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.GetType().ToString(), ex);
                var e2 = new JobExecutionException(ex, false);
                throw e2;
            }

            sw.Stop();
            log.Debug(string.Format("Clock updated in {0} msec", sw.ElapsedMilliseconds));
        }
    }
}
