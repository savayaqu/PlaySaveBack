using System.Configuration;
using System.Data;
using System.Windows;
using playsaveback.Properties;
using playsaveback.Services;
using playsaveback.Views;

namespace playsaveback
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ApiService _apiService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _apiService = new ApiService();

                // Переходим в окно авторизации
                var loginWindow = new MainWindow();
                loginWindow.Show();
        }

        private bool IsAuthenticated()
        {
            var token = Settings.Default.AuthToken;
            return !string.IsNullOrEmpty(token);
        }
    }

}
