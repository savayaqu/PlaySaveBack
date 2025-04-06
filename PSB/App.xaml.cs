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
        public static AuthWindow? AuthWindow { get; private set; }
        public static RegistrationWindow? RegistrationWindow { get; private set; }
        public static NavigationService? NavigationService { get; private set; }
        public static AuthNavigationService? AuthNavigationService { get; private set; }
        public static LibraryService? LibraryService { get; private set; }
        public static ZipHelper? ZipHelper { get; private set; }
        public static CloudFileUploader? CloudFileUploader { get; private set; }

        public App()
        {
            InitializeComponent();
            DialogService = new DialogService();
            ZipHelper = new ZipHelper();
            CloudFileUploader = new CloudFileUploader();
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
                InitializeAuthWindow();
            }
            
        }

        private void OnAppActivated(object? sender, AppActivationArguments args)
        {
            if (args.Kind == ExtendedActivationKind.Protocol)
            {
                var protocolArgs = (ProtocolActivatedEventArgs)args.Data;
                ProcessDeepLink(protocolArgs.Uri);
            }
        }

        private static void ProcessDeepLink(Uri uri)
        {
            // Вариант 1: Без await (если не нужно ждать завершения)
            MainWindow?.DispatcherQueue.TryEnqueue(() =>
            {
                if (uri.Scheme == "playsaveback" && uri.Host == "google-oauth")
                {
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    if (query["success"] == "1")
                    {
                        _ = MainWindow.AccountViewModel.LoadCloudServicesAsync();
                        NotificationService.ShowSuccess("Google Drive успешно подключен");
                    }
                }
            });
        }

        private static void InitializeMainWindow()
        {
            MainWindow = new MainWindow
            {
                ExtendsContentIntoTitleBar = true
            };

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
        private static void InitializeAuthWindow()
        {
            AuthWindow = new AuthWindow
            {
                ExtendsContentIntoTitleBar = true
            };
            AuthNavigationService = new AuthNavigationService(AuthWindow.ContentFrame);
            AuthWindow.Activate();
        }
        private static void InitializeRegistrationWindow()
        {
            RegistrationWindow = new RegistrationWindow
            {
                ExtendsContentIntoTitleBar = true
            };
            RegistrationWindow.Activate();
        }
        public static void SwitchToMain()
        {
            InitializeMainWindow();
            if (AuthWindow != null)
            {
                AuthWindow.Close();
                AuthWindow = null;
            }
            if(RegistrationWindow != null)
            {
                RegistrationWindow.Close();
                RegistrationWindow = null;
            }
        }
        public static void SwitchToLoginFromMain()
        {
            InitializeAuthWindow();
            if (MainWindow != null)
            {
                MainWindow.Close();
                MainWindow = null;
            }
        }
        public static void SwitchToRegistrationFromLogin()
        {
            InitializeRegistrationWindow();
            if (AuthWindow != null)
            {
                AuthWindow.Close();
                AuthWindow = null;
            }
        }
        public static void SwitchToLoginFromRegistration()
        {
            InitializeAuthWindow();
            if (RegistrationWindow != null)
            {
                RegistrationWindow.Close();
                RegistrationWindow = null;
            }
        }
    }
}