using PSB.Models;
using System.Threading.Tasks;

namespace PSB.Interfaces
{
    public interface IGameLaunchService
    {
        bool IsGameLaunched { get; }
        Task<GameSessionResult> LaunchGameAsync(string filePath);
    }
}
