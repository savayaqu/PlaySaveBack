using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using PSB.Services;
using PSB.ViewModels;
using PSB.Views.Settings;
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
        }
    }
}
