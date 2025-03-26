using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PSB.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views.Profile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UpdateProfileContentDialog : ContentDialog
    {
        public UpdateProfileViewModel UpdateProfileViewModel { get; set; }
        public UpdateProfileContentDialog()
        {
            this.InitializeComponent();
            UpdateProfileViewModel = new UpdateProfileViewModel(); // Передаем ссылку на текущий диалог

            DataContext = UpdateProfileViewModel;
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
