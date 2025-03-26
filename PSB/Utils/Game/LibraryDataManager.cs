using PSB.Interfaces;
using PSB.Models;

namespace PSB.Utils.Game
{
    public static class LibraryDataManager<T> where T : IGame
    {
        public static void SaveLibrary(T game, Library library) => BaseDataManager<Library>.SaveData(game.Type, game.Id, library, "Library");
        public static Library? LoadLibrary(string type, ulong gameId) => BaseDataManager<Library>.LoadData(type, gameId, "Library");
        public static void RemoveLibrary(string type, ulong gameId) => BaseDataManager<Library>.RemoveData(type, gameId, "Library");
    }
}