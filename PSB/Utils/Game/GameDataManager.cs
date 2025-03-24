using System;
using System.Diagnostics;
using PSB.Interfaces;
using PSB.Models;

namespace PSB.Utils.Game
{
    public static class GameDataManager
    {
        public static void SaveGame(IGame game)
        {
            try
            {
                BaseDataManager<IGame>.SaveData(game.Type, game.Id, game, "Game");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении игры: {ex.Message}");
            }
        }

        public static IGame? LoadGame(string type, ulong gameId)
        {
            try
            {
                return BaseDataManager<IGame>.LoadData(type, gameId, "Game");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке игры: {ex.Message}");
                return null;
            }
        }

        public static void RemoveGame(string type, ulong gameId)
        {
            try
            {
                BaseDataManager<IGame>.RemoveData(type, gameId, "Game");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при удалении игры: {ex.Message}");
            }
        }
    }
}
