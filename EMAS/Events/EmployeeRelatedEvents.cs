using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Events
{
    public static class EmployeeRelatedEvents
    {
        public delegate void ChangePassword();
        public static event ChangePassword? PasswordChangeIsPerformed;

        public delegate void AddEmployee();
        public static event AddEmployee? EmployeeAdditionIsPerformed;

        public delegate void EditEmployee();
        public static event EditEmployee? EmployeeEditionIsPerformed;

        public delegate void ReceiveEmployeeInfo(List<Employee> employees);
        public static event ReceiveEmployeeInfo? EmployeeInfoIsReady;

        public delegate void PackEmployeeInfo();
        public static event PackEmployeeInfo? EmployeeInfoPackRequested;


        public static void InvokePasswordChangeIsPerformed()
        {
            PasswordChangeIsPerformed?.Invoke();
        }
        public static void InvokeEmployeeAdditionIsPerformed()
        {
            EmployeeAdditionIsPerformed?.Invoke();
        }

        public static void InvokeEmployeeEditionIsPerformed()
        {
            EmployeeEditionIsPerformed?.Invoke();
        }

        public static void InvokeEmployeeInfoIsReady(List<Employee> employees)
        {
            EmployeeInfoIsReady?.Invoke(employees);
        }

        public static void InvokeEmployeeInfoPackRequested()
        {
            EmployeeInfoPackRequested?.Invoke();
        }

    }
}
