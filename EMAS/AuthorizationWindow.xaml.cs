using EMAS.ViewModel;
using Emas.View.Windows;
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
    /// Interaction logic for AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
            AuthorizationVM dataContext = (AuthorizationVM)this.DataContext;
            dataContext.LoginStarted += ShowThrobber;
            dataContext.LoginFailed += HideThrobber;
        }

        private void HideThrobber(string obj)
        {
            LoginInputGroupBox.Visibility = Visibility.Visible;
            LoadingSpinner.Visibility = Visibility.Collapsed;
        }

        private void ShowThrobber()
        {
            LoginInputGroupBox.Visibility = Visibility.Hidden;
            LoadingSpinner.Visibility = Visibility.Visible;
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