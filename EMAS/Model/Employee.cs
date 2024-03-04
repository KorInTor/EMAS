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
    public class Employee
    {
        /// <summary>
        /// Stores unique id of Employee.
        /// </summary>
        private int _id;

        /// <summary>
        /// Stores fullname of employee.
        /// </summary>
        private string _fullname;

        /// <summary>
        /// Stroes departmnet name.
        /// </summary>
        private string _department;

        /// <summary>
        /// Email of Employee.
        /// </summary>
        private string _email;


        private string _username;

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
        /// <summary>
        /// Returns id of employee.
        /// </summary>
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Returns and sets Fullname of given employee.
        /// </summary>
        public string Fullname
        {
            get
            {
                return _fullname;
            }
            set
            {
                _fullname = value;
            }
        }

        /// <summary>
        /// Returns and sets Department of given employee.
        /// </summary>
        public string Department
        {
            get
            {
                return _department;
            }
            set
            {
                _department = value;
            }
        }

        public string Email
        {
            get;
            set;
        }

        public string Username { get => _username; set => _username = value; }
    }
}
