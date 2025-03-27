using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Request;
using PSB.Api.Response;
using PSB.Services;
using PSB.Utils;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels.Auth
{
    public partial class RegistrationViewModel : ObservableObject
    {
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RegistrationCommand))] public partial string? Email { get; set; } = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RegistrationCommand))] public partial string? Password { get; set; } = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RegistrationCommand))] public partial string? PasswordConfirmation { get; set; } = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RegistrationCommand))] public partial string? Login { get; set; } = string.Empty;
        [ObservableProperty] public partial Dictionary<string, string> Errors { get; set; } = new();
        [ObservableProperty] public partial string? Error { get; set; } = string.Empty;
        public string? PasswordError => Errors.TryGetValue("password", out var error) ? error : null;
        public string? PasswordConfirmationError => Errors.TryGetValue("password_confirmation", out var error) ? error : null;
        public string? EmailError => Errors.TryGetValue("email", out var error) ? error : string.Empty;
        public string? LoginError => Errors.TryGetValue("login", out var error) ? error : null;
        private bool CanRegistration() =>
            Login != string.Empty &&
            Password != string.Empty &&
            PasswordConfirmation != string.Empty &&
            Password == PasswordConfirmation;

        [RelayCommand(CanExecute = nameof(CanRegistration))]
        private async Task Registration()
        {
            try
            {
                //TODO:  Сделать отдельный респонс
                (var res, var body) = await FetchAsync<AuthResponse>(
                    HttpMethod.Post, "register",
                    new SignUpRequest(Login!, Email, Password!, PasswordConfirmation!),
                    serialize: true
                );

                if (res.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    var responseBody = await res.Content.ReadAsStringAsync();
                    Errors = ErrorHandlerService.ParseValidationErrors(responseBody);
                    OnPropertyChanged(nameof(PasswordError));
                    OnPropertyChanged(nameof(PasswordConfirmationError));
                    OnPropertyChanged(nameof(EmailError));
                    OnPropertyChanged(nameof(LoginError));
                    return;
                }
                if (res.IsSuccessStatusCode && body != null)
                {
                    AuthData.SaveAndOpenMainWindow(body.Token, body.User);
                    //TODO:  Показать пользователю его секретный код
                }
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
            }
        }
        [RelayCommand]
        public static void GoToLogin()
        {
            App.SwitchToLoginFromRegistration();
        }
    }
}
