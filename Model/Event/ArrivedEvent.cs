using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Event
{
    public class ArrivedEvent : StorableObjectEvent
    {
        public long SentEventId;

        public string Comment;

        public ArrivedEvent(StorableObjectEvent storableObjectEvent, string comment, long sentEventId) : base(storableObjectEvent)
        {
            Comment = comment;
            SentEventId = sentEventId;
        }

        public ArrivedEvent(int employeeId, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects, string comment, long sentEventId) : base(employeeId, id, eventType, dateTime, storableObjects)
        {
            SentEventId = sentEventId;
            Comment = comment;
        }
    }
}
