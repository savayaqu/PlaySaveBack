using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api;
using PSB.Api.Response;
using PSB.Models;
using PSB.Utils;
using static PSB.Utils.Fetch;


namespace PSB.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        [ObservableProperty] public partial ObservableCollection<CollectionResponse> Collections { get; set; } = new();
        [ObservableProperty] public partial User? User { get; set; } = AuthData.User;
        [RelayCommand]
        private void Logout()
        {
            _ = AuthData.ExitAndNavigate();
        }
        [RelayCommand]
        public async Task LoadCollectionsAsync()
        {
            (var res, var body) = await FetchAsync<List<CollectionResponse>>(
                HttpMethod.Get, "collections",
                setError: e => Debug.WriteLine($"Error: {e}")
            );
            if(!res.IsSuccessStatusCode)
                return;
            // Сериализуем объект body в строку JSON для вывода в консоль
            string bodyJson = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                WriteIndented = true // Удобное форматирование
            });
            // Очистка коллекции и добавление новых элементов
            Collections.Clear();
            if (body != null)
            {
                foreach (var item in body)
                {
                    Collections.Add(item);
                }
            }
        }
    }
}
