using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Response;
using PSB.Models;
using PSB.Utils;
using static PSB.Utils.Fetch;


namespace PSB.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        // Профиль и библиотека берутся из AuthData
        [ObservableProperty] public partial User? User { get; set; } = AuthData.User;
        public ObservableCollection<Library> Libraries => AuthData.Libraries;

        public ProfileViewModel()
        {
            //_ = LoadProfileAsync();
            //_ = LoadLibraryAsync();
        }

        [RelayCommand]
        public async Task LoadProfileAsync()
        {
            await AuthData.LoadProfileAsync();
        }

        [RelayCommand]
        public async Task LoadLibraryAsync()
        {
            await AuthData.LoadLibraryAsync();
            OnPropertyChanged(nameof(Libraries)); // Уведомляем интерфейс об изменении
        }

        [RelayCommand]
        private void Logout()
        {
            _ = AuthData.ExitAndNavigate();
        }
    }
}
