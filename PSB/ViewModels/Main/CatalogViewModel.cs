using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using PSB.Api.Response;
using PSB.Models;
using PSB.Views;
using static PSB.Utils.Fetch;


namespace PSB.ViewModels
{
    public partial class CatalogViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<Game> Games { get; set; } = new();

        [ObservableProperty]
        public partial int? CurrentPage { get; set; } = 1;
        public bool IsCurrentPage(int? page) => page == CurrentPage;

        [ObservableProperty]
        public partial int? TotalPages { get; set; } = 1;

        [ObservableProperty]
        public partial int? Total { get; set; }

        [ObservableProperty]
        public partial string? Name { get; set; }

        private bool _isLoading;
        private CancellationTokenSource? _searchTokenSource;

        [ObservableProperty]
        public partial ObservableCollection<int?> PageNumbers { get; set; } = new();

        [RelayCommand]
        private void NavigateToPage(int? page)
        {
            Debug.WriteLine("нажата");
            if (page != null && page != CurrentPage)
            {
                CurrentPage = page;
                LoadGamesAsync(page);
            }
        }

        public void UpdatePageNumbers()
        {
            var pages = new List<int?>();
            int current = CurrentPage ?? 1;
            int total = TotalPages ?? 1;

            // Всегда добавляем первую страницу
            pages.Add(1);

            // Добавляем многоточие, если текущая страница далеко от начала
            if (current > 3)
            {
                pages.Add(null); // null будет обозначать многоточие
            }

            // Добавляем страницы вокруг текущей
            int start = Math.Max(2, current - 1);
            int end = Math.Min(total - 1, current + 1);

            for (int i = start; i <= end; i++)
            {
                if (i > 1 && i < total)
                {
                    pages.Add(i);
                }
            }

            // Добавляем многоточие, если текущая страница далеко от конца
            if (current < total - 2)
            {
                pages.Add(null);
            }

            // Всегда добавляем последнюю страницу, если она не первая
            if (total > 1)
            {
                pages.Add(total);
            }

            // Обновляем коллекцию для UI
            PageNumbers.Clear();
            foreach (var page in pages)
            {
                PageNumbers.Add(page);
            }
        }

        [RelayCommand]
        public async Task LoadGamesAsync(int? page = null)
        {
            if (_isLoading) return;

            _isLoading = true;
            try
            {
                var actualPage = page ?? CurrentPage;
                var query = $"games?page={actualPage}";

                if (!string.IsNullOrEmpty(Name))
                {
                    query += $"&name={WebUtility.UrlEncode(Name)}";
                }

                var (response, body) = await FetchAsync<PaginatedResponse<Game>>(HttpMethod.Get, query);

                if (response.IsSuccessStatusCode && body != null)
                {
                    // Создаем копию данных для UI потока
                    var gamesToAdd = body.Data.ToList();
                    var currentPage = body.Meta.CurrentPage;
                    var totalPages = body.Meta.LastPage;
                    var total = body.Meta.Total;
                    // Обновляем UI через Dispatcher
                    await UpdateUIAsync(() =>
                    {
                        Games.Clear();
                        foreach (var item in gamesToAdd)
                        {
                            Games.Add(item);
                        }

                        CurrentPage = currentPage;
                        TotalPages = totalPages;
                        Total = total;
                    });
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task UpdateUIAsync(Action updateAction)
        {
            try
            {
                // Получаем DispatcherQueue для UI потока
                var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()
                    ?? App.MainWindow?.DispatcherQueue;

                if (dispatcherQueue == null)
                {
                    Debug.WriteLine("DispatcherQueue не доступен");
                    return;
                }

                // Если мы уже в UI потоке
                if (dispatcherQueue.HasThreadAccess)
                {
                    updateAction();
                    UpdatePageNumbers(); // Обновляем номера страниц после загрузки данных

                    return;
                }

                // Создаем TaskCompletionSource для ожидания завершения
                var tcs = new TaskCompletionSource<bool>();

                // Отправляем действие в UI поток
                bool enqueued = dispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        updateAction();
                        UpdatePageNumbers(); // Обновляем номера страниц после загрузки данных
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });

                if (!enqueued)
                {
                    tcs.SetException(new InvalidOperationException("Не удалось отправить задание в DispatcherQueue"));
                    return;
                }

                await tcs.Task; // Ожидаем завершения обновления UI
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления UI: {ex.Message}");
                // Здесь можно добавить дополнительную обработку ошибок
            }
        }

        [RelayCommand]
        private async Task SearchGamesAsync()
        {
            var currentSearch = Name;
            _searchTokenSource?.Cancel();
            _searchTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _searchTokenSource.Token);

                if (!_searchTokenSource.IsCancellationRequested && currentSearch == Name)
                {
                    CurrentPage = 1;
                    await LoadGamesAsync(1);
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("Search was canceled");
            }
        }
        
        private bool CanGoToPreviousPage() => CurrentPage > 1;
        private bool CanGoToNextPage() => CurrentPage < TotalPages;
        partial void OnCurrentPageChanged(int? value)
        {
            PreviousPageCommand.NotifyCanExecuteChanged();
            NextPageCommand.NotifyCanExecuteChanged();
        }

        partial void OnTotalPagesChanged(int? value)
        {
            PreviousPageCommand.NotifyCanExecuteChanged();
            NextPageCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
        private async Task PreviousPageAsync()
        {
            await LoadGamesAsync(CurrentPage - 1);
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        private async Task NextPageAsync()
        {
            await LoadGamesAsync(CurrentPage + 1);
        }

        private string _lastProcessedName = string.Empty;
        private DateTime _lastNameChangeTime = DateTime.MinValue;

        partial void OnNameChanged(string value)
        {
            _lastNameChangeTime = DateTime.Now;

            if (App.NavigationService!.GetCurrentPage() is CatalogPage)
            {
                HandleSearch();
            }
            else
            {
                App.NavigationService!.Navigate("CatalogPage");
            }
        }

        private async void HandleSearch()
        {
            try
            {
                // Запоминаем текущее значение
                var currentName = Name;
                var changeTime = _lastNameChangeTime;

                // Для пустого значения - немедленная загрузка
                if (string.IsNullOrEmpty(currentName))
                {
                    _searchTokenSource?.Cancel();
                    _lastProcessedName = currentName;
                    CurrentPage = 1;
                    await LoadGamesAsync(1);
                    return;
                }

                // Для непустого значения - задержка 500 мс
                await Task.Delay(500);

                // Проверяем, что значение не изменилось за время задержки
                if (currentName == Name && changeTime == _lastNameChangeTime)
                {
                    _searchTokenSource?.Cancel();
                    _searchTokenSource = new CancellationTokenSource();

                    await Task.Delay(300, _searchTokenSource.Token); // Дополнительная небольшая задержка

                    if (!_searchTokenSource.IsCancellationRequested)
                    {
                        _lastProcessedName = currentName;
                        CurrentPage = 1;
                        await LoadGamesAsync(1);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Игнорируем отмену
            }
        }
    }
}