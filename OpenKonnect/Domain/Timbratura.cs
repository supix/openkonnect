using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKonnect.Domain
{
    public class Timbratura
    {
        public enum Verso { Entrata, Uscita }

        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string CodiceInterno { get; set; }
        public Verso VersoTimb { get; set; }
        public int Causale { get; set; }
        public string IdLettore { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5}", 
                IdLettore,
                DateTime.ToShortDateString(), 
                DateTime.ToShortTimeString(), 
                CodiceInterno,
                VersoTimb,
                Causale);
        }

        public char VersoToChar()
        {
            switch (VersoTimb)
            {
                case Verso.Entrata: return 'I';
                case Verso.Uscita: return 'O';
                default: throw new NotSupportedException("Unsupported direction");
            }
        }
        public static Verso CharToVerso(char c)
        {
            switch (c)
            {
                case 'E': return Verso.Entrata;
                case 'U': return Verso.Uscita;
                default: throw new NotSupportedException("Verso non supportato");
            }
        }
    }
}
