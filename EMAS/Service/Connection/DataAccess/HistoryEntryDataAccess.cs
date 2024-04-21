using EMAS.Exceptions;
using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Model.HistoryEntry;
using EMAS.ViewModel;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class HistoryEntryDataAccess
    {
        public List<HistoryEntryBase> SelectByEquipmentId(int equipmentId)
        {
            var list = new List<HistoryEntryBase>();
            var eventConstructors = new Dictionary<EventType, Func<Employee, DateOnly, HistoryEntryBase>>
            {
                { EventType.Addition, (responcible, date) => new AdditionHistoryEntry(responcible, date) },
                { EventType.Sent, (responcible, date) => new SentHistoryEntry(responcible, date) },
                { EventType.Arrived, (responcible, date) => new ReceivedHistoryEntry(responcible, date) },
                { EventType.Decommissioned, (responcible, date) => new DecommissionedHistoryEntry(responcible, date) },
                { EventType.Reserved, (responcible, date) => new ReservedHistoryEntry(responcible, date) },
                { EventType.ReserveEnded, (responcible, date) => new ReservationEndedHistoryEntry(responcible, date) }
            };

            var connection = ConnectionPool.GetConnection();

            string query = "SELECT event.date, event.employee_id, event.event_type " +
              "FROM public.\"event\" AS event " +
              "JOIN public.equipment_event AS equipmentEvent ON event.id = equipmentEvent.event_id " +
              "WHERE equipmentEvent.equipment_id = @id";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", equipmentId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var employeeAccess = new EmployeeDataAccess();
                Employee responcible = employeeAccess.SelectById(reader.GetInt32(1));
                DateOnly date = DateOnly.FromDateTime(reader.GetDateTime(0));
                short eventType = reader.GetInt16(2);

                if (eventConstructors.TryGetValue((EventType)eventType, out var constructor))
                {
                    list.Add(constructor(responcible, date));
                }
                else
                {
                    throw new UnknownEventTypeException($"Полученый тип события (id= '{eventType}') оказался неизвестным");
                }
            }
            ConnectionPool.ReleaseConnection(connection);
            foreach (var entry in list)
            {
                Debug.WriteLine("Полученны данные: " + entry.ToString());
            }

            return list;
        }
    }
}
