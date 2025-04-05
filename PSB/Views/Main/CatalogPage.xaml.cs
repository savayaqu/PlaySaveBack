using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using PSB.Models;
using PSB.ViewModels;

namespace PSB.Views
{
    public sealed partial class CatalogPage : Page
    {
        public CatalogViewModel CatalogViewModel { get; }

        public CatalogPage()
        {
            CatalogViewModel = App.MainWindow!.CatalogViewModel;
            this.InitializeComponent();
            this.DataContext = CatalogViewModel;
            this.Loaded += OnPageLoaded; // Восстанавливаем обработчик
        }
        private bool _isFirstLoad = true;

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // Загружаем данные только если коллекция пуста и поиск не задействован
            if (!CatalogViewModel.Games.Any() && string.IsNullOrEmpty(CatalogViewModel.Name))
            {
                _isFirstLoad = true;

                CatalogViewModel.LoadGamesCommand.ExecuteAsync(null);
            }
        }
        private void Games_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Прокручиваем вверх только если это не первая загрузка
            if (!_isFirstLoad)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ScrollToTopSmoothly();
                });
            }
            _isFirstLoad = false;
        }
        private async void ScrollToTopSmoothly()
        {
            var scrollViewer = MainScrollViewer;
            double currentOffset = scrollViewer.VerticalOffset;
            double duration = 300; // milliseconds
            double startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            while (true)
            {
                double now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                double elapsed = now - startTime;

                if (elapsed >= duration)
                {
                    scrollViewer.ChangeView(null, 0, null, true);
                    break;
                }

                double progress = elapsed / duration;
                double newOffset = currentOffset * (1 - EaseOutQuad(progress));

                scrollViewer.ChangeView(null, newOffset, null, true);
                await Task.Delay(16); // ~60 FPS
            }
        }

        private static double EaseOutQuad(double t)
        {
            return t * (2 - t);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Отписываемся от события при уходе со страницы
            CatalogViewModel.Games.CollectionChanged -= Games_CollectionChanged;
            base.OnNavigatedFrom(e);
        }
        private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.ItemContainer.ContentTemplateRoot is Grid root &&
                args.Item is Game game)
            {
                var image = (Image)root.FindName("GameImage");
                var placeholder = (Border)root.FindName("Placeholder");

                if (args.Phase == 0)
                {
                    // Первая фаза - установка источника изображения
                    args.RegisterUpdateCallback(OnImageLoaded);
                }
                else if (args.Phase == 1)
                {
                    // Вторая фаза - анимация появления изображения
                    if (image.Opacity == 0 && image.Source != null)
                    {
                        var fadeIn = new DoubleAnimation
                        {
                            To = 1,
                            Duration = TimeSpan.FromMilliseconds(300),
                            EnableDependentAnimation = true
                        };
                        Storyboard.SetTarget(fadeIn, image);
                        Storyboard.SetTargetProperty(fadeIn, "Opacity");

                        var fadeOut = new DoubleAnimation
                        {
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(200),
                            EnableDependentAnimation = true
                        };
                        Storyboard.SetTarget(fadeOut, placeholder);
                        Storyboard.SetTargetProperty(fadeOut, "Opacity");

                        var storyboard = new Storyboard();
                        storyboard.Children.Add(fadeIn);
                        storyboard.Children.Add(fadeOut);
                        storyboard.Begin();
                    }
                }
            }
        }

        private void OnImageLoaded(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.RegisterUpdateCallback(OnContainerContentChanging);
        }

        private void GameButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Game game)
            {
                string gameTag = $"Game_{game.Id}|{game.Name}";
                App.NavigationService?.Navigate(gameTag);
            }
        }
    }
}