using DocumentFormat.OpenXml.EMMA;
using Model;
using Model.Event;
using Service.Connection.DataAccess;
using Service.Connection.DataAccess.QueryBuilder;
using System.Collections.Generic;
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

		public bool IsStorableObjectsNotOccupied(IStorableObject[] storableObjects, out List<IStorableObject> occupiedObject)
		{
			return eventDataAccess.IsStorableObjectsNotOccupied(storableObjects, out occupiedObject);
		}

		public Dictionary<IStorableObject, StorableObjectEvent> SelectLastEventsForStorableObjects(IEnumerable<IStorableObject> storableObjects)
		{
			return eventDataAccess.SelectLastEventsForStorableObjects(storableObjects);
		}

		public StorableObjectEvent SelectEventById(long eventId, Type typeOfEvent = null)
		{
			var condition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<StorableObjectEvent>(x => x.Id), Comparison.Equal, eventId);

			return eventDataAccess.Select([condition], typeOfEvent).First();
		}

		public List<SentEvent> GetDeliverysOutOf(int locationId)
		{
            var notArrivedCondition = new NullCondition(SelectQueryBuilder.GetFullPropertyName<ArrivedEvent>(x => x.Id), true);
            var departureCondition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<SentEvent>(x => x.DepartureId), Comparison.Equal, locationId);

            IEnumerable<SentEvent> activeDeliveries = eventDataAccess.Select([notArrivedCondition, departureCondition], typeof(SentEvent)).OfType<SentEvent>();

            return activeDeliveries.ToList();
		}

        public List<SentEvent> GetDeliverysInTo(int locationId, bool selectActive = true)
        {
            var destinationCondition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<SentEvent>(x => x.DestinationId), Comparison.Equal, locationId);

			List<BaseCondition> conditions = [destinationCondition];
			if (selectActive == true)
			{
                var notArrivedCondition = new NullCondition(SelectQueryBuilder.GetFullPropertyName<ArrivedEvent>(x => x.Id), true);
				conditions.Add(notArrivedCondition);
            }

            IEnumerable<SentEvent> activeDeliveries = eventDataAccess.Select(conditions, typeof(SentEvent)).OfType<SentEvent>();

            return activeDeliveries.ToList();
        }

		public List<StorableObjectEvent> SelectLocationBoundedEvents(int locationId, DateTime? floorDateValue = null, DateTime? ceilingDateValue = null)
		{
			List<StorableObjectEvent> events = [];

            if (locationId == 0)
			{
				throw new ArgumentException("Missing Location Id");
			}

			List<BaseCondition> defaultConditions = [];

			if (floorDateValue is not null)
			{
				var floorCondition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<StorableObjectEvent>(x => x.DateTime), Comparison.GreaterThanOrEqual, floorDateValue);
				defaultConditions.Add(floorCondition);
            }

            if (ceilingDateValue is not null)
            {
                var ceilingCondition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<StorableObjectEvent>(x => x.DateTime), Comparison.GreaterThanOrEqual, ceilingDateValue);
                defaultConditions.Add(ceilingCondition);
            }

			//TODO Разбить на методы
            //-* Addition Event *-//

            List<BaseCondition> additionConditions = [];

			additionConditions.AddRange(defaultConditions);
			additionConditions.Add(new CompareCondition(SelectQueryBuilder.GetFullPropertyName<AdditionEvent>(x => x.LocationId), Comparison.Equal, locationId));

            events.AddRange(eventDataAccess.Select(additionConditions, typeof(AdditionEvent)));

            //-* Delivery Event *-//

            List<BaseCondition> deliveryConditions = [];
			deliveryConditions.AddRange(defaultConditions);
			deliveryConditions.Add(new CompareCondition(SelectQueryBuilder.GetFullPropertyName<SentEvent>(x => x.DestinationId), Comparison.Equal, locationId));

            events.AddRange(eventDataAccess.Select(deliveryConditions, typeof(SentEvent)));
            events.AddRange(eventDataAccess.Select(deliveryConditions, typeof(ArrivedEvent)));

			deliveryConditions.RemoveAt(deliveryConditions.Count-1);
            deliveryConditions.Add(new CompareCondition(SelectQueryBuilder.GetFullPropertyName<SentEvent>(x => x.DepartureId), Comparison.Equal, locationId));

            events.AddRange(eventDataAccess.Select(deliveryConditions, typeof(SentEvent)));
            events.AddRange(eventDataAccess.Select(deliveryConditions, typeof(ArrivedEvent)));

			//-* Reservation Event *-//

			List<BaseCondition> reservationCondition = [];
			reservationCondition.AddRange(defaultConditions);
			reservationCondition.Add(new CompareCondition(SelectQueryBuilder.GetFullPropertyName<ReservedEvent>(x => x.LocationId), Comparison.Equal, locationId));

			events.AddRange(eventDataAccess.Select(reservationCondition, typeof(ReservedEvent)));
			events.AddRange(eventDataAccess.Select(reservationCondition, typeof(ReserveEndedEvent)));

			//-* Decomission Event *-//

			List<BaseCondition> decomissionCondition = [];

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
			List<BaseCondition> conditionsList = [];
			var locationCondition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<ReservedEvent>(x => x.LocationId), Comparison.Equal, locationId);
			conditionsList.Add(locationCondition);
			if (selectOnlyActive)
			{
				var noEndReserveCondition = new NullCondition(SelectQueryBuilder.GetFullPropertyName<ReserveEndedEvent>(x => x.Id), true);

				conditionsList.Add(noEndReserveCondition);
			}


			return eventDataAccess.Select(conditionsList, typeof(ReservedEvent)).OfType<ReservedEvent>().ToList();
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

		public List<StorableObjectEvent> SelectForStorableObjectId(int storableObjectId)
		{
			return eventDataAccess.SelectEventsForStorableObject(storableObjectId).ToList();
		}

		public IEnumerable<StorableObjectEvent> SelectEvent(IEnumerable<BaseCondition> conditions)
		{
			return eventDataAccess.Select(conditions);

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
