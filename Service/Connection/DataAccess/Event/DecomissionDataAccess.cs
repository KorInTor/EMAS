using Model.Event;
using Npgsql;
namespace Service.Connection.DataAccess.Event
{
    public class DecomissionDataAccess
    {
        private readonly string schemaName = "\"event\".";
        private readonly string tableName = "decomission";

        public string FullTableName => schemaName + tableName;

        public void Insert(NpgsqlConnection connection, DecomissionedEvent decomissionedEvent)
        {
            using var command = new NpgsqlCommand("INSERT INTO "+FullTableName+" (event_id, reason, location_id) VALUES(@id, @reason, @locationid); ", connection);
            command.Parameters.AddWithValue("@id", decomissionedEvent.Id);
            command.Parameters.AddWithValue("@reason", decomissionedEvent.Comment);
            command.Parameters.AddWithValue("@locationid", decomissionedEvent.LocationId);
            command.ExecuteScalar();
            command.Dispose();
        }
    }
}
