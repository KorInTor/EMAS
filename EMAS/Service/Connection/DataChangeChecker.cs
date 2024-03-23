using DocumentFormat.OpenXml.Office2010.Excel;
using EMAS.Model;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection
{
    /// <summary>
    /// Provides methods for cheking updates in DataBase.
    /// </summary>
    public class DataChangeChecker : IDisposable
    {
        /// <summary>
        /// Stores last modification dateTime for each location. Stored in local time.
        /// </summary>
        private Dictionary<int,DateTime> lastModificationTimes = [];


        public delegate void HandleDataChange(int ID);
        /// <summary>
        /// Invokes when equipment data in location has updated.
        /// </summary>
        public static event HandleDataChange? DataChanged;

        /// <summary>
        /// CancelationToken for stoping listeners.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource = new();

        /// <summary>
        /// Initialize Checker with all location Ids.
        /// </summary>
        /// <param name="locationIdsList">All location Ids.</param>
        public DataChangeChecker(List<int> locationIdsList) 
        {
            foreach (int id in locationIdsList)
            {
                lastModificationTimes.Add(id,DateTime.MinValue);
            }
        }

        /// <summary>
        /// Initializes listeners for every location.
        /// </summary>
        /// <remarks>
        /// Better use <see cref="InitListener(int)"/>.
        /// </remarks>
        public void InitAllListeners()
        {
            StopActiveListeners();
            _cancellationTokenSource = new();
            foreach (int id in lastModificationTimes.Keys)
            {
                Task.Run(() => ListenForChangeOnLocation(id, _cancellationTokenSource.Token));
                Task.Delay(1000);
            }
        }

        /// <summary>
        /// Initializes listener for location.
        /// </summary>
        /// <param name="locationId">Location Id.</param>
        public void InitListener(int locationId)
        {
            StopActiveListeners();
            _cancellationTokenSource = new();
            Task.Run(() => ListenForChangeOnLocation(locationId, _cancellationTokenSource.Token));
        }

        /// <summary>
        /// Stops all active listeners.
        /// </summary>
        public void StopActiveListeners()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Will listen for change in dataBase every 10 second while <see cref="StopActiveListeners"/> or <see cref="IDisposable.Dispose"/> are not called.
        /// </summary>
        /// <param name="locationId">For which location listener will get last modification Date.</param>
        /// <param name="token">Cancellation token that will stop Listener.</param>
        /// <returns></returns>
        private async Task ListenForChangeOnLocation(int locationId, CancellationToken token)
        {
            using var connection = new NpgsqlConnection(ConnectionOptions.ConnectionString);
            connection.Open();
            string sql = "SELECT hash FROM public.location_data_change WHERE location_id=@id;";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", locationId);
            do
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                
                DateTimeOffset dateTimeOffset = (DateTimeOffset)command.ExecuteScalar();

                DateTime localChangeTime = dateTimeOffset.DateTime.ToLocalTime();

                if (localChangeTime != lastModificationTimes[locationId])
                {
                    lastModificationTimes[locationId] = localChangeTime;
                    DataChanged?.Invoke(locationId);
                }
                await Task.Delay(10000, token);
            } while (true);
        }

        /// <summary>
        /// Releasing all used resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            StopActiveListeners();
            _cancellationTokenSource.Dispose();
        }
    }
}
