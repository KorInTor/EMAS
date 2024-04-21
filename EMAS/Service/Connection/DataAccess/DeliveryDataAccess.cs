﻿using DocumentFormat.OpenXml.Spreadsheet;
using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Service.Connection.DataAccess.Interface;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class DeliveryDataAccess : IObjectStateLocationBoundedDataAccess<Delivery>
    {
        private readonly EventDataAccess eventAccess = new();
        private readonly StorableObjectInEventDataAccess storableObjectInEventDataAccess = new();
        private readonly string schemaName = "\"event\"";
        private readonly string tableName = "delivery";
        private string FullTableName => schemaName + tableName;
        public void Add(Delivery newDelivery)
        {
            Add([newDelivery]);
        }

        public void Delete(Delivery objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<Delivery> Select()
        {
            throw new NotImplementedException();
        }

        public Delivery SelectById(int id)
        {
            return SelectById(id);
        }

        public void Update(Delivery objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public List<Delivery> SelectOnLocation(int locationId)
        {
            var deliveries = new List<Delivery>();

            string query = "SELECT E.date, D.destination_id, D.dispatch_event_id " +
                                "FROM \"event\".delivery AS D " +
                                "JOIN public.\"event\" AS E ON E.id = D.dispatch_event_id " +
                                "WHERE D.arrival_event_id IS NULL AND D.departure_id = @locationId ";

            var connection = ConnectionPool.GetConnection();

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@locationId", locationId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                deliveries.Add(new Delivery(reader.GetInt64(2), locationId, reader.GetInt32(1), reader.GetDateTime(0), storableObjectInEventDataAccess.SelectObjectsInEvent(reader.GetInt64(2))));
                Debug.WriteLine($"Получили delivery идущее ИЗ Объекта с id - {locationId}");
            }

            ConnectionPool.ReleaseConnection(connection);

            return deliveries;
        }

        public void Complete(Delivery completedDelivery)
        {
            Complete([completedDelivery]);
        }

        public void AddOnLocation(Delivery item, int locationId)
        {
            Add(item);
        }

        public void Add(Delivery[] objectToAdd)
        {
            string query = "INSERT INTO " + FullTableName + " (dispatch_event_id, destination_id, departure_id) VALUES (@dispatch_event_id, @destination_id, @departure_id);";
            var connection = ConnectionPool.GetConnection();

            foreach (var delivery in objectToAdd)
            {
                StorableObjectEvent deliveryEvent = new(SessionManager.UserId, 0, EventType.Sent, delivery.DispatchDate, delivery.PackageList);

                eventAccess.Add(deliveryEvent);

                delivery.Id = deliveryEvent.Id;
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@dispatch_event_id", delivery.Id);
                command.Parameters.AddWithValue("@destination_id", delivery.DestinationId);
                command.Parameters.AddWithValue("@departure_id", delivery.DepartureId);

                command.ExecuteNonQuery();
            }
            
            Debug.WriteLine("Успешно добавлена доставка");

            ConnectionPool.ReleaseConnection(connection);
        }

        public void Delete(Delivery[] objectToDelete)
        {
            throw new NotImplementedException();
        }

        public void Update(Delivery[] objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public void AddOnLocation(Delivery[] item, int locationId)
        {
            throw new NotImplementedException();
        }

        public void Complete(Delivery[] objectToComplete)
        {
            foreach (var delivery in objectToComplete)
            {
                StorableObjectEvent newEvent = new(SessionManager.UserId, 0, EventType.Arrived, delivery.DispatchDate ,delivery.PackageList);

                eventAccess.Add(newEvent);

                var equipmentAccess = new EquipmentDataAccess();
                foreach(IStorableObject storableObject in delivery.PackageList)
                {
                    if (storableObject is Equipment equipmentInDelivery)
                    {
                        equipmentAccess.UpdateLocation((equipmentInDelivery, delivery.DestinationId));
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }

                var connection = ConnectionPool.GetConnection();
                using var command = new NpgsqlCommand("UPDATE" + FullTableName + "SET arrival_event_id=@arrival_event_id, WHERE dispatch_event_id=@sendedEventId ", connection);
                command.Parameters.AddWithValue("@arrival_event_id", newEvent.Id);
                command.Parameters.AddWithValue("@sendedEventId", delivery.Id);

                command.ExecuteNonQuery();

                ConnectionPool.ReleaseConnection(connection);
            }
        }

        public Delivery SelectById(long Id)
        {
            throw new NotImplementedException();
        }
    }
}
