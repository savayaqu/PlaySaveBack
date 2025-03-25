using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using PSB.Services;
using PSB.Utils;
using Windows.ApplicationModel.Activation;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using PSB.Helpers; // Для ProtocolActivatedEventArgs

namespace PSB
{
    public partial class App : Application
    {
        public static DialogService? DialogService { get; private set; }
        public static MainWindow? MainWindow { get; private set; }
        public static NavigationService? NavigationService { get; private set; }
        public static LibraryService? LibraryService { get; private set; }
        public static AuthService? AuthService { get; private set; }
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

            // Инициализируем главное окно
            InitializeMainWindow();
        }

        private async void OnAppActivated(object? sender, AppActivationArguments args)
        {
            if (args.Kind == ExtendedActivationKind.Protocol)
            {
                var protocolArgs = (ProtocolActivatedEventArgs)args.Data;
                await ProcessDeepLink(protocolArgs.Uri);
            }
        }

        public static async Task ProcessDeepLink(Uri uri)
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
                            Debug.WriteLine("подкл");
                            // Здесь можно вызывать синхронные методы
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

            AuthService = new AuthService(
                MainWindow.ProfileViewModel,
                MainWindow.AuthNavControl);

            MainWindow.Activate();
        }
    }
}