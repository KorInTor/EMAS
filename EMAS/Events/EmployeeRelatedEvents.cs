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

    }
}
