using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using PSB.Api.Request;
using PSB.Api.Response;
using PSB.Services;
using PSB.Utils;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(LoginCommand))] public partial string? Identifier { get; set; } = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(LoginCommand))] public partial string? Password { get; set; } = string.Empty;
        [ObservableProperty] public partial Dictionary<string, string> Errors { get; set; } = new();
        [ObservableProperty] public partial string? Error { get; set; } = string.Empty;
        public string? PasswordError => Errors.TryGetValue("password", out var error) ? error : null;
        public string? IdentifierError => Errors.TryGetValue("identifier", out var error) ? error : null;
        private bool CanLogin() =>
        Identifier != string.Empty &&
        Password != string.Empty;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task Login()
        {
            try
            {
                (var res, var body) = await FetchAsync<AuthResponse>(
                    HttpMethod.Post, "login",
                    new CredentialsRequest(Identifier!, Password!),
                    serialize: true
                );

                if (res.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    var responseBody = await res.Content.ReadAsStringAsync();
                    Errors = ErrorHandlerService.ParseValidationErrors(responseBody);
                    OnPropertyChanged(nameof(PasswordError));
                    OnPropertyChanged(nameof(IdentifierError));
                    Debug.WriteLine(Errors.Values);
                    Debug.WriteLine(IdentifierError);
                    Debug.WriteLine(PasswordError);
                    return;
                }
                if(res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Error = "Invalid credentials";
                }
                
                if (res.IsSuccessStatusCode && body != null)
                {
                    AuthData.SaveAndOpenMainWindow(body.Token, body.User);
                }
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
            }
        }
        [RelayCommand]
        public static void GoToRegistation()
        {
            App.SwitchToRegistrationFromLogin();
        }
    }
}