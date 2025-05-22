using PSB.Models;
using System;
using System.Threading.Tasks;

namespace PSB.Interfaces
{
    public interface IGameLaunchService
    {
        bool AnyGameRunning { get; }
        bool IsGameRunning(IGame game);
        Task<GameSessionResult> LaunchGameAsync(string filePath, IGame game);
        void CloseGame(IGame game);

        // Добавляем событие в интерфейс
        event EventHandler GameStatusChanged;
    }
}
