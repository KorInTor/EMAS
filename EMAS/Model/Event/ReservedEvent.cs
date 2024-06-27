using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.Event
{
    public class ReservedEvent : StorableObjectEvent
    {
        public int LocationId;

        public string Comment;

        public ReservedEvent(int employeeId, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects, int locationId, string comment) : base(employeeId, id, eventType, dateTime, storableObjects)
        {
            LocationId = locationId;
            Comment = comment;
        }
    }
}
