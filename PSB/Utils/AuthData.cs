using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PSB.Api.Response;
using PSB.Models;
using PSB.Views.Auth;
using Windows.Storage;
using static PSB.Utils.Fetch;
using User = PSB.Models.User;

namespace PSB.Utils
{
    public static class AuthData
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        // Токен
        private static string? _token = null;
        public static string? Token
        {
            get => _token ??= LocalSettings.Values["token"] as string;
            set
            {
                _token = value;
                LocalSettings.Values["token"] = value;
            }
        }

        // Профиль пользователя
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
                return _user;
            }
            set
            {
                _user = value;
                LocalSettings.Values["user"] = value == null ? null : JsonSerializer.Serialize(value);
            }
        }

        // Библиотека пользователя
        private static ObservableCollection<Library>? _libraries = null;
        public static ObservableCollection<Library> Libraries
        {
            get
            {
                if (_libraries == null)
                {
                    var setting = LocalSettings.Values["libraries"];
                    if (setting != null)
                        _libraries = JsonSerializer.Deserialize<ObservableCollection<Library>>((string)setting);
                    else
                        _libraries = new ObservableCollection<Library>();
                }
                return _libraries!;
            }
            set
            {
                _libraries = value;
                LocalSettings.Values["libraries"] = value == null ? null : JsonSerializer.Serialize(value);
            }
        }

        // Выход и очистка данных
        public static async Task ExitAndNavigate()
        {
            await FetchAsync(HttpMethod.Get, "logout");
            Token = null;
            User = null;
            Libraries.Clear();
            App.SwitchToLoginFromMain();
            ApplicationData.Current.LocalSettings.Values.Clear();
        }

        // Загрузка профиля
        public static async Task LoadProfileAsync()
        {
            (var res, var body) = await FetchAsync<User>(HttpMethod.Get, "profile");

            if (res.IsSuccessStatusCode && body != null)
            {
                User = body;
            }
        }

        // Загрузка библиотеки
        public static async Task LoadLibraryAsync()
        {
            (var res, var body) = await FetchAsync<PaginatedResponse<Library>>(HttpMethod.Get, "library?limit=0");

            if (res.IsSuccessStatusCode && body != null)
            {
                Libraries.Clear();
                foreach (var item in body.Data)
                {
                    Libraries.Add(item);
                }
            }
        }

        // Сохранение токена, пользователя, закрытие окна авторизации и инициализация основного
        public static void SaveAndOpenMainWindow(string? token, User? user)
        {
            Token = token;
            User = user;
            App.SwitchToMain();
        }
    }
}