using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Response;
using Microsoft.UI.Xaml.Media.Imaging;
using PSB.Models;
using PSB.Utils;
using static PSB.Utils.Fetch;
using System;


namespace PSB.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        [ObservableProperty] public partial ObservableCollection<LibraryResponse> Library { get; set; } = new();
        [ObservableProperty] public partial User? User { get; set; } = AuthData.User;
        public ProfileViewModel()
        {
            _ = LoadProfileAsync();
        }
        [RelayCommand]
        public async Task LoadProfileAsync()
        {
            (var res, var body) = await FetchAsync<User>(
                HttpMethod.Get, "profile",
                setError: e => Debug.WriteLine($"Ошибка при получении профиля: {e}")
            );

            if (res.IsSuccessStatusCode && body != null)
            {
                User = body;
                AuthData.User = User;
            }
        }

        [RelayCommand]
        private void Logout()
        {
            _ = AuthData.ExitAndNavigate();
        }
        [RelayCommand]
        public async Task LoadLibraryAsync()
        {
            (var res, var body) = await FetchAsync<PaginatedResponse<LibraryResponse>>(
                HttpMethod.Get, "library",
                setError: e => Debug.WriteLine($"Error: {e}")
            );

            if (!res.IsSuccessStatusCode || body == null)
                return;

            // Логируем JSON-ответ
            string bodyJson = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            Debug.WriteLine(bodyJson);

            // Очистка коллекции и добавление новых элементов
            Library.Clear();
            foreach (var item in body.Data) // Теперь берем body.Data, а не body напрямую
            {
                Library.Add(item);
            }
        }

    }
}
