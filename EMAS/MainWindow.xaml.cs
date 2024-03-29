using EMAS.ViewModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EMAS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AuthorizationVM dataContext = (AuthorizationVM)this.DataContext;
            dataContext.LoginFailed += ShowErrorMessage;
            dataContext.LoginSucceeded += OpenMainMenu;
        }

        private void OpenMainMenu()
        {
            MainMenu mainMenu = new();
            mainMenu.Show();
            this.Close();
        }

        private void ShowErrorMessage(string obj)
        {
            MessageBox.Show(obj, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                ((dynamic)this.DataContext).Password = ((PasswordBox)sender).Password;
            }
        }
    }
}