using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.Event
{
    public class ReservedEvent : StorableObjectEvent
    {
        public string Comment { get; set; }

        public int LocationId;

        public ReservedEvent()
        {
        }

        public ReservedEvent(StorableObjectEvent storableObjectEvent, string comment, int locationId) : base(storableObjectEvent)
        {
            LocationId = locationId;
            Comment = comment;
        }

        public ReservedEvent(int employee, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects, string comment, int locationId) : base(employee, id, eventType, dateTime, storableObjects)
        {
            LocationId = locationId;
            Comment = comment;
        }
    }
}
