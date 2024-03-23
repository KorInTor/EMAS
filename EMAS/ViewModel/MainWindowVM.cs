using DocumentFormat.OpenXml.Bibliography;
using EMAS.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    public class MainWindowVM
    {

        public MainWindowVM()
        {            
            GeneralEvents.InvokeLocationsNamesListIsRequested();
        }

        public ObservableCollection<string> Locations
        {
            get;
            set;
        }
        
        
    }
}
