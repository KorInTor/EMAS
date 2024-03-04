using EMAS.Service.Connection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace EMAS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            DataBaseClient.CloseSession();

            base.OnExit(e);
        }
    }
}
