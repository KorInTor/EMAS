using EMAS.Service.Connection;
using EMAS.ViewModel;
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
        public App()
        {
            ContentDispenser cd = new ContentDispenser();
            WindowManager wm = new WindowManager();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            DataBaseClient.CloseSession();

            base.OnExit(e);
        }
    }
}
