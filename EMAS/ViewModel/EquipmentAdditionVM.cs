using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using EMAS.Model;
using EMAS.Service.Command;
using EMAS.Service.Connection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace EMAS.ViewModel
{
    public class EquipmentAdditionVM : INotifyPropertyChanged
    {
        public ObservableCollection<string> Names { get; set; }
        public ObservableCollection<string> Types { get; set; }
        public ObservableCollection<string> AccuracyClasses { get; set; }
        public ObservableCollection<string> Units { get; set; }
        public ObservableCollection<string> Limits { get; set; }
        public ObservableCollection<string> Manufacturers { get; set; }

        public string SelectedName { get; set; }
        public string SelectedType { get; set; }
        public string SelectedAccuracyClass { get; set; }
        public string SelectedUnit { get; set; }
        public string SelectedLimit { get; set; }
        public string SelectedManufacturer { get; set; }
        public string SelectedStatus { get; set; }
        public string SerialNumber { get; set; }
        public string RegistrationNumber { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }

        public ICommand AddCommand { get; set; }
        public int CurrentLocation { get; set; }
        public EquipmentAdditionVM()
        {
            InitializeComboBoxes();

            AddCommand = new RelayCommand(AddEquipment, CanAddEquipment);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void InitializeComboBoxes()
        {
            Names = new ObservableCollection<string>(DataBaseClient.GetDistincEquipmentNames());
            Types = new ObservableCollection<string>(DataBaseClient.GetDistincEquipmentTypes());
            AccuracyClasses = new ObservableCollection<string>(DataBaseClient.GetDistincEquipmentAccuracyClasses());
            Units = new ObservableCollection<string>(DataBaseClient.GetDistincEquipmentMeasurmentUnits());
            Limits = new ObservableCollection<string>(DataBaseClient.GetDistincEquipmentMeasurmentLimits());
            Manufacturers = new ObservableCollection<string>(DataBaseClient.GetDistincEquipmentManufacturers());
        }
        private void AddEquipment(object _)
        {
            Equipment equipment = new Equipment(SelectedStatus, RegistrationNumber, Description, SelectedAccuracyClass, SelectedUnit, SelectedLimit, 0, SelectedName, SelectedManufacturer, SelectedType, SerialNumber, [.. Tags.Split('\n')]);
            DataBaseClient.AddNewEquipment(equipment, CurrentLocation);
        }
        private bool CanAddEquipment(object _)
        {
            return !string.IsNullOrWhiteSpace(SelectedName) &&
            !string.IsNullOrWhiteSpace(SelectedType) &&
            !string.IsNullOrWhiteSpace(SelectedAccuracyClass) &&
                   !string.IsNullOrWhiteSpace(SelectedUnit) &&
                   !string.IsNullOrWhiteSpace(SelectedLimit) &&
                   !string.IsNullOrWhiteSpace(SelectedManufacturer) &&
                   !string.IsNullOrWhiteSpace(SerialNumber) &&
                   !string.IsNullOrWhiteSpace(RegistrationNumber) &&
                   !string.IsNullOrEmpty(SelectedStatus);
        }
    }
}

