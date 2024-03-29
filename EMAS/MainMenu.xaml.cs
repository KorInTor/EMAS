using EMAS.ViewModel;
using EMAS.Windows;
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
    /// Логика взаимодействия для MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();
            MainMenuVM dataContext = (MainMenuVM)this.DataContext;
            dataContext.OpenWindow += OpenWindow;
        }

        private void OpenWindow(int obj)
        {
            switch (obj)
            {
                case 0:
                    EquipmentWindow equipmentWindow = new EquipmentWindow();
                    equipmentWindow.ShowDialog();
                    break;
                case 1: 
                    break;
                case 2:
                    AdminWindow adminWindow = new AdminWindow();
                    adminWindow.ShowDialog();
                    break;
            }
        }
    }
}
