using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Response;
using PSB.Interfaces;
using PSB.Models;
using PSB.Services;
using PSB.Utils;
using PSB.Utils.Game;
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

        public AccountViewModel()
        {
            _ = LoadCloudServicesAsync();
        }
        // Подключение гугл драйва
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
        // Отключение гугл драйва
        [RelayCommand]
        public async Task DisconnectionGoogleDrive(CloudService cloudService)
        {
            Debug.WriteLine("DisconnectionGoogleDrive command executed."); // Отладочное сообщение
            if (User != null)
            {
                var res = await FetchAsync(HttpMethod.Delete, $"google-drive/disconnect/{cloudService.UserCloudServiceId}");
                if (res.IsSuccessStatusCode)
                {
                    SavesDataManager<IGame>.RemoveAllSavesByCloudServiceId(cloudService.UserCloudServiceId);
                    _ = MainWindow.Instance!.AccountViewModel.LoadCloudServicesAsync();
                    NotificationService.ShowSuccess("Google Drive успешно отключен\n Все сохранения скрыты, зайдите повторно, чтобы они появились");
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
                if(cloudService.IsConnected == false || cloudService.IsTokenExpired())
                {
                    await ConnectionGoogleDrive();
                }
                else
                {
                    await DisconnectionGoogleDrive(cloudService);
                }
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
