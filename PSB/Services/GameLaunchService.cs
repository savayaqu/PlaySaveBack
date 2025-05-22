using PSB.Interfaces;
using PSB.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PSB.Services
{
    public class GameLaunchService : IGameLaunchService
    {
        private Dictionary<string, bool> _gameStatuses = new Dictionary<string, bool>();
        private Dictionary<string, Process> _runningProcesses = new Dictionary<string, Process>();
        public event EventHandler GameStatusChanged;

        public bool AnyGameRunning => _gameStatuses.Values.Contains(true);  // Реализация переименованного свойства

        private string GetGameKey(IGame game) => $"{game.Type}_{game.Id}";

        public bool IsGameRunning(IGame game)  // Реализация переименованного метода
        {
            if (game == null) return false;
            return _gameStatuses.TryGetValue(GetGameKey(game), out var status) && status;
        }

        // И вызывать его при изменении статуса игры
        private void OnGameStatusChanged()
        {
            GameStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<GameSessionResult> LaunchGameAsync(string filePath, IGame game)
        {
            var result = new GameSessionResult { Launched = false };

            if (game == null || IsGameRunning(game) || !File.Exists(filePath))
                return result;

            var gameKey = GetGameKey(game);
            _gameStatuses[gameKey] = true;
            OnGameStatusChanged();
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
                gameProcess.Exited += (s, e) =>
                {
                    _gameStatuses[gameKey] = false;
                    OnGameStatusChanged();
                    _runningProcesses.Remove(gameKey);
                    tcs.TrySetResult(true);
                };

                gameProcess.Start();
                _runningProcesses[gameKey] = gameProcess;

                await tcs.Task;

                result.EndTime = DateTime.Now;
                result.Launched = true;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                _gameStatuses[gameKey] = false;
                OnGameStatusChanged();
                _runningProcesses.Remove(gameKey);
                Debug.WriteLine("Ошибка запуска: " + ex.Message);
                NotificationService.ShowError("Ошибка запуска: " + ex.Message);
            }

            return result;
        }

        public void CloseGame(IGame game)
        {
            if (game == null) return;

            var gameKey = GetGameKey(game);
            if (_gameStatuses.TryGetValue(gameKey, out var status) && status)
            {
                if (_runningProcesses.TryGetValue(gameKey, out var process))
                {
                    try
                    {
                        if (!process.HasExited)
                            process.Kill();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при закрытии игры: {ex.Message}");
                    }
                    finally
                    {
                        _gameStatuses[gameKey] = false;
                        OnGameStatusChanged();
                        _runningProcesses.Remove(gameKey);
                        process?.Dispose();
                    }
                }
                else
                {
                    _gameStatuses[gameKey] = false;
                    OnGameStatusChanged();
                }
            }
        }
    }
}