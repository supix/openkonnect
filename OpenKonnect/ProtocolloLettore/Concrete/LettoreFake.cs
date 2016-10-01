using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenKonnect.ProtocolloLettore.Abstract;
using System.Net.Sockets;
using OpenKonnect.Domain;
using log4net;
using System.Diagnostics;

namespace OpenKonnect.ProtocolloLettore.Concrete
{
    public class LettoreFake : ILettoreTimbrature, IImpostazioneOrario
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int totalStamps = 0;
        private int returnedStamps = 0;

        public Timbratura GetProssimaTimbratura()
        {
            if (totalStamps == 0)
            {
                totalStamps = Faker.RandomNumber.Next(0, 5);
                log.DebugFormat("{0} total stamps will be returned", totalStamps);
            }

            if (returnedStamps++ < totalStamps)
            {
                var t = new Timbratura()
                {
                    Causale = Faker.RandomNumber.Next(0, 100),
                    CodiceInterno = Faker.RandomNumber.Next(1000000, 10000000).ToString(),
                    DateTime = DateTime.Now.AddSeconds(Faker.RandomNumber.Next(-600, 0)),
                    IdLettore = Faker.RandomNumber.Next(0, 1000).ToString(),
                    VersoTimb = Faker.RandomNumber.Next(0, 2) == 0 ? Timbratura.Verso.Entrata : Timbratura.Verso.Uscita
                };

                log.InfoFormat("Timbratura scaricata: {0}", t);
                return t;
            }

            return null;
        }

        public void ImpostaOrario()
        {
            log.Info("Clock set");
        }

        public void ImpostaOrario(DateTime dateTime)
        {
            log.InfoFormat("Clock set to date {0}", dateTime.ToString());
        }

        public void Dispose()
        {
            log.Debug("Disposed");
        }
    }
}
