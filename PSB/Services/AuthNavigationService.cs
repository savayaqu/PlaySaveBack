using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using PSB.Views.Auth;
using PSB.Views.Settings;

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
                default:
                    pageType = typeof(LoginPage);
                    break;
            }
            _frame.Navigate(pageType, null, new SuppressNavigationTransitionInfo());

        }
    }
}
