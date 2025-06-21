using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PSB.ViewModels;
using Windows.Graphics;


namespace PSB.Views.Auth
{
    public sealed partial class AuthWindow : Window
    {
        public LoginViewModel LoginViewModel { get; }
        public Frame ContentFrame => AuthContentFrame;

        public AuthWindow()
        {
            LoginViewModel = new LoginViewModel();
            this.InitializeComponent();
            AuthContentFrame.Navigate(typeof(LoginPage));
            // Получаем AppWindow
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            // Устанавливаем иконку
            AppWindow.SetIcon("Assets/Logo.ico");
            // Устанавливаем фиксированный размер
            appWindow.Resize(new SizeInt32(500, 600));
            // Получаем текущий Presenter или создаем новый
            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.IsResizable = false;
                overlappedPresenter.IsMaximizable = false;
            }
            else
            {
                appWindow.SetPresenter(OverlappedPresenter.Create());
                if (appWindow.Presenter is OverlappedPresenter newPresenter)
                {
                    newPresenter.IsResizable = false;
                    newPresenter.IsMaximizable = false;
                }
            }
            // Центруем приложение
            var area = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest)?.WorkArea;
            if (area == null) return;
            AppWindow.Move(new PointInt32((area.Value.Width - AppWindow.Size.Width) / 2, (area.Value.Height - AppWindow.Size.Height) / 2));
        }
    }
}
