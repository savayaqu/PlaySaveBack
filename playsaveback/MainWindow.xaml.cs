using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using playsaveback.Views;

namespace playsaveback
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new MainPage());
        }
        private void NavigateBack(object sender, RoutedEventArgs e)
        {
            // Проверяем, можем ли вернуться назад
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
            else
            {
                MessageBox.Show("There is no page to go back to.", "Navigation");
            }
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Включаем или отключаем кнопку "Назад" в зависимости от доступности
            var backButton = (Button)LogicalTreeHelper.FindLogicalNode(this, "BackButton");
            if (backButton != null)
            {
                backButton.IsEnabled = MainFrame.CanGoBack;
            }
        }

        private void NavigateToHome(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MainPage());
        }

        private void NavigateToSettings(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SettingsPage());
        }
    }
}