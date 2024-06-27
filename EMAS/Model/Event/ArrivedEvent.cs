using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.Event
{
    public class ArrivedEvent : StorableObjectEvent
    {
        public long SentEventId;

        public string Comment;

        public ArrivedEvent(int employeeId, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects, long sentEventId, string comment) : base(employeeId, id, eventType, dateTime, storableObjects)
        {
            SentEventId = sentEventId;
            Comment = comment;
        }
    }
}
