using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using log4net;
using log4net.Config;
using System.Configuration.Install;
using System.Reflection;

namespace OpenKonnectService
{
    static class Program
    {
        private static ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            try
            {
                if (System.Environment.UserInteractive)
                {
                    string parameter = string.Concat(args);
                    switch (parameter)
                    {
                        case "--install":
                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                            break;
                        case "--uninstall":
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                            break;
                    }
                }
                else
                {
                    ServiceBase.Run(new OpenKonnect());
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.GetType().ToString(), ex);
                throw;
            }
        }
    }
}
