using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using PSB.Views.Settings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }
        private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs e)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            System.Type pageType;

            switch (selectedItem.Tag)
            {
                case "General":
                    pageType = typeof(GeneralPage);
                    break;
                case "Behavior":
                    pageType = typeof(BehaviorPage);
                    break;
                case "Account":
                    pageType = typeof(AccountPage);
                    break;
                default:
                    pageType = typeof(GeneralPage);
                    break;
            }
            ContentFrame.Navigate(pageType, null, new SuppressNavigationTransitionInfo());
        }
    }
}
