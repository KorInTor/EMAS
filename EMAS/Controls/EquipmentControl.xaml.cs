using EMAS.ViewModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace EMAS.Controls
{
    /// <summary>
    /// Логика взаимодействия для EquipmentControl.xaml
    /// </summary>
    public partial class EquipmentControl : UserControl
    {
        public EquipmentControl()
        {
            InitializeComponent();
            if (DataContext == null)
            {
                Debug.WriteLine("Не найден Data Context!");
                return;
            }
            EquipmentVM dataContext = (EquipmentVM)DataContext;
            dataContext.AdditionWindowRequested += OpenAdditionWindow;
        }

        private void OpenAdditionWindow(int obj)
        {
            EquipmentAdditionWindow additionWindow = new();
            additionWindow.DataContext = new EquipmentAdditionVM(obj);
            additionWindow.ShowDialog();
        }
    }
}
