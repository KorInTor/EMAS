using Service.Connection;
using Model;
using Model.Event;

namespace Service
{
    public static class EventStringBuilder
    {
        private static List<Employee> employees = DataBaseClient.GetInstance().Select<Employee>().ToList();

        public readonly static Dictionary<int, string> locationIdNames = DataBaseClient.GetInstance().SelectNamedLocations();

        public static string EventToString(StorableObjectEvent storableObjectEvent)
        {
            return EventsListToStringList([storableObjectEvent]).First();
        }

        public static List<string> EventsListToStringList(List<StorableObjectEvent> storableObjectEvents)
        {
            List<string> eventsList = [];
            foreach (var storableObjectEvent in storableObjectEvents)
            {
                string employeeName = employees.Where(x => x.Id == storableObjectEvent.EmployeeId).Select(x => x.Fullname).FirstOrDefault();

                string info = $"{EventTypeToString(storableObjectEvent.EventType)}. Дата:{storableObjectEvent.DateTime}. Ответственный: {employeeName}.";

                switch (storableObjectEvent)
                {
                    case SentEvent sentEvent:
                        info = SentEventStringBuilder(sentEvent, info);
                        break;
                    case ReservedEvent reservedEvent:
                        info = ReservedEventStringBuilder(reservedEvent, info);
                        break;
                    case ArrivedEvent arrivedEvent:
                        info = ArrivedEventStringBuilder(arrivedEvent, info);
                        break;
                    case ReserveEndedEvent reserveEndedEvent:
                        info = ReserveEndedEventStringBuilder(reserveEndedEvent, info);
                        break;
                    case AdditionEvent additionEvent:
                        info = AdditionEventStringBuilder(additionEvent, info);
                        break;
                    case DecomissionedEvent decomissionedEvent:
                        info = DecomissionedEventStringBuilder(decomissionedEvent,info);
                        break;
                    //case EventType.DataChanged:
                    //    return DataChangedEventStringBuilder(storableObjectEvent, info);

                    default:
                        throw new NotImplementedException("Данный тип события ещё не поддерживается.");
                }

                info = info.Replace(". ", ".\r\n");

                eventsList.Add(info);
            }

            return eventsList;
        }

        private static string DecomissionedEventStringBuilder(DecomissionedEvent decomissionedEvent, string info)
        {
            return info + $" По причине: {decomissionedEvent.Comment}";
        }

        private static string AdditionEventStringBuilder(AdditionEvent additionEvent, string info)
        {
            return info + $" Добавлен на: {locationIdNames[additionEvent.LocationId]}";
        }

        private static string ReserveEndedEventStringBuilder(ReserveEndedEvent reserveEndedEvent, string info)
        {
            return info + $" Комментарий: {reserveEndedEvent.Comment}";
        }

        private static string ArrivedEventStringBuilder(ArrivedEvent arrivedEvent, string info)
        {
            return info + $" Комментарий: {arrivedEvent.Comment}";
        }

        private static string ReservedEventStringBuilder(ReservedEvent reservedEvent, string info)
        {
            return info + $" Зарезервирован на: {locationIdNames[reservedEvent.LocationId]}. Комментарий: {reservedEvent.Comment}";
        }

        private static string SentEventStringBuilder(SentEvent sentEvent, string info)
        {
            return info + $" Отправлен из: {locationIdNames[sentEvent.DepartureId]}. Место назначения: {locationIdNames[sentEvent.DestinationId]}. Комментарий: {sentEvent.Comment}";
        }

        public static string EventTypeToString(EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Sent:
                    return "Отправление";
                case EventType.Reserved:
                    return "Зарезервирован";
                case EventType.Arrived:
                    return "Прибытие";
                case EventType.ReserveEnded:
                    return "Конец резервации";
                case EventType.Addition:
                    return "Добавление в базу";
                case EventType.Decommissioned:
                    return "Списание";
                case EventType.DataChanged:
                    return "Информация изменена";
            }
            throw new NotImplementedException("Данный тип события ещё не поддерживается.");
        }
    }
}
