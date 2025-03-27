using Microsoft.UI.Xaml;
using Microsoft.UI;
using Windows.Graphics;
using Microsoft.UI.Windowing;
using PSB.ViewModels;


namespace PSB.Views.Auth
{
    public sealed partial class LoginWindow : Window
    {
        public LoginViewModel LoginViewModel { get; }
        public LoginWindow()
        {
            LoginViewModel = new LoginViewModel();
            this.InitializeComponent();

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
