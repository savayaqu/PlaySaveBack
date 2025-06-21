using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Services;
using PSB.Utils;
using PSB.Views.Auth;
using System;
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
        public static IGameLaunchService? GameLaunchService { get; private set; }
        public static AuthNavigationService? AuthNavigationService { get; private set; }
        public static LibraryService? LibraryService { get; private set; }
        public static ZipHelper? ZipHelper { get; private set; }
        public static CloudFileUploader? CloudFileUploader { get; private set; }

        public App()
        {
            InitializeComponent();
            DialogService = new DialogService();
            GameLaunchService = new GameLaunchService();
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
            if (AuthData.User != null)
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

        private static void InitializeMainWindow(Window? previousWindow = null)
        {
            MainWindow = new MainWindow
            {
                ExtendsContentIntoTitleBar = true
            };

            if (previousWindow != null)
            {
                ApplyWindowPosition(previousWindow, MainWindow);
            }

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

        private static void InitializeAuthWindow(Window? previousWindow = null)
        {
            AuthWindow = new AuthWindow
            {
                ExtendsContentIntoTitleBar = true
            };

            if (previousWindow != null)
            {
                ApplyWindowPosition(previousWindow, AuthWindow);
            }

            AuthNavigationService = new AuthNavigationService(AuthWindow.ContentFrame);
            AuthWindow.Activate();
        }

        private static void InitializeRegistrationWindow(Window? previousWindow = null)
        {
            RegistrationWindow = new RegistrationWindow
            {
                ExtendsContentIntoTitleBar = true
            };

            if (previousWindow != null)
            {
                ApplyWindowPosition(previousWindow, RegistrationWindow);
            }

            RegistrationWindow.Activate();
        }

        private static void ApplyWindowPosition(Window source, Window target)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(source);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            var targetHwnd = WinRT.Interop.WindowNative.GetWindowHandle(target);
            var targetWindowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(targetHwnd);
            var targetAppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(targetWindowId);

            // Получаем только позицию окна (без изменения размера)
            var position = appWindow.Position;
            var size = targetAppWindow.Size; // Сохраняем текущий размер нового окна

            // Создаем RectInt32 с новой позицией и текущим размером
            var newRect = new Windows.Graphics.RectInt32(
                position.X,
                position.Y,
                size.Width,
                size.Height);

            targetAppWindow.MoveAndResize(newRect);
        }

        public static void SwitchToMain()
        {
            Window? previousWindow = AuthWindow ?? (Window?)RegistrationWindow;
            InitializeMainWindow(previousWindow);

            if (AuthWindow != null)
            {
                AuthWindow.Close();
                AuthWindow = null;
            }
            if (RegistrationWindow != null)
            {
                RegistrationWindow.Close();
                RegistrationWindow = null;
            }
        }

        public static void SwitchToLoginFromMain()
        {
            InitializeAuthWindow(MainWindow);
            if (MainWindow != null)
            {
                MainWindow.Close();
                MainWindow = null;
            }
        }

        public static void SwitchToRegistrationFromLogin()
        {
            InitializeRegistrationWindow(AuthWindow);
            if (AuthWindow != null)
            {
                AuthWindow.Close();
                AuthWindow = null;
            }
        }

        public static void SwitchToLoginFromRegistration()
        {
            InitializeAuthWindow(RegistrationWindow);
            if (RegistrationWindow != null)
            {
                RegistrationWindow.Close();
                RegistrationWindow = null;
            }
        }
    }
}