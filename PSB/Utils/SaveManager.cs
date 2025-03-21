using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PSB.Models;

namespace PSB.Utils
{
    public partial class SaveManager : ObservableObject
    {
        private ObservableCollection<Save> _saves = new ObservableCollection<Save>();
        public ObservableCollection<Save> Saves
        {
            get => _saves;
            set => SetProperty(ref _saves, value);
        }

        public int UnsyncedSavesCount => Saves.Count(save => !save.IsSynced);
        [ObservableProperty] public partial int Test { get; set; } = 20;

        public SaveManager()
        {
            Saves.CollectionChanged += (s, e) => OnPropertyChanged(nameof(UnsyncedSavesCount));
        }

        public void AddSave(Save save)
        {
            Saves.Add(save);
        }

        public void MarkAsSynced(Save save)
        {
            save.IsSynced = true;
            OnPropertyChanged(nameof(UnsyncedSavesCount));
        }
    }
}
