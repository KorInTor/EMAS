using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model
{
    /// <summary>
    /// Stores info about equipment.
    /// </summary>
    public class Equipment : ObservableObject, IEquatable<Equipment> , ILocationBounded, IStorableObject
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
        /// List of tags.
        /// </summary>
        private List<string> _tags;

        public Equipment(){ }

        public Equipment(string status, string registrationNumber, string description)
        {
            Status = status;
            RegistrationNumber = registrationNumber;
            Description = description;
            Tags = [];
        }

        public Equipment(string status, string registrationNumber, string description, string? accuracyClass, string units, string limit, int id, string name, string manufacturer, string type, string factoryNumber, List<string> tags)
        {
            Status = status;
            RegistrationNumber = registrationNumber;
            Description = description;
            AccuracyClass = accuracyClass;
            Units = units;
            Limit = limit;
            Id = id;
            Name = name;
            Manufacturer = manufacturer;
            Type = type;
            FactoryNumber = factoryNumber;
            Tags = tags;
        }

        /// <summary>
        /// Returns and sets list of tags.
        /// </summary>
        public List<string> Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        /// <summary>
        /// Returns and sets Current status of equipment.
        /// </summary>
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// Returns and sets Registration number inside "ОЙЛТИМ" documentation.
        /// </summary>
        public string RegistrationNumber
        {
            get => _registrationNumber;
            set => SetProperty(ref _registrationNumber, value);
        }

        /// <summary>
        /// Returns and sets Current description of equipment.
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// Returns and sets accuracy class.
        /// </summary>
        public string? AccuracyClass
        {
            get => _accuracyClass;
            set => SetProperty(ref _accuracyClass, value);
        }

        /// <summary>
        /// Returns Mesurment units. {ex: °C}.
        /// </summary>
        public string Units
        {
            get => _units;
            set => SetProperty(ref _units, value);
        }

        /// <summary>
        /// Returns Mesurment limits. {ex: 0..10}.
        /// </summary>
        public string Limit
        {
            get => _limit;
            set => SetProperty(ref _limit, value);
        }

        /// <summary>
        /// Returns Unique id of Equipment.
        /// </summary>
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// Returns Name of equipment.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Returns Name of manufacturer.
        /// </summary>
        public string Manufacturer
        {
            get => _manufacturer;
            set => SetProperty(ref _manufacturer, value);
        }

        /// <summary>
        /// Returns Type of equipment.
        /// </summary>
        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        /// <summary>
        /// Returns Factory number given by manufacturer.
        /// </summary>
        public string FactoryNumber
        {
            get => _factoryNumber;
            set => SetProperty(ref _factoryNumber, value);
        }

        public bool Equals(Equipment? other)
        {
            if (other == null || GetType() != other.GetType())
            {
                return false;
            }

            return Id == other.Id &&
                   Type == other.Type &&
                   Name == other.Name &&
                   Units == other.Units &&
                   AccuracyClass == other.AccuracyClass &&
                   Limit == other.Limit &&
                   Manufacturer == other.Manufacturer &&
                   FactoryNumber == other.FactoryNumber &&
                   Status == other.Status &&
                   Description == other.Description &&
                   RegistrationNumber == other.RegistrationNumber &&
                   (Tags == other.Tags || Tags.SequenceEqual(other.Tags));
        }
    }
}
