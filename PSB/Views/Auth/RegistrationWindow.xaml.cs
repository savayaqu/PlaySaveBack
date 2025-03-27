using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Windows.Graphics;
using PSB.ViewModels.Auth;


namespace PSB.Views.Auth
{
    public sealed partial class RegistrationWindow : Window
    {
        public RegistrationViewModel RegistrationViewModel { get; }
        public RegistrationWindow()
        {
            RegistrationViewModel = new RegistrationViewModel();
            this.InitializeComponent();

            // Получаем AppWindow
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            // Устанавливаем фиксированный размер
            appWindow.Resize(new SizeInt32(500, 750));

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
