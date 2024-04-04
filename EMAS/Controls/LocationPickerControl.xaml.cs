using EMAS.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EMAS.Controls
{
    /// <summary>
    /// Логика взаимодействия для LocationPickerControl.xaml
    /// </summary>
    public partial class LocationPickerControl : UserControl
    {
        public LocationPickerControl()
        {
            InitializeComponent();
            if (DataContext == null)
            {
                Debug.WriteLine($"DataContext не назанчен для{ nameof(LocationPickerControl)}");
                return;
            }
            LocationPickerVM dataContext = DataContext as LocationPickerVM;
            dataContext.ActionSuccessful += ShowSuccesfullMessage;
            dataContext.ActionFailed += ShowFailedMessage;
        }

        private void ShowFailedMessage(string message)
        {
            MessageBox.Show(message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowSuccesfullMessage(string message)
        {
            MessageBox.Show(message, "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
