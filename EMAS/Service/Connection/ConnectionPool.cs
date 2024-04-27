using EMAS.Exceptions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection
{
    public static class ConnectionPool
    {
        private static List<NpgsqlConnection> _connections = new List<NpgsqlConnection>();

        private static int _maxConnections = 5;

        private static string _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();
                    builder.Host = "26.34.196.234";
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
            return connection;
            lock (_connections)
            {
                if (_connections.Count > 0)
                {
                    var conn = _connections[0];
                    _connections.RemoveAt(0);
                    Debug.WriteLine($"Отсалось соединений:{_connections.Count}");
                    return conn;
                }
                else if (_connections.Count < _maxConnections)
                {
                    var conn = new NpgsqlConnection(ConnectionString);
                    conn.Open();
                    Debug.WriteLine($"Отсалось соединений:{_connections.Count}");
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
                    Debug.WriteLine($"Отсалось соединений:{_connections.Count}");
                    return conn;
                }
            }
        }

        public static void ReleaseConnection(NpgsqlConnection conn)
        {
            conn.Close();
            return;
            lock (_connections)
            {
                _connections.Add(conn);
                Debug.WriteLine($"Отсалось соединений:{_connections.Count}");
            }
        }

        public static void TryConnect()
        {
            using var conection = new NpgsqlConnection(ConnectionString);
            try
            {
                conection.Open();
                conection.Close();
            }
            catch (Exception ex)
            {
                throw new ConnectionFailedException(ex.Message);
            }
        }
    }
}
