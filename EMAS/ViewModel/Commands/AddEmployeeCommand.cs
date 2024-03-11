using EMAS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EMAS.ViewModel.Commands
{
    public class AddEmployeeCommand : ICommand
    {

        public event EventHandler? CanExecuteChanged;

        public virtual bool CanExecute(object? parameter)
        {
            return true;
        }

        public virtual void Execute(object? parameter)
        {
            EmployeeRelatedEvents.InvokeEmployeeAdditionIsPerformed();
        }
    }
}
