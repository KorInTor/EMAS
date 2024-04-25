using EMAS.Model;
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

namespace EMAS.Controls
{
    /// <summary>
    /// Interaction logic for HistoryEntryControl.xaml
    /// </summary>
    public partial class HistoryEntryControl : UserControl
    {
        public HistoryEntryControl()
        {
            InitializeComponent();
        }

        private void historyEntryButton_Click(object sender, RoutedEventArgs e)
        {
            if (FullInfoStackPanel.Visibility == Visibility.Collapsed)
                FullInfoStackPanel.Visibility = Visibility.Visible;
            else
                FullInfoStackPanel.Visibility = Visibility.Collapsed;
        }
    }
}
