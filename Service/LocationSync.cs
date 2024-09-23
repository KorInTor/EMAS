using Model.Event;
using Model;
using Service.Connection.DataAccess.Query;
using Service.Connection.DataAccess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Service.Connection;

namespace Service
{
	public static class LocationSync
	{
		public static event Action<List<StorableObjectEvent>> NewEventsOccured;

		private static long lastEventId;

		public static long LastEventId
		{
			get
			{
				return lastEventId;
			}
			private set
			{
				lastEventId = value;
			}
		}

		public static void SyncData(List<Location> locationsToSync)
		{
			if (IsDataUpToDate(out long lastDataBaseEventId))
				return;

			QueryBuilder queryBuilder = new();
			queryBuilder.LazyInit<StorableObjectEvent>().Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.Id)}", ">", LastEventId);
			List<StorableObjectEvent> newStorableObjectEvents = DataBaseClient.GetInstance().SelectEventsCustom<StorableObjectEvent>(queryBuilder).ToList();

			LastEventId = lastDataBaseEventId;

			var locationIdDictionary = new Dictionary<int, Location>();
			foreach (var location in locationsToSync)
			{
				locationIdDictionary.Add(location.Id, location);
			}

			var eventHandlers = new Dictionary<EventType, Action<Dictionary<int, Location>, StorableObjectEvent>>
			{
				{ EventType.Addition, HandleAdditionEvent},
				{ EventType.Arrived, HandleArrivalEvent},
				{ EventType.Sent, HandleSentEvent},
				{ EventType.DataChanged, HandleDataChangedEvent},
				{ EventType.Decommissioned, HandleDecomissionedEvent},
				{ EventType.Reserved, HandleReservedEvent},
				{ EventType.ReserveEnded, HandleReserveEndedEvent},
			};

			foreach (var newStorableObjectEvent in newStorableObjectEvents)
			{
				eventHandlers[newStorableObjectEvent.EventType].Invoke(locationIdDictionary, newStorableObjectEvent);
			}

			NewEventsOccured?.Invoke(newStorableObjectEvents);
		}

		private static void HandleReserveEndedEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
		{
			ReserveEndedEvent completedReservation = (ReserveEndedEvent)newStorableObjectEvent;

			ReservedEvent? reservedEvent = null;

			foreach (var location in locationIdDictionary.Values)
			{
				reservedEvent = location.Reservations.Where(reservation => reservation.Id == completedReservation.ReserveEventId).FirstOrDefault();

				location.Reservations.RemoveAll(reservation => reservation.Id == completedReservation.ReserveEventId);

				if (reservedEvent != null)
					break;
			}

			locationIdDictionary[reservedEvent.LocationId].StorableObjectsList.AddRange(newStorableObjectEvent.ObjectsInEvent);
		}

		private static void HandleReservedEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
		{
			ReservedEvent Reservation = (ReservedEvent)newStorableObjectEvent;

			locationIdDictionary[Reservation.LocationId].Reservations.Add(Reservation);

			foreach (var storableObjectInEvent in newStorableObjectEvent.ObjectsInEvent)
			{
				locationIdDictionary[Reservation.LocationId].StorableObjectsList.RemoveAll(StorableObjectOnLocation => StorableObjectOnLocation.Id == storableObjectInEvent.Id);
			}
		}

		private static void HandleDecomissionedEvent(Dictionary<int, Location> dictionary, StorableObjectEvent @event)
		{
			throw new NotImplementedException();
		}

		private static void HandleDataChangedEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
		{
			int locationChangedId = 0;
			foreach (var location in locationIdDictionary.Values)
			{
				foreach (var storableObjectInEvent in newStorableObjectEvent.ObjectsInEvent)
				{
					foreach (var storableObjectInLocation in location.StorableObjectsList)
					{
						if (storableObjectInEvent.Id == storableObjectInLocation.Id)
						{
							locationChangedId = location.Id;
							break;
						}
					}
					if (locationChangedId != 0)
						break;
				}
				if (locationChangedId != 0)
					break;
			}


			foreach (var storableObject in newStorableObjectEvent.ObjectsInEvent)
			{
				if (storableObject is Equipment equipmentInEvent)
				{
					locationIdDictionary[locationChangedId].StorableObjectsList.RemoveAll(equipmentOnLocation => equipmentOnLocation.Id == equipmentInEvent.Id);
					locationIdDictionary[locationChangedId].StorableObjectsList.Add(equipmentInEvent);
				}
			}
		}

		private static void HandleAdditionEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
		{
			AdditionEvent additionEvent = (AdditionEvent)newStorableObjectEvent;
			foreach (var storableObject in additionEvent.ObjectsInEvent)
			{
				locationIdDictionary[additionEvent.LocationId].StorableObjectsList.AddRange(additionEvent.ObjectsInEvent);
			}
		}

		private static void HandleSentEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
		{
			SentEvent newDelivery = (SentEvent)newStorableObjectEvent;

			locationIdDictionary[newDelivery.DepartureId].OutgoingDeliveries.Add(newDelivery);
			locationIdDictionary[newDelivery.DestinationId].IncomingDeliveries.Add(newDelivery);

			foreach (var storableObject in newStorableObjectEvent.ObjectsInEvent)
			{
				locationIdDictionary[newDelivery.DepartureId].StorableObjectsList.RemoveAll(objectOnLocation => objectOnLocation.Id == storableObject.Id);
			}
		}

		private static void HandleArrivalEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
		{
			ArrivedEvent arrivedDelivery = (ArrivedEvent)newStorableObjectEvent;

			SentEvent completedDelivery = null;

			foreach (var location in locationIdDictionary.Values)
			{
				completedDelivery = location.OutgoingDeliveries.Where(delivery => delivery.Id == arrivedDelivery.SentEventId).FirstOrDefault();
				if (completedDelivery != null)
					break;
			}


			locationIdDictionary[completedDelivery.DepartureId].OutgoingDeliveries.RemoveAll(delivery => delivery.Id == completedDelivery.Id);
			locationIdDictionary[completedDelivery.DestinationId].IncomingDeliveries.RemoveAll(delivery => delivery.Id == completedDelivery.Id);

			locationIdDictionary[completedDelivery.DestinationId].StorableObjectsList.AddRange(newStorableObjectEvent.ObjectsInEvent);
		}

		private static bool IsDataUpToDate(out long lastDataBaseEventId)
		{
			throw new NotImplementedException();
			StorableObjectEvent? lastEvent = null;

			if (lastEvent is null)
			{
				lastDataBaseEventId = 0;
			}
			else
			{
				lastDataBaseEventId = lastEvent.Id;
			}

			if (lastDataBaseEventId == LastEventId)
			{
				Debug.WriteLine("Data is UpToDate");
				return true;
			}
			Debug.WriteLine("Data is OutDated");
			return false;
		}

		public static bool IsDataUpToDate()
		{
			return IsDataUpToDate(out _);
		}

	}
}
