using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKonnect.Conf
{
    public class ConfEntry
    {
        public ConfEntry(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new SystemException("Errore nel parsing della riga del file di configurazione: " + s);

            var v = s.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (v.Length != 4)
                throw new SystemException("Numero errato di parametri nella riga di configurazione: " + s);

            Name = v[0].Trim();
            IP = Normalize(v[2].Trim());
            SecondsInterval = Convert.ToInt32(v[3]);
        }

        private string Normalize(string ip_port)
        {
            var v = ip_port.Split(new char[] { '.', ':' });
            var sb = new StringBuilder();

            sb.Append(Convert.ToInt16(v[0]));
            sb.Append('.');
            sb.Append(Convert.ToInt16(v[1]));
            sb.Append('.');
            sb.Append(Convert.ToInt16(v[2]));
            sb.Append('.');
            sb.Append(Convert.ToInt16(v[3]));
            if (v.Length == 5)
            {
                sb.Append(':');
                sb.Append(Convert.ToInt16(v[4]));
            }

            return sb.ToString();
        }

        public string Name { get; set; }
        public string IP { get; set; }
        public int SecondsInterval { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Name, IP, SecondsInterval);
        }
    }
}
