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
    public class LettoreKronotech : ILettoreTimbrature, IImpostazioneOrario
    {
        private const int DefaultPort = 3000;
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly string ip;
        private readonly string name;
        private readonly bool safeMode;

        private TcpClient tcpClient = null;
        private bool primaTimbratura;
        private int timbratureTotali;
        private int idxTimbraturaCorrente;
        private string idUltimaTimbratura;
        private Crc8Calculator crc = new Crc8Calculator();
        
        public LettoreKronotech(string ip, string name, bool safeMode)
        {
            this.ip = ip;
            this.name = name;
            this.safeMode = safeMode;
            this.primaTimbratura = true;
        }

        private void Connect()
        {
            if ((tcpClient == null) || (!tcpClient.Connected))
            {
                int port = DefaultPort;
                var v = ip.Split(':');
                if (v.Length > 1)
                    port = Convert.ToInt16(v[1]);
                tcpClient = new TcpClient();
                tcpClient.Connect(v[0], port);
                log.Debug("Connessione eseguita!");
            }
        }
        
        public Timbratura GetProssimaTimbratura()
        {
            Connect();

            if (primaTimbratura)
                return InizializzaERestituisciPrima();
            else
                return RestituisciSuccessiva();
        }

        private Timbratura RestituisciSuccessiva()
        {
            if (safeMode)
                return null;

            CancellaTimbratura();
            idxTimbraturaCorrente++;
            if (idxTimbraturaCorrente < timbratureTotali)
                return GetTimbratura();
            else
                return null;
        }

        private void CancellaTimbratura()
        {
            if (string.IsNullOrEmpty(idUltimaTimbratura))
                return;

            var bytes = new byte[] { 0x01, 0x30, 0x31, 0x02, 0x30, 0x32, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x0d };

            //sovrascrivi nel buffer l'id timbratura
            for (int i = 0; i < 4; i++)
            {
                var b = (byte)idUltimaTimbratura[i];
                bytes[i + 6] = b;
            }

            //sovrascrivi nel buffer la checksum
            var chk = crc.ComputeChecksum(bytes.Skip(4).Take(6).ToArray());
            bytes[11] = chk;

            tcpClient.Client.Send(bytes);
            var receiveBuffer = ReceivePacket();

            log.Info(string.Format("Timbratura cancellata [{0}]", idUltimaTimbratura));
            idUltimaTimbratura = string.Empty;
        }

        private Timbratura InizializzaERestituisciPrima()
        {
            timbratureTotali = GetTimbraturePendenti();
            log.Info(string.Format("Trovate {0} timbrature", timbratureTotali));
            idxTimbraturaCorrente = 0;
            primaTimbratura = false;
            if (idxTimbraturaCorrente < timbratureTotali)
                return GetTimbratura();
            else
                return null;
        }

        private Timbratura GetTimbratura()
        {
            byte[] receiveBuffer = null;
            try
            {
                var bytes = new byte[] { 0x01, 0x30, 0x31, 0x02, 0x30, 0x31, 0x03, 0x00, 0x0d };
                tcpClient.Client.Send(bytes);
                receiveBuffer = ReceivePacket();

                if ((receiveBuffer[3] != 0x02) ||
                    (receiveBuffer[4] != 0x30) ||
                    (receiveBuffer[5] != 0x31))
                    throw new SystemException("Unknown response");

                idUltimaTimbratura = ReadBufferToString(receiveBuffer, 6, 4);

                //verifica che il verso sia 0x30 o 0x31: altrimenti logga e imposta a Entrata
                Timbratura.Verso verso;
                if (receiveBuffer[11] == 0x30)
                    verso = Timbratura.Verso.Entrata;
                else
                    if (receiveBuffer[11] == 0x31)
                        verso = Timbratura.Verso.Uscita;
                    else
                    {
                        log.WarnFormat("Verso sconosciuto: {0}", receiveBuffer[11]);
                        verso = Timbratura.Verso.Entrata;
                    }

                var badge = ReadBufferToString(receiveBuffer, 12, 10);
                var anno = "20" + ReadBufferToString(receiveBuffer, 22, 2);
                var mese = ReadBufferToString(receiveBuffer, 24, 2);
                var giorno = ReadBufferToString(receiveBuffer, 26, 2);
                var ora = ReadBufferToString(receiveBuffer, 28, 2);
                var minuto = ReadBufferToString(receiveBuffer, 30, 2);
                var causale = ReadBufferToString(receiveBuffer, 35, 3);

                //verifica che la causale sia formata da interi: altrimenti logga e imposta a 99
                int i_causale;
                if (!int.TryParse(causale, out i_causale))
                {
                    log.WarnFormat("Causale sconosciuta: {0}", causale);
                    i_causale = 99;
                }

                var t = new Timbratura()
                {
                    VersoTimb = verso,
                    CodiceInterno = badge,
                    DateTime = new DateTime(Convert.ToInt16(anno), Convert.ToInt16(mese), Convert.ToInt16(giorno), Convert.ToInt16(ora), Convert.ToInt16(minuto), Convert.ToInt16(0)),
                    Causale = i_causale,
                    IdLettore = name
                };

                log.Info(string.Format("Timbratura scaricata [{0}]: {1}", idUltimaTimbratura, t));

                return t;
            }
            catch (Exception ex)
            {
                if (receiveBuffer != null)
                    throw new SystemException(string.Format("Unsupported packet: {0}", ReadBufferToBytes(receiveBuffer, 0, receiveBuffer.Length)), ex);
                else
                    throw;
            }
        }

        private string ReadBufferToString(byte[] receivedBytes, int startIndex, int count)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
                sb.Append((char)receivedBytes[i + startIndex]);
            
            return sb.ToString();
        }

        private string ReadBufferToBytes(byte[] receivedBytes, int startIndex, int count)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(receivedBytes[i + startIndex].ToString("X2"));
                sb.Append(' ');
            }

            return sb.ToString();
        }

        private int GetTimbraturePendenti()
        {
            var bytes = new byte[] { 0x01, 0x30, 0x31, 0x02, 0x30, 0x30, 0x03, 0x02, 0x0d };
            tcpClient.Client.Send(bytes);
            var receiveBuffer = ReceivePacket();
            var s = ReadBufferToString(receiveBuffer, 6, 4);
            return Convert.ToInt32(s, 16);
        }

        public void Dispose()
        {
            if (tcpClient != null)
            {
                if (tcpClient.Connected)
                {
                    var ns = tcpClient.GetStream();

                    if (ns != null)
                        ns.Close();
                }
                
                tcpClient.Close();
                log.Debug("Disconnected");
            }
        }

        private byte[] ReceivePacket()
        {
            const int maxWaitSeconds = 30;
            var receiveBuffer = new byte[512];
            var totBytes = 0;

            bool crReceived = false;
            var timer = new Stopwatch();
            timer.Start();
            do
            {
                if (totBytes == receiveBuffer.Length)
                    throw new SystemException("Buffer overflow");

                if (tcpClient.Client.Poll((int)(200 * 1e3), SelectMode.SelectRead)) //verifica se ci sono bytes da leggere
                {
                    var receivedBytes = tcpClient.Client.Receive(receiveBuffer, totBytes, receiveBuffer.Length - totBytes, SocketFlags.None);
                    totBytes += receivedBytes;
                    crReceived = receiveBuffer[totBytes - 1] == 0x0d;
                }
            } while (!crReceived && (timer.Elapsed.TotalSeconds < maxWaitSeconds));

            timer.Stop();
            if (!crReceived)
                throw new SystemException(string.Format("Cannot receive packet closing byte (0x0d) after {0} milliseconds", timer.ElapsedMilliseconds));

            var packet = receiveBuffer.Take(totBytes).ToArray();

            if ((packet[0] != 0x01) ||
                (packet[1] != 0x30) ||
                (packet[2] != 0x31) ||
                (packet[3] != 0x06) ||
                (packet[4] != 0x06) ||
                (packet[5] != 0x0d))
                Checksum(packet);

            return packet;
        }

        private void Checksum(byte[] packet, bool set = false)
        {
            var sot = Array.IndexOf(packet, (byte)0x02);
            var eot = Array.IndexOf(packet, (byte)0x03);

            if ((sot < 0) || (eot < 0) || (eot <= sot))
                throw new SystemException("Malformed packet");

            var actualChk = crc.ComputeChecksum(packet.Skip(sot + 1).Take(eot - sot - 1).ToArray());

            if (!set)
            {
                var expectedChecksum = packet[packet.Length - 2];

                if (actualChk != expectedChecksum)
                    throw new SystemException("Wrong checksum");
            }
            else
            {
                packet[packet.Length - 2] = actualChk;
            }
        }

        public void ImpostaOrario()
        {
            this.ImpostaOrario(DateTime.Now);
        }

        public void ImpostaOrario(DateTime dateTime)
        {
            if (safeMode)
            {
                log.Debug("SafeMode enabled: time not set.");
                return;
            }

            Connect();

            var s_dateTime = dateTime.ToString("yyyyMMddHHmmss");
            byte[] b_dateTime = s_dateTime.Select<char, byte>(c => Convert.ToByte(c)).ToArray();
            var bytes = new byte[] { 0x01, 0x30, 0x31, 0x02, 0x32, 0x39, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x03, 0x00, 0x0d };
            Array.Copy(b_dateTime, 0, bytes, 6, b_dateTime.Length);
            Checksum(bytes, true);
            tcpClient.Client.Send(bytes);

            var receiveBuffer = ReceivePacket();

            if (receiveBuffer.Length != 6)
                throw new SystemException("Wrong response packet while setting date time");

            log.InfoFormat("Orologio aggiornato alla data {0} {1}", dateTime.ToShortDateString(), dateTime.ToLongTimeString());
        }
    }
}
