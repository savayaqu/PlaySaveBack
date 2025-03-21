using Microsoft.UI.Xaml;
using PSB.Services;
using PSB.Utils;
using Windows.Devices.Bluetooth.Advertisement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public static DialogService DialogService { get; private set; }
        public static MainWindow MainWindow { get; private set; }
        public static NavigationService NavigationService { get; private set; }
        public static LibraryService LibraryService { get; private set; }
        public static AuthService AuthService { get; private set; }
        public static SaveManager SaveManager { get; private set; }

        public App()
        {
            this.InitializeComponent();
            DialogService = new DialogService();
            SaveManager = new SaveManager();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.ExtendsContentIntoTitleBar = true;

            // Инициализируем NavigationService первым
            NavigationService = new NavigationService(MainWindow.ContentFrameControl, MainWindow.NavigationViewControl, MainWindow.HeaderTextBlock);

            // Теперь передаем NavigationService в LibraryService
            LibraryService = new LibraryService(MainWindow.NavigationViewControl, MainWindow.ProfileViewModel, NavigationService);

            // Инициализируем AuthService
            AuthService = new AuthService(MainWindow.ProfileViewModel, MainWindow.AuthNavControl);
            MainWindow.Activate();
        }


    }
}
