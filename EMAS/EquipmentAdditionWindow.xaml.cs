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
using System.Windows.Shapes;

namespace EMAS
{
    /// <summary>
    /// Логика взаимодействия для EquipmentAdditionWindow.xaml
    /// </summary>
    public partial class EquipmentAdditionWindow : Window
    {
        public EquipmentAdditionWindow()
        {
            InitializeComponent();
            EquipmentAdditionVM dataContext = (EquipmentAdditionVM)this.DataContext;
            dataContext.AdditionConfirmed += AdditionConfirmed;
            dataContext.AdditionFailed += AdditionFailed;
        }

        private void AdditionFailed(string obj)
        {
            MessageBox.Show($"Произошла ошибка добавления: \n\r {obj}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void AdditionConfirmed()
        {
            MessageBox.Show("Добавление успешно");
            this.Close();
        }
    }
}
