using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.EventArgs
{
    public class EmployeeListEventArgs
    {
        public EmployeeListEventArgs(List<Employee> employees)
        {
            EmployeeList = employees;
        }

        public List<Employee> EmployeeList;
    }
}
