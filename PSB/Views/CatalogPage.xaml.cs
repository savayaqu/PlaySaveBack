using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using PSB.Models;
using PSB.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CatalogPage : Page
    {
        public CatalogViewModel CatalogViewModel { get; set; }
        public CatalogPage()
        {
            this.InitializeComponent();
            CatalogViewModel = new CatalogViewModel();
        }
        private void OnGameTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Game game)
            {
                string gameTag = $"Game_{game.Id}|{game.Name}";
                App.NavigationService.Navigate(gameTag);
            }
        }
        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
           await CatalogViewModel.LoadGamesAsync();
        }
    }
}
