using PSB.Interfaces;
using PSB.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSB.Services
{
    public class GameLaunchService : IGameLaunchService
    {
        private bool _isGameLaunched;
        public bool IsGameLaunched => _isGameLaunched;

        public async Task<GameSessionResult> LaunchGameAsync(string filePath)
        {
            var result = new GameSessionResult { Launched = false };

            if (_isGameLaunched || !File.Exists(filePath))
                return result;

            _isGameLaunched = true;
            result.StartTime = DateTime.Now;

            try
            {
                using Process gameProcess = new();
                ProcessStartInfo startInfo = new()
                {
                    FileName = filePath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(filePath),
                    CreateNoWindow = false
                };

                gameProcess.StartInfo = startInfo;
                gameProcess.EnableRaisingEvents = true;

                var tcs = new TaskCompletionSource<bool>();
                gameProcess.Exited += (s, e) => tcs.TrySetResult(true);

                gameProcess.Start();
                await tcs.Task;

                result.EndTime = DateTime.Now;
                result.Launched = true;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Debug.WriteLine("Ошибка запуска: " + ex.Message);
                NotificationService.ShowError("Ошибка запуска: " + ex.Message);
            }
            finally
            {
                _isGameLaunched = false;
            }

            return result;
        }

    }

}
