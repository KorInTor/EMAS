using EMAS.ViewModel;
using System;
using System.Collections.Generic;
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

namespace EMAS.View.Control
{
    /// <summary>
    /// Логика взаимодействия для LocationControl.xaml
    /// </summary>
    public partial class LocationControl : UserControl
    {
        public LocationControl()
        {
            InitializeComponent();
            LocationControlVM locationControlVM = (LocationControlVM)DataContext;
            locationControlVM.AdditionConfirmed += OpenSuccesfullAdditionMessage;
            locationControlVM.AdditionFailed += OpenFailesAdditionMessage;
        }

        private void OpenFailesAdditionMessage(string message)
        {
            MessageBox.Show(message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OpenSuccesfullAdditionMessage(string message)
        {
            MessageBox.Show(message, "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
