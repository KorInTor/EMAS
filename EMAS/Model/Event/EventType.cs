using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.Event
{
    public enum EventType
    {
        Arrived = 1,
        Sent = 2,
        Decommissioned = 3,
        Reserved = 4,
        ReserveEnded = 5,
        Addition = 6
    }
}
