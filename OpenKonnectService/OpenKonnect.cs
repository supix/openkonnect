using System.ServiceProcess;
using log4net;
using OpenKonnect;

namespace OpenKonnectService
{
    public partial class OpenKonnect : ServiceBase
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private CompositionRoot compositionRoot;

        public OpenKonnect()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            log.Debug("Starting composition root...");
            compositionRoot = new CompositionRoot();
            compositionRoot.Start();
            log.Debug("Started");
        }

        protected override void OnStop()
        {
            log.Debug("Stopping composition root...");
            compositionRoot.Stop();
            log.Debug("Stopped");
        }
    }
}
