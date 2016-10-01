using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;

namespace OpenKonnect.Conf
{
    public class Parser
    {
        private readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IEnumerable<ConfEntry> Parse(string confFileName)
        {
            char[] commentDelimiters = new char[] { '#', ';' };
            int line = 0;

            using (var stream = new StreamReader(confFileName))
            {
                string s;
                while ((s = stream.ReadLine()) != null)
                {
                    line++;
                    //scarto i commenti
                    if (string.IsNullOrWhiteSpace(s))
                        continue;

                    s = s.Trim();

                    if (commentDelimiters.Contains(s[0]))  //riga di commento
                        continue;

                    var commentDelimiterIndex = s.IndexOfAny(commentDelimiters);
                    if (commentDelimiterIndex > -1)
                    {
                        s = s.Substring(0, commentDelimiterIndex);
                    }

                    ConfEntry ce = null;
                    try
                    {
                        ce = new ConfEntry(s);
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Errore alla riga {0} del file di configurazione.", line), ex);
                        throw;
                    }

                    yield return ce;
                }
            }
        }
    }
}
