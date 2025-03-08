using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PSB.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views.Settings.Account
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UpdateEmailContentDialog : ContentDialog
    {
        public UpdateEmailViewModel UpdateEmailViewModel {  get; set; }
        public UpdateEmailContentDialog()
        {
            this.InitializeComponent();
            UpdateEmailViewModel = new UpdateEmailViewModel(this); // Передаем ссылку на текущий диалог

            DataContext = UpdateEmailViewModel;
            nav.Click += Nav_Click; // Обработчик клика на гиперссылку
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            // Меняем контент на форму для обновления пароля
            var updatePasswordDialog = new UpdatePasswordContentDialog();
            this.Hide(); // Скрыть текущий диалог

            // Показываем новый диалог
            _ = App.DialogService.ShowDialogAsync(updatePasswordDialog);
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
