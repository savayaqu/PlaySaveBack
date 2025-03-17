using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using PSB.Interfaces;
using PSB.Models;
using Windows.Storage;

namespace PSB.Utils
{
    public static class SavesDataManager<T> where T : IGame
    {
        public static void SaveSaves(T game, List<Save> saves) => BaseDataManager<List<Save>>.SaveData(game.Type, game.Id, saves, "Saves");
        public static List<Save>? LoadSaves(string type, ulong gameId) => BaseDataManager<List<Save>>.LoadData(type, gameId, "Saves");
        public static void RemoveSaves(string type, ulong gameId) => BaseDataManager<List<Save>>.RemoveData(type, gameId, "Saves");
    }
}