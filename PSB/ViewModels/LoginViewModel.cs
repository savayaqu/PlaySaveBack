using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Request;
using PSB.Api.Response;
using PSB.Utils;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(LoginCommand))] public partial string? Identifier { get; set; } = "";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(LoginCommand))] public partial string? Password { get; set; } = "";
        [ObservableProperty] public partial ErrorResponse Errors { get; set; } = new();
        [ObservableProperty] public partial string? Error { get; set; } = null;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(LoginCommand))] public partial bool IsFetch { get; set; } = false;
        private Task HandleValidationErrors(string errorResponse)
        {
            var parsedErrors = JsonSerializer.Deserialize<ErrorResponse>(errorResponse);
            Debug.WriteLine(parsedErrors);
            return Task.CompletedTask;
        }

        private bool CanLogin() =>
        Identifier != "" &&
        Password != "" &&
        !IsFetch;

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task Login()
        {
            var credentials = new CredentialsRequest(Identifier, Password);
            try
            {
                (var res, var body) = await FetchAsync<AuthResponse>(
                    HttpMethod.Post, "login",
                    new CredentialsRequest(Identifier, Password),
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