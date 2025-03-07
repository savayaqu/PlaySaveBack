using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Request;
using PSB.Models;
using PSB.Utils;
using PSB.Views.Settings.Account;
using static PSB.Utils.Fetch;
namespace PSB.ViewModels
{

    public partial class UpdateEmailViewModel: ObservableObject
    {
        //TODO: валидацию отображать
        //TODO: после обновления на AccountPage не меняется почта
        [ObservableProperty] public partial User? User { get; set; } = AuthData.User;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(UpdateEmailCommand))]
        public partial string? Email { get; set; } = "";
        private bool CanUpdate() => !string.IsNullOrEmpty(Email);

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
            }
        }
    }
}
