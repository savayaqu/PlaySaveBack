using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using User = PSB.Models.User;
using static PSB.Utils.Fetch;
using System.Net.Http;
using Microsoft.UI.Xaml.Controls;

namespace PSB.Utils
{
    public static class AuthData
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        public static async Task SaveAndNavigate(string? token, User? user)
        {
            Token = token;
            User = user;
            await App.AuthService.UpdateAuthNavAsync();
        }
        public static async Task ExitAndNavigate(Action<bool>? setIsFetch = null)
        {
            await FetchAsync(HttpMethod.Get, "logout", setIsFetch);
            Token = null;
            User = null;
            await App.AuthService.UpdateAuthNavAsync();

        }
        private static string? _token = null;
        public static string? Token
        {
            get => _token ??= LocalSettings.Values["token"] as string;
            set
            {
                _token = value;

                if (value == null)
                    LocalSettings.Values["token"] = null;
                else
                    LocalSettings.Values["token"] = value;
            }
        }

        private static User? _user = null;
        public static User? User
        {
            get
            {
                if (_user == null)
                {
                    var setting = LocalSettings.Values["user"];
                    if (setting != null)
                        _user = JsonSerializer.Deserialize<User>((string)setting);
                }
                Debug.WriteLine("user" + _user);

                return _user;
            }
            set
            {
                _user = value;
                Debug.WriteLine("user" + _user);
                if (value == null)
                    LocalSettings.Values["user"] = null;
                else
                    LocalSettings.Values["user"] = JsonSerializer.Serialize(value);
            }
        }
    }
}