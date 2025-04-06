using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PSB.ViewModels;

namespace PSB.Views.Settings.Account
{
    public sealed partial class UpdateEmailContentDialog : ContentDialog
    {
        public UpdateEmailViewModel UpdateEmailViewModel { get;  private set; }
        public UpdateEmailContentDialog()
        {
            UpdateEmailViewModel = new UpdateEmailViewModel(); // Передаем ссылку на текущий диалог

            this.InitializeComponent();

            DataContext = UpdateEmailViewModel;
            nav.Click += Nav_Click; // Обработчик клика на гиперссылку
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            this.Hide(); // Скрыть текущий диалог
            // Показываем новый диалог
            _ = App.DialogService!.ShowDialogAsync(new UpdatePasswordContentDialog());
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
