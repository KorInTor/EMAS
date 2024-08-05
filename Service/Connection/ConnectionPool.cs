using Model.Exceptions;
using Npgsql;
using System.Diagnostics;

namespace Service.Connection
{
    public static class ConnectionPool
    {
        private static List<NpgsqlConnection> _connections = [];

        private static int _maxConnections = 5;

        private static string _connectionString = ConnectionString;

        private static short _openedConnections = 0;

        public static string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();
                    builder.Host = "localhost";
                    builder.Port = 5432;
                    builder.Database = "praktik";
                    builder.Username = "praktikant";
                    builder.Password = "hPS2lwTK0XaE";
                    _connectionString = builder.ConnectionString;
                }
                return _connectionString;
            }
        }

        public static NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            _openedConnections++;
            Debug.WriteLine($"Открыто соединений:{_openedConnections}");
            return connection;
            lock (_connections)
            {
                if (_connections.Count > 0)
                {
                    var conn = _connections[0];
                    _connections.RemoveAt(0);
                    return conn;
                }
                else if (_connections.Count < _maxConnections)
                {
                    var conn = new NpgsqlConnection(ConnectionString);
                    conn.Open();
                    return conn;
                }
                else
                {
                    while (_connections.Count == 0)
                    {
                        Thread.Sleep(100);
                    }
                    var conn = _connections[0];
                    _connections.RemoveAt(0);
                    return conn;
                }
            }
        }

        public static void ReleaseConnection(NpgsqlConnection conn)
        {
            conn.Close();
            conn.Dispose();
            _openedConnections--;
            Debug.WriteLine($"Открыто соединений:{_openedConnections}");
            return;
            lock (_connections)
            {
                _connections.Add(conn);
            }
        }

        public static void TryConnect()
        {
            using var conection = new NpgsqlConnection(ConnectionString);
            conection.Open();
            try
            {

                conection.Close();
            }
            catch (Exception ex)
            {
                throw new ConnectionFailedException(ex.Message);
            }
        }
    }
}
