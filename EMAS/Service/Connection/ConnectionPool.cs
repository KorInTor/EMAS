﻿using EMAS.Exceptions;
using Npgsql;
using System;
using System.Collections.Generic;
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
            lock (_connections)
            {
                _connections.Add(conn);
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
