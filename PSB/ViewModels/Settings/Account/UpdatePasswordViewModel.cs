using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Request;
using PSB.Api.Response;
using PSB.Models;
using PSB.Services;
using PSB.Utils;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using static PSB.Utils.Fetch;
namespace PSB.ViewModels
{
    public partial class UpdatePasswordViewModel : ObservableObject
    {
        //TODO: валидацию отображать
        [ObservableProperty] public partial User? User { get; set; } = AuthData.User;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdatePasswordCommand))]
        public partial string? CurrentPassword { get; set; } = "";
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdatePasswordCommand))]
        public partial string? NewPassword { get; set; } = "";
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdatePasswordCommand))]
        public partial string? NewPasswordConfirmation { get; set; } = "";
        [ObservableProperty] public partial string? ErrorNewPass { get; set; } = null;
        [ObservableProperty] public partial string? ErrorNewPassConf { get; set; } = null;
        [ObservableProperty] public partial string? Error { get; set; } = null;

        private bool CanUpdate() => CurrentPassword != "" && NewPassword != "" && NewPasswordConfirmation != "";

        [RelayCommand(CanExecute = nameof(CanUpdate))]
        public async Task UpdatePassword()
        {
            var res = await FetchAsync(
                HttpMethod.Post, "profile",
                body: new UpdateAccountRequest.UpdatePasswordRequest(CurrentPassword!, NewPassword!, NewPasswordConfirmation!),
                serialize: true);
            if (res.IsSuccessStatusCode)
            {
                App.DialogService!.HideDialog();
                NotificationService.ShowSuccess("Пароль обновлён");
            }
            else if (res.StatusCode == HttpStatusCode.UnprocessableContent)
            {
                // Получаем содержимое ответа как строку
                var errorContent = await res.Content.ReadAsStringAsync();

                // Десериализуем JSON-строку в объект ErrorResponse
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                Debug.WriteLine(errorResponse);
                Debug.WriteLine(errorResponse.ToString());
                // Устанавливаем ошибку
                ErrorNewPass = errorResponse.Errors["new_password"][0];
                ErrorNewPassConf = errorResponse.Errors["new_password_confirmation"][0];
                return;
            }
            else if (res.StatusCode == HttpStatusCode.Unauthorized)
            {

                Error = "Invalid current password";
                return;
            }
        }
    }
}
