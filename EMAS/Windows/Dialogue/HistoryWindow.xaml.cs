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
using System.Windows.Shapes;

namespace EMAS.Windows.Dialogue
{
    /// <summary>
    /// Interaction logic for HistoryWindow.xaml
    /// </summary>
    public partial class HistoryWindow : Window
    {
        public HistoryWindow()
        {
            InitializeComponent();
        }

        private void historyEntryButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var itemStackPanel = (StackPanel)button.Parent;

            TextBox actionType = new();
            TextBox responsible = new();
            TextBox contacts = new();
            TextBox dateAndTime = new();

            actionType.Text = $"{((HistoryEntryBase)button.DataContext).TypeOfAction}";
            responsible.Text = $"Ответственный: {((HistoryEntryBase)button.DataContext).Responsible.Fullname}";
            contacts.Text = $"Контакты: {((HistoryEntryBase)button.DataContext).Responsible.Email}";
            dateAndTime.Text = $"Время: {((HistoryEntryBase)button.DataContext).Date}";

            itemStackPanel.Children.Add(actionType);
            itemStackPanel.Children.Add(dateAndTime);
            itemStackPanel.Children.Add(responsible);
            itemStackPanel.Children.Add(contacts);
        }
    }
}
