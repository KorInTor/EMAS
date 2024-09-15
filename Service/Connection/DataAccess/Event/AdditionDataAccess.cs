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
    }
}
