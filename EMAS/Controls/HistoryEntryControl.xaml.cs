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
        bool isPushed;   
        public HistoryEntryControl()
        {
            InitializeComponent();
            isPushed = false;
        }

        private void historyEntryButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var itemStackPanel = (StackPanel)button.Parent;

            if(isPushed == true)
            {
                for (int i = itemStackPanel.Children.Count - 1; i >= 0; i--)
                {
                    if (itemStackPanel.Children[i] is TextBlock)
                        itemStackPanel.Children.RemoveAt(i);
                }
                isPushed = false;
                return;
            }

            TextBlock actionType = new();
            TextBlock responsible = new();
            TextBlock contacts = new();
            TextBlock dateAndTime = new();

            actionType.Text = $"{((HistoryEntryBase)button.DataContext).TypeOfAction}";
            responsible.Text = $"Ответственный: {((HistoryEntryBase)button.DataContext).Responsible.Fullname}";
            contacts.Text = $"Контакты: {((HistoryEntryBase)button.DataContext).Responsible.Email}";
            dateAndTime.Text = $"Время: {((HistoryEntryBase)button.DataContext).Date}";

            itemStackPanel.Children.Add(actionType);
            itemStackPanel.Children.Add(dateAndTime);
            itemStackPanel.Children.Add(responsible);
            itemStackPanel.Children.Add(contacts);

            isPushed = true;
        }
    }
}
