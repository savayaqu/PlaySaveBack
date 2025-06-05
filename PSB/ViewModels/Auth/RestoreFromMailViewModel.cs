using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using PSB.Api.Request.Mail;
using PSB.Api.Response;
using PSB.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels.Auth
{
    public partial class RestoreFromMailViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RestoreCommand))]
        [NotifyCanExecuteChangedFor(nameof(SendCodeCommand))]
        public partial string? Email { get; set; } = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RestoreCommand))] public partial string? Code { get; set; } = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RestoreCommand))] public partial string? ResetToken { get; set; } = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RestoreCommand))] public partial string? NewPassword { get; set; } = string.Empty;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(RestoreCommand))] public partial string? NewPasswordConfirmation { get; set; } = string.Empty;
        [ObservableProperty] public partial bool LogoutBool { get; set; } = false;
        [ObservableProperty] public partial Dictionary<string, string> Errors { get; set; } = new();
        [ObservableProperty] public partial string? Error { get; set; } = string.Empty;
        public string? EmailError => Errors.TryGetValue("email", out var error) ? error : null;
        public string? CodeError => Errors.TryGetValue("code", out var error) ? error : null;
        public string? NewPasswordError => Errors.TryGetValue("new_password", out var error) ? error : null;
        public string? NewPasswordConfirmationError => Errors.TryGetValue("new_password_confirmation", out var error) ? error : null;
        [ObservableProperty] public partial bool IsSendForm { get; set; } = true;
        [ObservableProperty] public partial bool IsConfirmForm { get; set; } = false;
        [ObservableProperty] public partial bool IsRestoreForm { get; set; } = false;
        [ObservableProperty] public partial bool IsButtonEnabled { get; set; } = true;
        [ObservableProperty] public partial string? TimerText { get; set; } = "Отправить код";
        private DispatcherTimer? Timer;
        private DateTime LastSentTime;
        private const int CooldownSeconds = 60;
        private bool CanSend() =>
            Email != string.Empty && IsButtonEnabled;


        [RelayCommand(CanExecute = nameof(CanSend))]
        private async Task SendCode()
        {
            try
            {
                IsButtonEnabled = false;
                LastSentTime = DateTime.Now;
                StartTimer();

                var res = await FetchAsync(
                    HttpMethod.Post, "mail/send",
                    new SendCodeRequest(Email!),
                    serialize: true
                );
                if (res.IsSuccessStatusCode)
                {
                    IsConfirmForm = true;
                }
                if (res.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    var responseBody = await res.Content.ReadAsStringAsync();
                    Errors = ErrorHandlerService.ParseValidationErrors(responseBody);
                    OnPropertyChanged(nameof(EmailError));
                    return;
                }
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
            }
            finally
            {
                SendCodeCommand.NotifyCanExecuteChanged();
            }
        }
        [RelayCommand]
        private async Task ConfirmCode()
        {
            try
            {
                (var res, var body) = await FetchAsync<ResetTokenResponse>(
                    HttpMethod.Post, "mail/verify",
                    new ConfirmCodeRequest(Email!, Code!),
                    serialize: true
                );

                if (res.IsSuccessStatusCode && body != null)
                {
                    ResetToken = body.ResetToken;
                    IsSendForm = false;
                    IsConfirmForm = false;
                    IsRestoreForm = true;
                }
                else
                {
                    Error = "Invalid or expired code";
                }
            }
            catch (HttpRequestException ex)
            {
                Error = ex.Message;
            }
        }
        private bool CanRestore() =>
            Email != string.Empty &&
            ResetToken != string.Empty &&
            NewPassword != string.Empty &&
            NewPasswordConfirmation != string.Empty;

        [RelayCommand(CanExecute = nameof(CanRestore))]
        private async Task Restore()
        {
            try
            {
                var res = await FetchAsync(
                    HttpMethod.Post, "mail/restore",
                    new RestoreFromMailRequest(Email!, ResetToken!, NewPassword!, NewPasswordConfirmation!, LogoutBool),
                    serialize: true
                );

                if (res.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    var responseBody = await res.Content.ReadAsStringAsync();
                    Errors = ErrorHandlerService.ParseValidationErrors(responseBody);
                    OnPropertyChanged(nameof(EmailError));
                    OnPropertyChanged(nameof(CodeError));
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
                    Email = string.Empty;
                    Code = string.Empty;
                    ResetToken = string.Empty;
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
        private void StartTimer()
        {
            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            Timer.Tick += (s, e) => UpdateTimer();
            Timer.Start();
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            var elapsed = DateTime.Now - LastSentTime;
            var remaining = CooldownSeconds - elapsed.TotalSeconds;

            if (remaining <= 0)
            {
                Timer?.Stop();
                IsButtonEnabled = true;
                TimerText = "Отправить код";
                SendCodeCommand.NotifyCanExecuteChanged();
                return;
            }

            TimerText = $"{Math.Ceiling(remaining)}";
        }
    }
}
