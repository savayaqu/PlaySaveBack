using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using PSB.Helpers;
using PSB.Services;
using PSB.Utils;
using PSB.Views.Auth;
using Windows.ApplicationModel.Activation;
namespace PSB
{
    public partial class App : Application
    {
        public static DialogService? DialogService { get; private set; }
        public static MainWindow? MainWindow { get; private set; }
        public static LoginWindow? LoginWindow { get; private set; }
        public static RegistrationWindow? RegistrationWindow { get; private set; }
        public static NavigationService? NavigationService { get; private set; }
        public static LibraryService? LibraryService { get; private set; }
        public static ZipHelper? ZipHelper { get; private set; }

        public App()
        {
            InitializeComponent();
            DialogService = new DialogService();
            ZipHelper = new ZipHelper();
        }

        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Настраиваем single-instance и обработку deep links
            var instance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("main");
            if (!instance.IsCurrent)
            {
                // Если приложение уже запущено, перенаправляем активацию и закрываемся
                await instance.RedirectActivationToAsync(AppInstance.GetCurrent().GetActivatedEventArgs());
                Environment.Exit(0);
                return;
            }

            // Подписываемся на события активации (включая deep links)
            instance.Activated += OnAppActivated;
            if(AuthData.User != null)
            {
                // Инициализируем главное окно
                InitializeMainWindow();
            }
            else
            {
                InitializeLoginWindow();
            }
            
        }

        private async void OnAppActivated(object? sender, AppActivationArguments args)
        {
            if (args.Kind == ExtendedActivationKind.Protocol)
            {
                var protocolArgs = (ProtocolActivatedEventArgs)args.Data;
                await ProcessDeepLink(protocolArgs.Uri);
            }
        }

        private static async Task ProcessDeepLink(Uri uri)
        {
            if (MainWindow != null)
            {
                // Вариант 1: Без await (если не нужно ждать завершения)
                MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    if (uri.Scheme == "playsaveback" && uri.Host == "google-oauth")
                    {
                        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        if (query["success"] == "1")
                        {
                            _ = MainWindow.AccountViewModel.LoadCloudServices();
                        }
                    }
                });
            }
        }

        private static void InitializeMainWindow()
        {
            MainWindow = new MainWindow();
            MainWindow.ExtendsContentIntoTitleBar = true;

            NavigationService = new NavigationService(
                MainWindow.ContentFrameControl,
                MainWindow.NavigationViewControl,
                MainWindow.HeaderTextBlock);

            LibraryService = new LibraryService(
                MainWindow.NavigationViewControl,
                MainWindow.ProfileViewModel,
                NavigationService);

            MainWindow.Activate();
        }
        private static void InitializeLoginWindow()
        {
            LoginWindow = new LoginWindow();
            LoginWindow.ExtendsContentIntoTitleBar = true;
            LoginWindow.Activate();
        }
        private static void InitializeRegistrationWindow()
        {
            RegistrationWindow = new RegistrationWindow();
            RegistrationWindow.ExtendsContentIntoTitleBar = true;
            RegistrationWindow.Activate();
        }
        public static void SwitchToMain()
        {
            InitializeMainWindow();
            if (LoginWindow != null)
            {
                LoginWindow.Close();
                LoginWindow = null;
            }
            if(RegistrationWindow != null)
            {
                RegistrationWindow.Close();
                RegistrationWindow = null;
            }
        }
        public static void SwitchToLoginFromMain()
        {
            InitializeLoginWindow();
            if (MainWindow != null)
            {
                MainWindow.Close();
                MainWindow = null;
            }
        }
        public static void SwitchToRegistrationFromLogin()
        {
            InitializeRegistrationWindow();
            if (LoginWindow != null)
            {
                LoginWindow.Close();
                LoginWindow = null;
            }
        }
        public static void SwitchToLoginFromRegistration()
        {
            InitializeLoginWindow();
            if (RegistrationWindow != null)
            {
                RegistrationWindow.Close();
                RegistrationWindow = null;
            }
        }
    }
}