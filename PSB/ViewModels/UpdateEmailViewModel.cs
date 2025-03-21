using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using PSB.Api.Request;
using PSB.Api.Response;
using PSB.Models;
using PSB.Utils;
using PSB.Views.Settings.Account;
using static PSB.Utils.Fetch;
namespace PSB.ViewModels
{

    public partial class UpdateEmailViewModel: ObservableObject
    {
        //TODO: валидацию отображать
        
        [ObservableProperty] public partial User? User { get; set; } = AuthData.User;
        [ObservableProperty] public partial ContentDialog? ContentDialog { get; set; }
        [ObservableProperty] public partial string? ErrorEmail { get; set; } = null;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdateEmailCommand))]
        public partial string? Email { get; set; } = "";
        private bool CanUpdate() => !string.IsNullOrEmpty(Email);
        public UpdateEmailViewModel(ContentDialog contentDialog)
        {
            ContentDialog = contentDialog;
        }

        [RelayCommand(CanExecute = nameof(CanUpdate))]
        public async Task UpdateEmail()
        {
            (var res, var body) = await FetchAsync<User>(
                HttpMethod.Post, "profile",
                body: new UpdateAccountRequest.UpdateEmailRequest(Email!),
                serialize: true);

            if (res.IsSuccessStatusCode)
            {
                AuthData.User = body;
                App.MainWindow.ProfileViewModel.User = body;
            }
            else if (res.StatusCode == HttpStatusCode.UnprocessableContent)
            {
                // Получаем содержимое ответа как строку
                var errorContent = await res.Content.ReadAsStringAsync();

                // Десериализуем JSON-строку в объект ErrorResponse
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent);

                // Устанавливаем ошибку
                ErrorEmail = errorResponse.Errors["email"][0];
                return;
            }

            ContentDialog.Hide();
        }
    }
}
