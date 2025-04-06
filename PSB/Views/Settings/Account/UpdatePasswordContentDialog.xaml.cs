using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using PSB.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views.Settings.Account
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UpdatePasswordContentDialog : ContentDialog
    {
        public UpdatePasswordViewModel UpdatePasswordViewModel { get; set; } = new UpdatePasswordViewModel();

        public UpdatePasswordContentDialog()
        {
            this.InitializeComponent();
            DataContext = UpdatePasswordViewModel;
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            // Меняем контент на форму для обновления пароля
            var updateEmailDialog = new UpdateEmailContentDialog();
            this.Hide(); // Скрыть текущий диалог

            // Показываем новый диалог
            _ = App.DialogService.ShowDialogAsync(updateEmailDialog);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
