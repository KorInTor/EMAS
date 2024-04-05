using EMAS.Exceptions;
using EMAS.Model;
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
            var eventConstructors = new Dictionary<string, Func<Employee, DateOnly, HistoryEntryBase>>
            {
                { "Addition", (responcible, date) => new AdditionHistoryEntry(responcible, date) },
                { "Sent", (responcible, date) => new SentHistoryEntry(responcible, date) },
                { "Arrived", (responcible, date) => new ReceivedHistoryEntry(responcible, date) },
                { "Decommissioned", (responcible, date) => new DecommissionedHistoryEntry(responcible, date) },
                { "Reserved", (responcible, date) => new ReservedHistoryEntry(responcible, date) },
                { "ReserveEnded", (responcible, date) => new ReservationEndedHistoryEntry(responcible, date) }
            };

            var connection = ConnectionPool.GetConnection();

            string query = "SELECT event.date, event.employee_id, event_type.\"name\" " +
                    "FROM public.\"event\" AS event " +
                    "JOIN public.equipment_event AS equipmenEvent " +
                    "JOIN public.event_type AS event_type ON event_type.id = event.event_type " +
                    "AND event.id = equipmenEvent.event_id AND equipmenEvent.equipment_id = @id";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", equipmentId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var employeeAccess = new EmployeeDataAccess();
                Employee responcible = employeeAccess.SelectById(reader.GetInt32(1));
                DateOnly date = DateOnly.FromDateTime(reader.GetDateTime(0));
                string eventType = reader.GetString(2);

                if (eventConstructors.TryGetValue(eventType, out var constructor))
                {
                    list.Add(constructor(responcible, date));
                }
                else
                {
                    throw new UnknownEventTypeException($"Полученый тип события = '{eventType}' оказался неизвестным");
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
