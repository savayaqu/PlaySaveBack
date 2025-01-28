using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PSB.Utils;
using Windows.Storage;

namespace PSB
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }

        public MainWindow()
        {
            Instance = this; // Сохраняем текущий экземпляр

            this.InitializeComponent();
            //ContentFrame.Navigate(typeof(Views.HomePage));
            ContentFrame.Navigated += ContentFrame_Navigated; // Подписываемся на событие Navigated

            UpdateAuthNav();
        }
        public void Nav(string pageTag)
        {
            Type? pageType = Type.GetType($"PSB.Views.{pageTag}");
            if (pageType != null)
            {
                ContentFrame.Navigate(pageType);
            }
            else
            {
                // Можно добавить сообщение об ошибке или обработку ситуации
                ContentFrame.Content = new TextBlock { Text = "Page not found" };
            }
        }
        public void UpdateAuthNav()
        {
            if (AuthData.User != null && AuthData.Token != null)
            {
                // Если пользователь авторизован, меняем элемент навигации на профиль
                AuthNav.Tag = "ProfilePage";
                AuthNav.Content = AuthData.User.Nickname;
            }
            else
            {
                // Если пользователь не авторизован, возвращаемся к LoginPage
                AuthNav.Tag = "LoginPage";
                AuthNav.Content = "LoginPage";
            }
        }
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                Nav(selectedItem.Tag.ToString());
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        // Обработчик события Navigated для обновления заголовка и синхронизации NavigationView
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // TODO: можно сделать что-то типа русификации

            // Обновляем заголовок в зависимости от текущей страницы
            if (e.Content is Page page)
            {
                //HeaderText.Text = page.GetType().Name.Replace("Page", ""); // Убираем "Page" из имени
                HeaderText.Text = page.GetType().Name;
            }

            // Управляем видимостью кнопки "Назад"
            BackButton.Visibility = ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;

            // Синхронизируем выбранный элемент в NavigationView
            SyncNavigationViewSelection();
        }

        // Метод для синхронизации выбранного элемента в NavigationView
        private void SyncNavigationViewSelection()
        {
            if (ContentFrame.Content is Page page)
            {
                string pageName = page.GetType().Name; // Получаем имя текущей страницы

                // Ищем соответствующий NavigationViewItem
                foreach (var item in nvSample.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == pageName)
                    {
                        nvSample.SelectedItem = navItem;
                        break;
                    }
                }
            }
        }
    }
}