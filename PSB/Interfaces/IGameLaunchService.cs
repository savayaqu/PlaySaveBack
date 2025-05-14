using PSB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSB.Interfaces
{
    public interface IGameLaunchService
    {
        bool IsGameLaunched { get; }
        Task<GameSessionResult> LaunchGameAsync(string filePath);
    }
}
