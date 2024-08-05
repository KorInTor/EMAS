using Model.Event;
using Npgsql;

namespace Service.Connection.DataAccess.Event
{
    public class AdditionDataAccess
    {
        public void InsertAdditionEvent(NpgsqlConnection connection, AdditionEvent additionEvent)
        {
            using var command = new NpgsqlCommand("INSERT INTO \"event\".addition (event_id, location_id) VALUES(@id, @locatioId); ", connection);
            command.Parameters.AddWithValue("@id", additionEvent.Id);
            command.Parameters.AddWithValue("@locatioId", additionEvent.LocationId);
            command.ExecuteScalar();
            command.Dispose();
        }

        public IEnumerable<AdditionEvent> SelectAdditionEvents(IEnumerable<StorableObjectEvent> storableObjectEvents, NpgsqlConnection connection)
        {
            List<AdditionEvent> additionEventsList = [];
            string query = "SELECT location_id FROM \"event\".addition WHERE event_id=@id";
            var command = new NpgsqlCommand(query, connection);
            foreach (StorableObjectEvent storableObjectEvent in storableObjectEvents)
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@id", storableObjectEvent);
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    additionEventsList.Add(new(storableObjectEvent, reader.GetInt32(0)));
                }
                reader.Close();
            }
            command.Dispose();
            return additionEventsList;
        }

        public AdditionEvent? SelectAdditionEvent(StorableObjectEvent storableObjectEvent, NpgsqlConnection connection)
        {
            return SelectAdditionEvents([storableObjectEvent], connection).FirstOrDefault();
        }
    }
}
