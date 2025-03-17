using System;
using System.Diagnostics;
using System.Text.Json;
using PSB.Interfaces;
using PSB.Models;
using Windows.Storage;

namespace PSB.Utils
{
    public static class GameDataManager
    {
        public static void SaveGame(IGame game) => BaseDataManager<IGame>.SaveData(game.Type, game.Id, game, "Game");
        public static IGame? LoadGame(string type, ulong gameId) => BaseDataManager<IGame>.LoadData(type, gameId, "Game");
        public static void RemoveGame(string type, ulong gameId) => BaseDataManager<IGame>.RemoveData(type, gameId, "Game");
    }
}