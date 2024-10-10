using CommunityToolkit.Mvvm.ComponentModel;
using Model.Enum;
using Service.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// Stores info about employee.
    /// </summary>
    public class Employee : ObservableObject
    {
        /// <summary>
        /// Stores unique id of Employee.
        /// </summary>
        private int _id = 0;

        /// <summary>
        /// Stores fullname of employee.
        /// </summary>
        private string _fullname = string.Empty;

        /// <summary>
        /// Stroes departmnet name.
        /// </summary>
        private string _department = string.Empty;

        /// <summary>
        /// Email of Employee.
        /// </summary>
        private string _email = string.Empty;

        private string _passwordHash = string.Empty;

        private string _username = string.Empty;

        private bool _isAdmin = false;

        public bool IsAdmin { get => _isAdmin; set => _isAdmin = value; }

        private List<Permission> _permissions = [];

        public Employee(int id, string fullname, string username, string email, List<Permission> permissions, bool isAdmin)
        {
            Id = id;
            Fullname = fullname;
            Username = username;
            Email = email;
            _permissions = permissions;
            IsAdmin = isAdmin;
        }

        public Employee(string fullname, string email, string username)
        {
            Fullname = fullname;
            Email = email;
            Username = username;
        }

        public Employee()
        {
        }

        /// <summary>
        /// Returns id of employee.
        /// </summary>
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// Returns and sets Fullname of given employee.
        /// </summary>
        public string Fullname
        {
            get => _fullname;
            set => SetProperty(ref _fullname, value);
        }

        /// <summary>
        /// Returns and sets Department of given employee.
        /// </summary>
        public string Department
        {
            get => _department;
            set => SetProperty(ref _department, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Username 
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string PasswordHash
        {
            get => _passwordHash;
            set => SetProperty(ref _passwordHash, value);
        }

        public List<Permission> Permissions { get => _permissions; set => _permissions = value; }

        public PermissionInfo PermissionInfo
        {
            get
            {
                Dictionary<int, List<string>> permissionsDictionary = [];

                foreach (var permission in _permissions)
                {
                    List<string> permissionsList = [];
                    if (!permissionsDictionary.ContainsKey(permission.LocationId))
                    {
                        permissionsDictionary.Add(permission.LocationId, []);
                    }
                    permissionsDictionary[permission.LocationId].Add(permission.ToString());
                }

                var permissionInfo = new PermissionInfo(IsAdmin, permissionsDictionary);

                return permissionInfo;
            }

            set
            {
                IsAdmin = value.IsCurrentEmployeeAdmin;
                _permissions.Clear();
                foreach(var key in value.Permissions.Keys)
                {
                    foreach (var permission in value.Permissions[key])
                    {
                        if (System.Enum.TryParse(permission, out PermissionType permissionType))
                        {
                            _permissions.Add(new Permission(key,permissionType));
                        }
                        else
                        {
                            throw new InvalidCastException("Unable to cast string to enum");
                        }
                    }
                }
            }
        }

    }
}
