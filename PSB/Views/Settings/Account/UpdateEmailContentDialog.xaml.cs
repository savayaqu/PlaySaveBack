using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PSB.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views.Settings.Account
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UpdateEmailContentDialog : ContentDialog
    {
        public UpdateEmailViewModel UpdateEmailViewModel { get; set; }
        public UpdateEmailContentDialog()
        {
            this.InitializeComponent();
            UpdateEmailViewModel = new UpdateEmailViewModel(this); // �������� ������ �� ������� ������

            DataContext = UpdateEmailViewModel;
            nav.Click += Nav_Click; // ���������� ����� �� �����������
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            // ������ ������� �� ����� ��� ���������� ������
            var updatePasswordDialog = new UpdatePasswordContentDialog();
            this.Hide(); // ������ ������� ������

            // ���������� ����� ������
            _ = App.DialogService.ShowDialogAsync(updatePasswordDialog);
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
