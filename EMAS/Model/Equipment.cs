using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model
{
    /// <summary>
    /// Stores info about equipment.
    /// </summary>
    public class Equipment
    {
        /// <summary>
        /// Unique id of Equipment.
        /// </summary>
        private int _id;

        /// <summary>
        /// Type of equipment.
        /// </summary>
        private string _type;

        /// <summary>
        /// Name of equipment.
        /// </summary>
        private string _name;

        /// <summary>
        /// Mesurment units. {ex: °C}.
        /// </summary>
        private string? _units;

        /// <summary>
        /// Accuracy class.
        /// </summary>
        private string? _accuracyClass;

        /// <summary>
        /// Mesurment limits. {ex: 0..10}.
        /// </summary>
        private string? _limit;

        /// <summary>
        /// Name of manufacturer.
        /// </summary>
        private string _manufacturer;

        /// <summary>
        /// Factory number given by manufacturer.
        /// </summary>
        private string _factoryNumber;


        /// <summary>
        /// Registration number inside "ОЙЛТИМ" documentation, can change during life-time.
        /// </summary>
        private string _registrationNumber;

        /// <summary>
        /// Current status of equipment.
        /// </summary>
        private string _status;

        /// <summary>
        /// Current description of equipment.
        /// </summary>
        private string _description;


        /// <summary>
        /// List of event for this equipment.
        /// </summary>
        private List<HistoryEntryBase> _equipmentHistory;

        /// <summary>
        /// List of tags.
        /// </summary>
        private List<string> _tags;

        public Equipment(string status, string registrationNumber, string description)
        {
            Status = status;
            RegistrationNumber = registrationNumber;
            Description = description;
            History = [];
            Tags = [];
        }

        /// <summary>
        /// Returns and sets list of tags.
        /// </summary>
        public List<string> Tags
        {
            get
            {
                return _tags;
            }
            set
            {
                _tags = value;
            }
        }

        /// <summary>
        /// Returns and sets Current status of equipment.
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }

        /// <summary>
        /// Returns and sets Registration number inside "ОЙЛТИМ" documentation.
        /// </summary>
        public string RegistrationNumber
        {
            get
            {
                return _registrationNumber;
            }
            set
            {
                _registrationNumber = value;
            }
        }

        /// <summary>
        /// Returns and sets Current description of equipment.
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }

        /// <summary>
        /// Returns and sets list of event for this equipment.
        /// </summary>
        public List<HistoryEntryBase> History
        {
            get
            {
                return _equipmentHistory;
            }
            set
            {
                _equipmentHistory = value;
            }
        }

        /// <summary>
        /// Returns and sets accuracy class.
        /// </summary>
        public string? AccuracyClass
        {
            get
            {
                return _accuracyClass;
            }
            set
            {
                _accuracyClass = value;
            }
        }

        /// <summary>
        /// Returns Mesurment units. {ex: °C}.
        /// </summary>
        public string Units
        {
            get
            {
                return _units ?? string.Empty;
            }
            private set
            {
                _units = value;
            }
        }

        /// <summary>
        /// Returns Mesurment limits. {ex: 0..10}.
        /// </summary>
        public string Limit
        {
            get
            {
                return _limit ?? string.Empty;
            }
            set
            {
                _limit = value;
            }
        }

        /// <summary>
        /// Returns Unique id of Equipment.
        /// </summary>
        public int Id
        {
            get
            {
                return _id;
            }
            private set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Returns Name of equipment.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            private set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Returns Name of manufacturer.
        /// </summary>
        public string Manufacturer
        {
            get
            {
                return _manufacturer;
            }
            private set
            {
                _manufacturer = value;
            }
        }

        /// <summary>
        /// Returns Type of equipment.
        /// </summary>
        public string Type
        {
            get
            {
                return _type;
            }
            private set
            {
                _type = value;
            }
        }

        /// <summary>
        /// Returns Factory number given by manufacturer.
        /// </summary>
        public string FactoryNumber
        {
            get
            {
                return _factoryNumber;
            }
            private set
            {
                _factoryNumber = value;
            }
        }
    }
}
