using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Interfaces;
using PSB.Models;
using PSB.Utils.Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace PSB.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<GroupedSaves> LocalSaves { get; set; } = [];

        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        public HomeViewModel()
        {
            LoadLocalSaves();
        }
        [RelayCommand]
        public void LoadLocalSaves()
        {
            IsLoading = true;
            LocalSaves.Clear();

            try
            {
                var grouped = SavesDataManager<IGame>.GetAllLocalSaves();

                foreach (var group in grouped)
                {
                    var gameName = group.Key; // можно извлечь ID и преобразовать в читаемое имя, если нужно
                    var unsyncedSaves = group.Value;

                    if (unsyncedSaves.Any())
                    {
                        LocalSaves.Add(new GroupedSaves
                        {
                            GameName = gameName,
                            Saves = unsyncedSaves
                        });
                    }
                }

                Debug.WriteLine($"Успешно загружено {LocalSaves.Count} групп");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке сохранений: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }


    }
}
