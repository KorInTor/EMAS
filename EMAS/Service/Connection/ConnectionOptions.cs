using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection
{
    public static class ConnectionOptions
    {
        private static string _host = "26.34.196.234";

        private static int _port = 5432;

        private static string _dataBase = "praktik";

        private static string _DBMSlogin = "praktikant";

        private static string _DBMSpassword = "hPS2lwTK0XaE";

        public static string ConnectionString
        {
            get
            {
                return $"Host={_host};Port={_port};Username={_DBMSlogin};Password={_DBMSpassword};Database={_dataBase}";
            }
        }
    }
}
