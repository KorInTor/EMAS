using DocumentFormat.OpenXml.EMMA;
using Model;
using Model.Event;
using Service.Connection.DataAccess;
using Service.Connection.DataAccess.Query;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace Service.Connection
{
	public class DataBaseClient
	{
		private StorableObjectDataAccess storableObjectDataAccess;
		private EmployeeDataAccess employeeDataAccess;
		private LocationDataAccess locationDataAccess;

		private EventDataAccess eventDataAccess;

		private static DataBaseClient instance;

		private DataBaseClient()
		{
			storableObjectDataAccess = new StorableObjectDataAccess();
			employeeDataAccess = new EmployeeDataAccess();
			locationDataAccess = new LocationDataAccess();
			eventDataAccess = new EventDataAccess();
		}

		public static DataBaseClient GetInstance()
		{
			instance ??= new DataBaseClient();
			return instance;
		}

		public void Add(object objectToAdd)
		{
			if (objectToAdd is Employee newEmployee)
			{
				employeeDataAccess.Add(newEmployee);
				return;
			}

			if (objectToAdd is Location newLocation)
			{
				locationDataAccess.Add(newLocation);
				return;
			}

			if (objectToAdd is StorableObjectEvent objectEvent)
			{
				eventDataAccess.Add(objectEvent);
				return;
			}

			if (objectToAdd is IStorableObject)
			{
				throw new ArgumentException("Use AdditionEvent for adding new StorableObject to dataBase");
			}

			throw new NotSupportedException("Этот тип не поддерживается");
		}

		public void Add(IEnumerable<object> objectToAdd)
		{
			if (objectToAdd is IEnumerable<Employee> newEmployee)
			{
				employeeDataAccess.Add(newEmployee.ToArray());
				return;
			}

			if (objectToAdd is IEnumerable<Location> newLocation)
			{
				locationDataAccess.Add(newLocation.ToArray());
				return;
			}

			if (objectToAdd is IEnumerable<StorableObjectEvent> newEvents)
			{
				eventDataAccess.Add(newEvents);
				return;
			}

			if (objectToAdd is IEnumerable<IStorableObject>)
			{
				throw new ArgumentException("Use AdditionEvent for adding new StorableObject to dataBase");
			}

			throw new NotSupportedException("Этот тип не поддерживается");
		}

		public void Update(object objectToUpdate)
		{
			if (objectToUpdate is Equipment newEquipment)
			{
				storableObjectDataAccess.Update(newEquipment);
				return;
			}
			if (objectToUpdate is Employee newEmployee)
			{
				employeeDataAccess.Update(newEmployee);
				return;
			}
			if (objectToUpdate is Location newLocation)
			{
				locationDataAccess.Update(newLocation);
				return;
			}
			throw new NotSupportedException("Этот тип не поддерживается");
		}

		public bool IsStorableObjectsNotOccupied(IEnumerable<IStorableObject> storableObjects, out List<IStorableObject> occupiedObject)
		{
			return eventDataAccess.IsStorableObjectsNotOccupied(storableObjects, out occupiedObject);
		}

		public Dictionary<int, StorableObjectEvent> SelectLastEventsForStorableObjects(IEnumerable<IStorableObject> storableObjects)
		{
			return eventDataAccess.SelectLastEventsForStorableObjects(storableObjects.Select(x => x.Id));
		}

		public List<TEvent> SelectEventsByIds<TEvent>(IEnumerable<long> eventId) where TEvent : StorableObjectEvent
		{
			QueryBuilder queryBuilder = new();
			queryBuilder.Init<TEvent>().Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.Id)}","=",eventId);

			return eventDataAccess.Select<TEvent>(queryBuilder).ToList();
		}

		public List<SentEvent> SelectDeliveries(int locationId, bool selectIncoming, bool selectActive = true)
		{
			QueryBuilder queryBuilder = new ();
			queryBuilder.Init<SentEvent>();

			if (selectIncoming)
				queryBuilder.Where($"{nameof(SentEvent)}.{nameof(SentEvent.DestinationId)}", "=", locationId);
			else
				queryBuilder.Where($"{nameof(SentEvent)}.{nameof(SentEvent.DepartureId)}", "=", locationId);

			if (selectActive)
				queryBuilder.AndWhere($"{nameof(ArrivedEvent)}.{nameof(ArrivedEvent.Id)}", "IS", null);

			IEnumerable<SentEvent> activeDeliveries = eventDataAccess.Select<SentEvent>(queryBuilder);

            return activeDeliveries.ToList();
		}

		public List<StorableObjectEvent> SelectLocationBoundedEvents(int locationId, DateTime? floorDateValue = null, DateTime? ceilingDateValue = null)
		{
			List<StorableObjectEvent> events = [];
			Stopwatch stopwatch = new();
			bool useNewEventAccess = false;

			if (locationId == 0)
			{
				throw new ArgumentException("Missing Location Id");
			}

			var queryBuilder = new QueryBuilder();
			queryBuilder.Init<AdditionEvent>().Where($"{nameof(AdditionEvent)}.{nameof(AdditionEvent.LocationId)}", "=", locationId);
			events.AddRange(eventDataAccess.Select<AdditionEvent>(queryBuilder));

			queryBuilder = new QueryBuilder();
			queryBuilder
				.Init<SentEvent>()
				.Where($"{nameof(SentEvent)}.{nameof(SentEvent.DepartureId)}", "=", locationId)
				.OrWhere($"{nameof(SentEvent)}.{nameof(SentEvent.DestinationId)}", "=", locationId);

			events.AddRange(eventDataAccess.Select<SentEvent>(queryBuilder));

			queryBuilder = new QueryBuilder();
			queryBuilder
				.Init<ArrivedEvent>()
				.Where($"{nameof(SentEvent)}.{nameof(SentEvent.DepartureId)}", "=", locationId)
				.OrWhere($"{nameof(SentEvent)}.{nameof(SentEvent.DestinationId)}", "=", locationId);

			events.AddRange(eventDataAccess.Select<ArrivedEvent>(queryBuilder));

			queryBuilder = new QueryBuilder();
			queryBuilder
				.Init<ReservedEvent>()
				.Where($"{nameof(ReservedEvent)}.{nameof(ReservedEvent.LocationId)}", "=", locationId);
			events.AddRange(eventDataAccess.Select<ReservedEvent>(queryBuilder));

			queryBuilder = new QueryBuilder();
			queryBuilder
				.Init<ReserveEndedEvent>()
				.Where($"{nameof(ReservedEvent)}.{nameof(ReservedEvent.LocationId)}", "=", locationId);
			events.AddRange(eventDataAccess.Select<ReserveEndedEvent>(queryBuilder));

			return events;
		}

        public List<IStorableObject> SelectStorableObjectOn(int locationId)
		{
			return storableObjectDataAccess.SelectOnLocation(locationId).ToList();
		}

		public List<IStorableObject> SelectStorableObjectsByIds(IEnumerable<int> ids)
		{
			return storableObjectDataAccess.SelectByIds(ids).ToList();
		}

		public List<ReservedEvent> SelectReservationOn(int locationId, bool selectOnlyActive = true)
		{
			var queryBuilder = new QueryBuilder();
			queryBuilder.Init<ReservedEvent>().Where($"{nameof(ReservedEvent)}.{nameof(ReservedEvent.LocationId)}","=",locationId);
			if (selectOnlyActive)
				queryBuilder.AndWhere($"{nameof(ReserveEndedEvent)}.{nameof(ReserveEndedEvent.Id)}", "IS", null);

			return eventDataAccess.Select<ReservedEvent>(queryBuilder).ToList();
		}
		
		public Dictionary<string,List<string>> SelectDistinctPropertyValues(Type objectToSelectPropertyFor, IEnumerable<string>? properties = null)
		{
			if(objectToSelectPropertyFor == typeof(Equipment))
			{
				return storableObjectDataAccess.equipmentDataAccess.SelectDistinct(properties);
			}

			throw new NotImplementedException();
		}

		public List<Employee> SelectEmployee()
		{
			return employeeDataAccess.Select();
		}

		public Employee? SelectEmployee(int employeeId)
		{
			return employeeDataAccess.SelectById(employeeId);
		}

		public List<Location> SelectLocations()
		{
			return locationDataAccess.Select();
		}

		public Dictionary<int, string> SelectNamedLocations()
		{
			Dictionary<int, string> namedLocations = [];
			foreach (Location location in SelectLocations())
			{
				namedLocations.Add(location.Id, location.Name);
			}
			return namedLocations;
		}

		public Dictionary<int, List<StorableObjectEvent>> SelectForStorableObjectsIds(IEnumerable<int> storableObjectIds)
		{
			return eventDataAccess.SelectEventsForStorableObjectsIds(storableObjectIds);
		}

		public IEnumerable<TEvent> SelectEventsCustom<TEvent>(QueryBuilder queryBuilder) where TEvent : StorableObjectEvent
		{
			return eventDataAccess.Select<TEvent>(queryBuilder);
		}

		public Dictionary<int,string> SelectEquipmentStatuses()
		{
			return storableObjectDataAccess.equipmentDataAccess.SelectStatuses();
		}

		public void UpdateEquipmentStatuses(IEnumerable<(int,string)> statuses)
		{
			storableObjectDataAccess.equipmentDataAccess.UpdateStatuses(statuses);
		}
	}
}
