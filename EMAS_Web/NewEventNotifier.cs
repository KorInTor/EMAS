using Microsoft.AspNetCore.SignalR;
using Model;
using Model.Event;
using Newtonsoft.Json.Linq;
using Service.Connection;
using Service.Connection.DataAccess.Query;

namespace EMAS_Web
{
    public class NewEventNotifier : BackgroundService, IObserver<StorableObjectEvent>
    {
        private readonly DataBaseClient _dataBaseClient;
        private readonly IHubContext<EventsHub> _hubContext;
        public NewEventNotifier(DataBaseClient dataBaseClient, IHubContext<EventsHub> hubContext)
        {
            _dataBaseClient = dataBaseClient;
            _hubContext = hubContext;
            _dataBaseClient.Subscribe(this);
        }

        public void OnCompleted()
        {
            Console.WriteLine("Event stream completed.");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine($"Error: {error.Message}");
        }

        public void OnNext(StorableObjectEvent value)
        {
            _hubContext.Clients.All.SendAsync("ReceiveNewEvent", JsonConverter.EventToJObject(value).ToString());
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _dataBaseClient.Unsubscribe(this);
            return base.StopAsync(cancellationToken);
        }

    }

    public static class JsonConverter
    {
        public static JObject EventToJObject(StorableObjectEvent StorableObjectEvent)
        {
            var responcible = DataBaseClient.GetInstance().SelectSingleById<Employee>(StorableObjectEvent.EmployeeId, nameof(Employee.Id));
            var locationsDictionary = DataBaseClient.GetInstance().SelectNamedLocations();
            var jObject = JObject.FromObject(StorableObjectEvent);
            jObject["employeeInfo"] = $"{responcible.Fullname}\n{responcible.Email}";

            switch (StorableObjectEvent)
            {
                case SentEvent sentEvent:
                    jObject["DepartureName"] = locationsDictionary[sentEvent.DepartureId];
                    jObject["DestinationName"] = locationsDictionary[sentEvent.DestinationId];
                    break;
                case ReservedEvent reservedEvent:
                    jObject["LocationName"] = locationsDictionary[reservedEvent.LocationId];
                    break;
                case ArrivedEvent arrivedEvent:
                    var queryBuilder = new QueryBuilder();
                    queryBuilder.Where($"{nameof(SentEvent)}.{nameof(SentEvent.Id)}", "=", arrivedEvent.SentEventId);
                    SentEvent endedSentEvent = DataBaseClient.GetInstance().SelectEvent<SentEvent>(queryBuilder).First();
                    jObject["DepartureName"] = locationsDictionary[endedSentEvent.DepartureId];
                    jObject["DestinationName"] = locationsDictionary[endedSentEvent.DestinationId];
                    break;
                case ReserveEndedEvent reserveEndedEvent:
                    var queryBuilderReserveEvent = new QueryBuilder();
                    queryBuilderReserveEvent.Where($"{nameof(ReservedEvent)}.{nameof(ReservedEvent.Id)}", "=", reserveEndedEvent.ReserveEventId);
                    ReservedEvent endedReserveEvent = DataBaseClient.GetInstance().SelectEvent<ReservedEvent>(queryBuilderReserveEvent).First();
                    jObject["LocationName"] = locationsDictionary[endedReserveEvent.LocationId];
                    break;
                case AdditionEvent additionEvent:
                    jObject["LocationName"] = locationsDictionary[additionEvent.LocationId];
                    break;
                case DecomissionedEvent decomissionedEvent:
                    jObject["LocationName"] = locationsDictionary[decomissionedEvent.LocationId];
                    break;
                //case EventType.DataChanged:
                //    return DataChangedEventStringBuilder(storableObjectEvent, info);

                default:
                    throw new NotImplementedException("Данный тип события ещё не поддерживается.");
            }

            return jObject;
        }
    }
}
