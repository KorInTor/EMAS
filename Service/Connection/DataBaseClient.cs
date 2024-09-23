using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
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

		public void AddSingle(object objectToAdd)
		{
			Add([objectToAdd]);
		}

		public void Add(IEnumerable<object> objectToAdd)
		{
			if (objectToAdd is IEnumerable<Employee> newEmployee)
			{
				employeeDataAccess.Add(newEmployee);
				return;
			}

			if (objectToAdd is IEnumerable<Location> newLocation)
			{
				locationDataAccess.Add(newLocation.ToArray());
				return;
			}

			if (objectToAdd is IEnumerable<object> objects && objects.OfType<StorableObjectEvent>().Any())
			{
				var newEvents = objects.OfType<StorableObjectEvent>();
				eventDataAccess.Add(newEvents);
				return;
			}

			if (objectToAdd is IEnumerable<IStorableObject>)
			{
				throw new ArgumentException("Use AdditionEvent for adding new StorableObject to dataBase");
			}

			throw new NotSupportedException("Этот тип не поддерживается");
		}

		public void Update(IEnumerable<object> objectToUpdate)
		{
			if (objectToUpdate is IStorableObject newStorableObject)
			{
				storableObjectDataAccess.Update([newStorableObject]);
				return;
			}
			if (objectToUpdate is IEnumerable<Employee> newEmployees)
			{
				employeeDataAccess.Update(newEmployees);
				return;
			}
			if (objectToUpdate is IEnumerable<Location> newLocation)
			{
				locationDataAccess.Update(newLocation);
				return;
			}
			throw new NotSupportedException("Этот тип не поддерживается");
		}

		public void UpdateSingle(object objectToUpdate)
		{
			Update([objectToUpdate]);
		}

		public IEnumerable<T> Select<T>(QueryBuilder? queryBuilder = null)
		{
			queryBuilder = queryBuilder ?? new QueryBuilder();
			if (typeof(T) == typeof(Employee))
				return (IEnumerable<T>)employeeDataAccess.Select(queryBuilder);

			if (typeof(StorableObjectEvent).IsAssignableFrom(typeof(T)))
				throw new InvalidOperationException($"For Events use SelectEventMethod");

			throw new InvalidOperationException($"Select method not supported for type {typeof(T)}");
		}

		public IEnumerable<T> SelectByIds<T>(IEnumerable<int> ids, string idPropertyName)
		{
			QueryBuilder queryBuilder = new();
			queryBuilder.LazyInit<T>();

			queryBuilder.Where($"{typeof(T).Name}.{idPropertyName}", "=", ids.ToArray());

			return Select<T>(queryBuilder);
		}

		public T? SelectSingleById<T>(int id, string idPropertyName)
		{
			QueryBuilder queryBuilder = new();
			queryBuilder.LazyInit<T>();

			queryBuilder.Where($"{typeof(T).Name}.{idPropertyName}", "=", id);

			return Select<T>(queryBuilder).FirstOrDefault();
		}

		public IEnumerable<T> SelectEvent<T>(QueryBuilder? queryBuilder = null) where T : StorableObjectEvent
		{
			queryBuilder = queryBuilder ?? new QueryBuilder();
			return eventDataAccess.Select<T>(queryBuilder);
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
			queryBuilder.LazyInit<TEvent>().Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.Id)}","=",eventId);

			return eventDataAccess.Select<TEvent>(queryBuilder).ToList();
		}

		public List<SentEvent> SelectDeliveries(int locationId, bool selectIncoming, bool selectActive = true)
		{
			QueryBuilder queryBuilder = new ();
			queryBuilder.LazyInit<SentEvent>();

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
			queryBuilder.LazyInit<AdditionEvent>().Where($"{nameof(AdditionEvent)}.{nameof(AdditionEvent.LocationId)}", "=", locationId);
			events.AddRange(eventDataAccess.Select<AdditionEvent>(queryBuilder));

			queryBuilder = new QueryBuilder();
			queryBuilder
				.LazyInit<SentEvent>()
				.Where($"{nameof(SentEvent)}.{nameof(SentEvent.DepartureId)}", "=", locationId)
				.OrWhere($"{nameof(SentEvent)}.{nameof(SentEvent.DestinationId)}", "=", locationId);

			events.AddRange(eventDataAccess.Select<SentEvent>(queryBuilder));

			queryBuilder = new QueryBuilder();
			queryBuilder
				.LazyInit<ArrivedEvent>()
				.Where($"{nameof(SentEvent)}.{nameof(SentEvent.DepartureId)}", "=", locationId)
				.OrWhere($"{nameof(SentEvent)}.{nameof(SentEvent.DestinationId)}", "=", locationId);

			events.AddRange(eventDataAccess.Select<ArrivedEvent>(queryBuilder));

			queryBuilder = new QueryBuilder();
			queryBuilder
				.LazyInit<ReservedEvent>()
				.Where($"{nameof(ReservedEvent)}.{nameof(ReservedEvent.LocationId)}", "=", locationId);
			events.AddRange(eventDataAccess.Select<ReservedEvent>(queryBuilder));

			queryBuilder = new QueryBuilder();
			queryBuilder
				.LazyInit<ReserveEndedEvent>()
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
			queryBuilder.LazyInit<ReservedEvent>().Where($"{nameof(ReservedEvent)}.{nameof(ReservedEvent.LocationId)}","=",locationId);
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
			return storableObjectDataAccess.equipmentStatusDataAccess.SelectStatuses();
		}

		public void UpdateEquipmentStatuses(IEnumerable<(int,string)> statuses)
		{
			storableObjectDataAccess.equipmentStatusDataAccess.UpdateStatuses(statuses);
		}
	}
}
