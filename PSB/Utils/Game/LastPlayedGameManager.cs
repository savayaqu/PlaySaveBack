using PSB.Interfaces;
using PSB.Models;
using System.Text.Json;
using Windows.Storage;

namespace PSB.Utils.Game
{
    public static class LastPlayedGameManager
    {
        private static readonly string CacheKey = $"{AuthData.User!.Id}_LastPlayedGame";


        public static void SaveLastPlayedGame(IGame game, Library library)
        {
            var model = new LastPlayedGame
            {
                Game = game,
                Library = library
            };
            var json = JsonSerializer.Serialize(model);
            ApplicationData.Current.LocalSettings.Values[CacheKey] = json;
        }
        public static LastPlayedGame LoadLastPlayedGame()
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(CacheKey, out object value))
            {
                return JsonSerializer.Deserialize<LastPlayedGame>(value.ToString());
            }
            return null;
        }

    }
}
