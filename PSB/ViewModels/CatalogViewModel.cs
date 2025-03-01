using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Api.Response;
using PSB.Models;
using PSB.Utils;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels
{
    public partial class CatalogViewModel : ObservableObject
    {
        
        public CatalogViewModel() 
        {
            _ = LoadGamesAsync();
        }
        [ObservableProperty] public partial ObservableCollection<Game> Games { get; set; } = new();


        [RelayCommand]
        public async Task LoadGamesAsync()
        {
            (var res, var body) = await FetchAsync<PaginatedResponse<Game>>(
                HttpMethod.Get, "games",
                setError: e => Debug.WriteLine($"Error: {e}")
            );
            if (!res.IsSuccessStatusCode || body == null)
                return;

            // Логируем JSON-ответ
            string bodyJson = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            //Debug.WriteLine(bodyJson);

            // Очистка коллекции и добавление новых элементов
            Games.Clear();
            foreach (var item in body.Data) // Теперь берем body.Data, а не body напрямую
            {
                Games.Add(item);
            }
        }
    }
}
