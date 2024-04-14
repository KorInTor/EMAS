using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Model.Enum;
using EMAS.Service;
using EMAS.Service.Connection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    public partial class PermissionChangerVM : ObservableObject
    {
        public RelayCommand ConfirmPermissionChange { get;set ;}
        public static event Action<string> PermissionChangeSucessfull;
        public static event Action<string> PermissionChangeFailed;

        public string WindowHeader
        {
            get => "Изменение прав для: " + ManagedEmployee.Fullname;
        }

        [ObservableProperty]
        private Employee managedEmployee = new();

        [ObservableProperty]
        private ObservableCollection<LocationPermission> _locationPermissionList = new ObservableCollection<LocationPermission>();

        [ObservableProperty]
        private LocationPermission _selectedLocation;

        private PermissionInfo currentUserPermissions = SessionManager.PermissionInfo;
        public static IWindowsDialogueService DialogueService { get; private set; }

        public PermissionChangerVM()
        {
            ConfirmPermissionChange = new RelayCommand(TryChangeEmployeePermission);
        }

        private void TryChangeEmployeePermission()
        {
            Dictionary<int,List<string>> newPermissions = [];

            foreach (var locationPermission in LocationPermissionList)
            {
                newPermissions.Add(locationPermission.AsKeyValuePairForPermissionInfo().Key, locationPermission.AsKeyValuePairForPermissionInfo().Value);
            }
            ManagedEmployee.PermissionInfo = new PermissionInfo(false,newPermissions);
            try
            {
                DataBaseClient.GetInstance().Update(ManagedEmployee);
                PermissionChangeSucessfull?.Invoke("Успешно изменены права доступа");
                DialogueService.ShowSuccesfullMessage("Успешно изменены права доступа");
            }
            catch(Exception exception)
            {
                PermissionChangeFailed?.Invoke(exception.Message);
                DialogueService.ShowFailMessage(exception.Message);
            }
        }

        public void InitValues(Employee managedEmployee, Dictionary<int, string>? namedLocations = null)
        {
            ManagedEmployee = managedEmployee;

            namedLocations ??= DataBaseClient.GetInstance().SelectNamedLocations();

            foreach(var namedLocation in namedLocations)
            {
                LocationPermission location = new();
                location.Id = namedLocation.Key;
                location.Name = namedLocation.Value;
                location.CheckBoxItems = new();
                foreach (string permission in Enum.GetValues(typeof(PermissionType)).Cast<PermissionType>().Select(e => e.ToString()).ToList())
                {
                    location.CheckBoxItems.Add(new());
                    location.CheckBoxItems[location.CheckBoxItems.Count - 1].Content = permission;
                    location.CheckBoxItems[location.CheckBoxItems.Count - 1].IsChecked = false;
                }
                foreach (var item in location.CheckBoxItems)
                {
                    foreach (string truePermission in ManagedEmployee.PermissionInfo.Permissions[location.Id])
                    {
                        if (item.Content == truePermission)
                        {
                            item.IsChecked = true;
                        }
                    }
                }

                LocationPermissionList.Add(location);

            }
        }
    }

    public class CheckBoxItem : ObservableObject
    {
        private bool _isChecked;

        public string Content { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
    }
    public class LocationPermission : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ObservableCollection<CheckBoxItem> CheckBoxItems { get; set; }

        public KeyValuePair<int,List<string>> AsKeyValuePairForPermissionInfo()
        {
            var pair = new KeyValuePair<int, List<string>>(Id, []);
            foreach (var item in CheckBoxItems)
            {
                if (item.IsChecked)
                {
                    pair.Value.Add(item.Content);
                }
            }

            return pair;
        }
    }
}
