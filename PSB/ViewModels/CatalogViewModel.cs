using System;
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
        [ObservableProperty] public partial int CurrentPage { get; set; } = 1;
        [ObservableProperty] public partial int TotalPages { get; set; } = 1;
        [ObservableProperty] public partial string PageInput { get; set; }


        [RelayCommand]
        public async Task LoadGamesAsync(int page = 1)
        {
            (var res, var body) = await FetchAsync<PaginatedResponse<Game>>(
                HttpMethod.Get, $"games?page={page}",
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
            Games.Clear();
            foreach (var item in body.Data) // Теперь берем body.Data, а не body напрямую
            {
                Games.Add(item);
            }
            // Обновляем информацию о пагинации
            CurrentPage = body.Meta.CurrentPage;
            TotalPages = body.Meta.LastPage;
        }
        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                await LoadGamesAsync(CurrentPage - 1);
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                await LoadGamesAsync(CurrentPage + 1);
            }
        }
        [RelayCommand]
        private async Task JumpToPageAsync()
        {
            if (int.TryParse(PageInput, out int pageNumber))
            {
                // Проверяем, что номер страницы в допустимых пределах
                if (pageNumber >= 1 && pageNumber <= TotalPages)
                {
                    await LoadGamesAsync(pageNumber);
                }
                else
                {
                    // Если номер страницы некорректен, переходим на следующую страницу
                    await LoadGamesAsync(CurrentPage + 1);
                }
            }
            else
            {
                // Если ввод некорректен, очищаем текстовое поле
                PageInput = string.Empty;
            }
        }
    }
}
