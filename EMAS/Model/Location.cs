using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model
{
    public class Location
    {
        /// <summary>
        /// Unique id of this site.
        /// </summary>
        private int _id;

        /// <summary>
        /// Name of site.
        /// </summary>
        private string _name;

        /// <summary>
        /// Equipments that are currently in this site.
        /// </summary>
        private List<Equipment>? _equipments;

        /// <summary>
        /// Employees woring on this site.
        /// </summary>
        private List<Employee>? _employees;

        /// <summary>
        /// List of Outgoing and Incoming deliviries.
        /// </summary>
        private List<Delivery>? _deliviries;

        /// <summary>
        /// Return unique id of this site.
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
        /// Return name of site.
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
        /// Return and sets equipment list that are currently in this site.
        /// </summary>
        public List<Equipment> Equipments
        {
            get
            {
                return _equipments;
            }
            set
            {
                _equipments = value;
            }
        }

        /// <summary>
        /// Return and set list of Delivery on this site.
        /// </summary>
        public List<Delivery> Deliveries
        {
            get
            {
                return _deliviries;
            }
            set
            {
                _deliviries = value;
            }
        }

        /// <summary>
        /// Return and set list of Employees on this site.
        /// </summary>
        public List<Employee> Employees
        {
            get
            {
                return _employees;
            }
            set
            {
                _employees = value;
            }
        }

        /// <summary>
        /// Creates instance of site with Lists for equipment, Delivery, Reservation.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Name"></param>
        public Location(int Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
            Equipments = new List<Equipment>();
            Deliveries = new List<Delivery>();
        }

        public Location()
        {
        }
    }
}
