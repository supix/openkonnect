using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenKonnect.Domain;

namespace OpenKonnect.Persistence.Abstract
{
    public interface IDbAppender
    {
        void Insert(Timbratura t);
    }
}
