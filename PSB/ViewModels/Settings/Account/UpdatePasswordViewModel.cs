using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Request;
using PSB.Models;
using PSB.Services;
using PSB.Utils;
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
        }
    }
}
