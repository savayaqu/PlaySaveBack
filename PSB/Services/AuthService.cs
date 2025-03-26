using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using PSB.Utils;
using PSB.ViewModels;

namespace PSB.Services
{
    public class AuthService
    {
        private readonly ProfileViewModel _profileViewModel;
        private readonly NavigationViewItem _authNavItem;

        public AuthService(ProfileViewModel profileViewModel, NavigationViewItem authNavItem)
        {
            _profileViewModel = profileViewModel;
            _authNavItem = authNavItem;
        }

        public async Task UpdateAuthNavAsync()
        {
            try
            {
                if (AuthData.User != null && AuthData.Token != null)
                {
                    await _profileViewModel.LoadLibraryAsync();
                    _authNavItem.Tag = "ProfilePage";
                }
                else
                {
                    _profileViewModel.Libraries.Clear();
                    _authNavItem.Tag = "LoginPage";
                    _authNavItem.Content = "Войти";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating auth navigation: {ex.Message}");
            }
        }
    }
}
