using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Model.Event
{
    public enum EventType
    {
        Arrived = 1,
        Sent = 2,
        Decommissioned = 3,
        Reserved = 4,
        ReserveEnded = 5,
        Addition = 6,
        DataChanged = 7
    }

	public static class EventTypeExtensions
	{
		public static Type ToType(this EventType eventType)
		{
			switch (eventType)
			{
				case EventType.Addition:
					return typeof(AdditionEvent);
				case EventType.Arrived:
					return typeof(ArrivedEvent);
				case EventType.Sent:
					return typeof(SentEvent);
				case EventType.Reserved:
					return typeof(ReservedEvent);
				case EventType.ReserveEnded:
					return typeof(ReserveEndedEvent);
				case EventType.Decommissioned:
					return typeof(DecomissionedEvent);
				case EventType.DataChanged:
					throw new NotImplementedException();
				default:
					throw new NotImplementedException();
			}
		}
	}
}
