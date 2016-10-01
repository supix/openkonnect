using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenKonnect.Domain;

namespace OpenKonnect.ProtocolloLettore.Abstract
{
    public interface ILettoreTimbrature : IDisposable
    {
        Timbratura GetProssimaTimbratura();
    }
}
