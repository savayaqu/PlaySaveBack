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
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && toggleButton.Tag is string tag)
            {
                // Находим PasswordBox по Tag
                var passwordBox = FindChild<PasswordBox>((StackPanel)toggleButton.Parent, tag);
                if (passwordBox != null)
                {
                    passwordBox.PasswordRevealMode = PasswordRevealMode.Visible; // Показываем пароль
                }
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && toggleButton.Tag is string tag)
            {
                // Находим PasswordBox по Tag
                var passwordBox = FindChild<PasswordBox>((StackPanel)toggleButton.Parent, tag);
                if (passwordBox != null)
                {
                    passwordBox.PasswordRevealMode = PasswordRevealMode.Hidden; // Скрываем пароль
                }
            }
        }

        // Вспомогательный метод для поиска дочернего элемента по Tag
        private T? FindChild<T>(DependencyObject parent, string tag) where T : FrameworkElement
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                if (child != null && child.Tag as string == tag && child is T result)
                {
                    return result;
                }
                var foundChild = FindChild<T>(child, tag);
                if (foundChild != null) return foundChild;
            }
            return null;
        }
    }
}
