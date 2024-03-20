using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EMAS.ViewModel
{
    public class Tool
    {

        public Tool( string title, ICommand ToolAction)
        {

        }

        public string Title { get; set; }
        public ICommand ToolAction { get; set; }
    }
}
