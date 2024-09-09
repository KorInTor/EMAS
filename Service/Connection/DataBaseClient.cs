using Model;
using Model.Event;
using Service.Connection.DataAccess;
using Service.Connection.DataAccess.Interface;
using Service.Connection.DataAccess.QueryBuilder;
using System.Net.Http.Headers;

namespace Service.Connection
{
	public class DataBaseClient
	{
		private StorableObjectDataAccess storableObjectDataAccess;
		private ISimpleDataAccess<Employee> employeeDataAccess;
		private ISimpleDataAccess<Location> locationDataAccess;

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
			occupiedObject = [];

			foreach (var objectLastEventPair in eventDataAccess.SelectLastEventsForStorableObjects(storableObjects))
			{
				if (objectLastEventPair.Value.EventType == EventType.Sent || objectLastEventPair.Value.EventType == EventType.Reserved || objectLastEventPair.Value.EventType == EventType.Decommissioned)
				{
					occupiedObject.Add(objectLastEventPair.Key);
				}
			}

			if (occupiedObject.Count == 0)
				return true;
			else
				return false;
		}

		public StorableObjectEvent SelectEventById(long eventId, Type typeOfEvent = null)
		{
			var condition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<StorableObjectEvent>(x => x.Id), Comparison.Equal, eventId);

			return eventDataAccess.Select([condition], typeOfEvent).First();
		}

		public List<SentEvent> GetDeliverysOutOf(int locationId)
		{
			return eventDataAccess.SelectActiveDeliveriesOutOfLocation(locationId);
		}

		public List<IStorableObject> SelectStorableObjectOn(int locationId)
		{
			return storableObjectDataAccess.SelectOnLocation(locationId).ToList();
		}

		public List<IStorableObject> SelectStorableObjectsByIds(IEnumerable<int> ids)
		{
			return storableObjectDataAccess.SelectByIds(ids).ToList();
		}

		public List<ReservedEvent> GetReservationOn(int locationId)
		{
			var condition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<ReservedEvent>(x => x.LocationId), Comparison.Equal, locationId);
			return (List<ReservedEvent>)Task.Run(() => eventDataAccess.SelectAsync([condition], typeof(ReservedEvent))).Result;
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
