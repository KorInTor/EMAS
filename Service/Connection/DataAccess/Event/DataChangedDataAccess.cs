using Model.Event;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Connection.DataAccess.Event
{
    public class DataChangedDataAccess
    {
        private readonly string schemaName = "\"event\".";
        private readonly string tableName = "data_changed";

        public string FullTableName => schemaName + tableName;

        public void Insert(NpgsqlConnection connection, DataChangedEvent dataChangedEvent)
        {
            using var command = new NpgsqlCommand("INSERT INTO " + FullTableName + " (event_id, comment) VALUES(@id, @reason); ", connection);
            command.Parameters.AddWithValue("@id", dataChangedEvent.Id);
            command.Parameters.AddWithValue("@reason", dataChangedEvent.Comment);
            command.ExecuteScalar();
            command.Dispose();
        }
    }
}
