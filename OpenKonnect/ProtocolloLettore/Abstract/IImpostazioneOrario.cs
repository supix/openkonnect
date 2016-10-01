using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKonnect.ProtocolloLettore.Abstract
{
    public interface IImpostazioneOrario : IDisposable
    {
        void ImpostaOrario();
        void ImpostaOrario(DateTime dateTime);
    }
}
