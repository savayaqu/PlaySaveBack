using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using PSB.Views.Auth;

namespace PSB.Services
{
    public class AuthNavigationService
    {
        private readonly Frame _frame;
        public AuthNavigationService(Frame frame)
        {
            _frame = frame;
        }
        public void Navigate(string tag)
        {
            System.Type pageType;

            switch (tag)
            {
                case "Login":
                    pageType = typeof(LoginPage);
                    break;
                case "RestoreFromKey":
                    pageType = typeof(RestoreFromKeyPage);
                    break;
                case "RestoreFromMail":
                    pageType = typeof(RestoreFromMailPage);
                    break;
                default:
                    pageType = typeof(LoginPage);
                    break;
            }
            _frame.Navigate(pageType, null, new SuppressNavigationTransitionInfo());

        }
    }
}
