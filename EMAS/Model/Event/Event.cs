using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.Event
{
    public class Event(int employeeId, long id, EventType eventType, int objectId)
    {
        public int EmployeeId { get; set; } = employeeId;
        public long Id { get; set; } = id;
        public EventType EventType { get; set; } = eventType;
        public int ObjectId { get; set; } = objectId;
    }
}
