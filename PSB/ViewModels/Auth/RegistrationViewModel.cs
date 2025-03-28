using System;
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
using PSB.Views.Auth;
using System.Text.Json.Nodes;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels.Auth
{
    public partial class RegistrationViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegistrationCommand))]
        public partial string? Email { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegistrationCommand))]
        public partial string? Password { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegistrationCommand))]
        public partial string? PasswordConfirmation { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegistrationCommand))]
        public partial string? Login { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Dictionary<string, string> Errors { get; set; } = new();

        [ObservableProperty]
        public partial string? Error { get; set; } = string.Empty;

        private SignUpResponse? _lastSuccessfulResponse;
        private SignUpRequest? _lastSentRequest;

        public string? PasswordError => Errors.TryGetValue("password", out var error) ? error : null;
        public string? PasswordConfirmationError => Errors.TryGetValue("password_confirmation", out var error) ? error : null;
        public string? EmailError => Errors.TryGetValue("email", out var error) ? error : string.Empty;
        public string? LoginError => Errors.TryGetValue("login", out var error) ? error : null;

        private bool CanRegistration() =>
            !string.IsNullOrEmpty(Login) &&
            !string.IsNullOrEmpty(Password) &&
            !string.IsNullOrEmpty(PasswordConfirmation) &&
            Password == PasswordConfirmation;

        [RelayCommand(CanExecute = nameof(CanRegistration))]
        private async Task Registration()
        {
            try
            {
                var currentRequest = new SignUpRequest(Login!, Email, Password!, PasswordConfirmation!);
                var node1 = JsonNode.Parse(JsonSerializer.Serialize(currentRequest));
                var node2 = JsonNode.Parse(JsonSerializer.Serialize(_lastSentRequest));

                if (JsonNode.DeepEquals(node1, node2))
                {
                    ShowKeyDialog(_lastSuccessfulResponse);
                    return;
                }
               
                // Отправляем запрос на сервер
                (var res, var body) = await FetchAsync<SignUpResponse>(
                    HttpMethod.Post, "register",
                    currentRequest,
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
                    // Сохраняем успешный ответ и отправленные данные
                    _lastSuccessfulResponse = body;
                    _lastSentRequest = currentRequest;

                    ShowKeyDialog(body);
                }
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
            }
        }

        private async void ShowKeyDialog(SignUpResponse response)
        {
            var dialog = new KeyContentDialog(response.Key);
            dialog.XamlRoot = App.RegistrationWindow!.Content.XamlRoot;
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                dialog.Hide();
                AuthData.SaveAndOpenMainWindow(response.Token, response.User);
            }
        }

        [RelayCommand]
        public static void GoToLogin()
        {
            App.SwitchToLoginFromRegistration();
        }
    }
}