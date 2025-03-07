using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Response;
using PSB.ContentDialogs;
using PSB.Models;
using PSB.Utils;
using PSB.Views.Settings.Account;
using Windows.System;
using static PSB.Utils.Fetch;
using User = PSB.Models.User;

namespace PSB.ViewModels
{
    public partial class AccountViewModel: ObservableObject
    {
        [ObservableProperty] public partial User? User { get; set; } = AuthData.User;
        public AccountViewModel() {}

        [RelayCommand]
        public async Task ConnectionGoogleDrive()
        {
            Debug.WriteLine("ConnectionGoogleDrive command executed."); // Отладочное сообщение
            if (User != null)
            {
                (var res, var body) = await FetchAsync<ConnectionServiceResponse>(
                    HttpMethod.Get, "google-drive/auth-url",
                    setError: e => Debug.WriteLine($"Error: {e}")
                );
                if (res.IsSuccessStatusCode && body != null)
                {   
                    await Launcher.LaunchUriAsync(new Uri(body.Url));
                }
            }
        }
        [RelayCommand]
        public async Task UpdateEmail()
        {
            var dialog = new UpdateEmailContentDialog();
            await App.DialogService.ShowDialogAsync(dialog);
        }
        [RelayCommand]
        public async Task UpdatePassword()
        {
            var dialog = new UpdatePasswordContentDialog();
            await App.DialogService.ShowDialogAsync(dialog);
        }
    }
}
