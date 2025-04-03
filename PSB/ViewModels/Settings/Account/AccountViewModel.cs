using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using PSB.Api.Request;
using PSB.Api.Response;
using PSB.Enums;
using PSB.Models;
using PSB.Utils;
using PSB.Views.Settings.Account;
using Windows.System;
using static PSB.Utils.Fetch;
using User = PSB.Models.User;

namespace PSB.ViewModels
{
    public partial class AccountViewModel : ObservableObject
    {
        [ObservableProperty] public partial User? User { get; set; } = AuthData.User;
        [ObservableProperty] public partial ObservableCollection<CloudService> CloudServices { get; set; } = new();
        [ObservableProperty] public partial ObservableCollection<CloudService> ConnectedCloudServices { get; set; } = new();
        public static AccountViewModel? Instance = App.MainWindow!.AccountViewModel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(User))]
        public partial UserVisibility SelectedVisibility { get; set; } = (UserVisibility)AuthData.User!.Visibility;
        public static Array VisibilityOptions => Enum.GetValues(typeof(UserVisibility));
        public AccountViewModel()
        {
            _ = LoadCloudServicesAsync();
            // Подписываемся на изменение SelectedVisibility
            PropertyChanged += async (sender, args) =>
            {
                if (args.PropertyName == nameof(SelectedVisibility))
                {
                    await UpdateProfileVisibility((int)SelectedVisibility);
                }
            };
        }
        [RelayCommand]
        public async Task UpdateProfileVisibility(int value)
        {
            (var res, var body) = await FetchAsync<User>(HttpMethod.Post, "profile", new UpdateAccountRequest.UpdateVisibility(value), true);
            if( res.IsSuccessStatusCode)
            {
                AuthData.User = body;
                User = body;
            }
        }
        [RelayCommand]
        public async Task ConnectionGoogleDrive()
        {
            Debug.WriteLine("ConnectionGoogleDrive command executed."); // Отладочное сообщение
            if (User != null)
            {
                (var res, var body) = await FetchAsync<ConnectionServiceResponse>(HttpMethod.Get, "google-drive/auth-url");
                if (res.IsSuccessStatusCode && body != null)
                {
                    await Launcher.LaunchUriAsync(new Uri(body.Url));
                }
            }
        }
        // Загрузка облачных сервисов
        public async Task LoadCloudServicesAsync()
        {
            (var res, var body) = await FetchAsync<List<CloudService>>(HttpMethod.Get, "profile/services");
            if (res.IsSuccessStatusCode && body != null)
            {
                CloudServices.Clear();
                foreach (var item in body)
                {
                    CloudServices.Add(item);
                    if(item.IsConnected)
                        AuthData.ConnectedCloudServices.Add(item);
                }
            }
        }
        [RelayCommand]
        public async Task ConnectService(CloudService cloudService)
        {
            Debug.WriteLine("нажата");
            if (cloudService.Name == "Google Drive")
            {
                await ConnectionGoogleDrive();
            }
        }
        [RelayCommand]
        public async Task UpdateEmail()
        {
            var dialog = new UpdateEmailContentDialog();
            await App.DialogService!.ShowDialogAsync(dialog);
        }
        [RelayCommand]
        public async Task UpdatePassword()
        {
            var dialog = new UpdatePasswordContentDialog();
            await App.DialogService!.ShowDialogAsync(dialog);
        }
    }
}
