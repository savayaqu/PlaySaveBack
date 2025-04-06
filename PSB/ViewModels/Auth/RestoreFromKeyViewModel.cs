using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using PSB.Api.Request;
using PSB.Models;
using PSB.Services;
using PSB.Utils;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels.Auth
{
    public partial class RestoreFromKeyViewModel : ObservableObject
    {
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RestoreCommand))] public partial string? Login { get; set; } = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RestoreCommand))] public partial string? Key { get; set; } = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RestoreCommand))] public partial string? NewPassword { get; set; } = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RestoreCommand))] public partial string? NewPasswordConfirmation { get; set; } = string.Empty;
        [ObservableProperty]public partial bool LogoutBool { get; set; } = false;
        [ObservableProperty] public partial Dictionary<string, string> Errors { get; set; } = new();
        [ObservableProperty] public partial string? Error { get; set; } = string.Empty;
        public string? LoginError => Errors.TryGetValue("login", out var error) ? error : null;
        public string? KeyError => Errors.TryGetValue("key", out var error) ? error : null;
        public string? NewPasswordError => Errors.TryGetValue("new_password", out var error) ? error : null;
        public string? NewPasswordConfirmationError => Errors.TryGetValue("new_password_confirmation", out var error) ? error : null;
        private bool CanRestore() =>
        Login != string.Empty &&
        Key != string.Empty &&
        NewPassword != string.Empty &&
        NewPasswordConfirmation != string.Empty &&
        Key != string.Empty;

        [RelayCommand(CanExecute = nameof(CanRestore))]
        private async Task Restore()
        {
            try
            {
                var res = await FetchAsync(
                    HttpMethod.Post, "restore-from-key",
                    new RestoreFromKeyRequest(Login!, Key!, NewPassword!, NewPasswordConfirmation!, LogoutBool),
                    serialize: true
                );

                if (res.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    var responseBody = await res.Content.ReadAsStringAsync();
                    Errors = ErrorHandlerService.ParseValidationErrors(responseBody);
                    OnPropertyChanged(nameof(LoginError));
                    OnPropertyChanged(nameof(KeyError));
                    OnPropertyChanged(nameof(NewPasswordError));
                    OnPropertyChanged(nameof(NewPasswordConfirmationError));
                    return;
                }

                if (res.IsSuccessStatusCode)
                {
                    App.DialogService!.SetXamlRoot(App.AuthWindow!.Content.XamlRoot);
                    bool result = await App.DialogService!.ShowConfirmationAsync("Успех", "Желаете перейти на страницу авторизации?");
                    if (result)
                        NavigateToLogin();
                    Login = string.Empty;
                    Key = string.Empty;
                    NewPassword = string.Empty;
                    NewPasswordConfirmation = string.Empty;
                    LogoutBool = false;
                }
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
            }
        }
        [RelayCommand]
        public void NavigateToLogin()
        {
            App.AuthNavigationService!.Navigate("Login");
        }
    }
}
