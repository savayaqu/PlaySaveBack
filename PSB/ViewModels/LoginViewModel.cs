using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Request;
using PSB.Api.Response;
using PSB.Utils;
using Windows.Storage;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(LoginCommand))] public partial string? Email { get; set; } = "";
        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(LoginCommand))] public partial string? Password { get; set; } = "";
        [ObservableProperty] public partial ErrorResponse Errors { get; set; } = new();
        [ObservableProperty] public partial string? Error { get; set; } = null;
        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(LoginCommand))] public partial bool IsFetch { get; set; } = false;
        private Task HandleValidationErrors(string errorResponse)
        {
            var parsedErrors = JsonSerializer.Deserialize<ErrorResponse>(errorResponse);
            Email = parsedErrors?.Errors?.GetValueOrDefault("email")?.FirstOrDefault();
            Password = parsedErrors?.Errors?.GetValueOrDefault("password")?.FirstOrDefault();
            return Task.CompletedTask;
        }

        private bool CanLogin() =>
        Email != "" &&
        Password != "" &&
        !IsFetch;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task Login()
        {
            //Error = $"Email: {Email}, Password: {Password}";
            var credentials = new CredentialsRequest(Email, Password);
            try
            {
                (var res, var body) = await FetchAsync<AuthResponse>(
                    HttpMethod.Post, "login",
                    isFetch => IsFetch = isFetch,
                    error => Error = error,
                    new CredentialsRequest(Email, Password),
                    serialize: true
                );

                if (!res.IsSuccessStatusCode && res.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    var responseBody = await res.Content.ReadAsStringAsync();
                    await HandleValidationErrors(responseBody);
                    return;
                }
                if (body != null)
                {
                    AuthData.SaveAndNavigate(body.Token, body.User);
                    // Сброс ошибок при успешном логине
                    Errors = new ErrorResponse();
                }
            }
            catch (HttpRequestException ex)
            {
                Error = "Ошибка соединения: " + ex.Message;
            }
        }
    }
}
