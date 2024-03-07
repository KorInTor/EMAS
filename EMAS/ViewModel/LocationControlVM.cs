using EMAS.EventArgs;
using EMAS.Events;
using EMAS.Model;
using EMAS.Service.Command;
using EMAS.Service.Connection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Xaml.Schema;

namespace EMAS.ViewModel
{
    public class LocationControlVM : INotifyPropertyChanged //TODO: Добавить показ всплывающего окна если пользователь ввёл не правильные значения.
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<Location> _locations = new(DataBaseClient.GetLocationData());
        private string _newLocationName;
        private RelayCommand _addLocationCommand;

        public RelayCommand AddLocationCommand
        {
            get
            {
                return _addLocationCommand ??= new RelayCommand(param => AddNewLocation());
            }
        }

        public ObservableCollection<Location> Locations
        {
            get
            {
                return _locations;
            }
            private set
            {
                if (value != _locations)
                {
                    _locations = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Locations)));
                }
            }
        }

        public string NewLocationName
        {
            get
            {
                return _newLocationName;
            }
            set
            {
                if (value == string.Empty)
                {
                    return;
                }
                _newLocationName = value;
            }
        }

        public LocationControlVM()
        {
            MiscellaneousEvents.LocationPackageIsReady += AssertValues;
            MiscellaneousEvents.InvokeLocationPackageIsRequested();
        }

        private void AddNewLocation()
        {
            if (NewLocationName == null)
            {
                MessageBox.Show("Не заполнены значения для имени новго объекта.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DataBaseClient.AddNewLocation(new Location(0, NewLocationName));
            MessageBox.Show("Добавление нового объекта успешно", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
            MiscellaneousEvents.InvokeLocationPackageIsRequested();
            //UpdateLocationsList();
        }

        private void UpdateLocationsList()
        {
            Locations = new(DataBaseClient.GetLocationData());
        }

        private void AssertValues(LocationListEventArgs e)
        {
            Locations = new ObservableCollection<Location>(e.Locations);
        }
    }

}

