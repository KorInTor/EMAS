using EMAS.Service.Connection;

namespace EMAS.Model.Event
{
    public static class EventStringBuilder
    {
        private static List<Employee> employees = DataBaseClient.GetInstance().SelectEmployee();

        private static Dictionary<int, string> locationIdNames = DataBaseClient.GetInstance().SelectNamedLocations();

        public static string EventToString(StorableObjectEvent storableObjectEvent)
        {
            string employeeName = employees.Where(x => x.Id == storableObjectEvent.EmployeeId).Select(x => x.Fullname).FirstOrDefault();

            string info = $"{EventTypeToString(storableObjectEvent.EventType)}. Дата:{storableObjectEvent.DateTime}. Ответственный: {employeeName}.";

            switch (storableObjectEvent)
            {
                case SentEvent sentEvent:
                    return SentEventStringBuilder(sentEvent, info);
                case ReservedEvent reservedEvent:
                    return ReservedEventStringBuilder(reservedEvent, info);
                case ArrivedEvent arrivedEvent:
                    return ArrivedEventStringBuilder(arrivedEvent, info);
                case ReserveEndedEvent reserveEndedEvent:
                    return ReserveEndedEventStringBuilder(reserveEndedEvent, info);
                case AdditionEvent additionEvent:
                    return AdditionEventStringBuilder(additionEvent, info);
                    //case EventType.Decommissioned:
                    //    return DecommissionedEventStringBuilder(storableObjectEvent, info);
                    //case EventType.DataChanged:
                    //    return DataChangedEventStringBuilder(storableObjectEvent, info);
            }
            throw new NotImplementedException("Данный тип события ещё не поддерживается.");
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

        private static string EventTypeToString(EventType eventType)
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
                    return "Инофрмация изменена";
            }
            throw new NotImplementedException("Данный тип события ещё не поддерживается.");
        }
    }
}
