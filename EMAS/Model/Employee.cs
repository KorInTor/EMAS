using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model
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

        public Employee(int id, string fullname, string username, string email)
        {
            Id = id;
            Fullname = fullname;
            Username = username;
            Email = email;
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
    }
}
