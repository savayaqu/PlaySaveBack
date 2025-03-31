using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

namespace PSB.Services
{
    public static class NotificationService
    {
        private static InfoBar? _infoBar;
        private static Grid? _rootGrid;
        private static DispatcherQueueTimer? _autoHideTimer;

        public static void Initialize(InfoBar infoBar, Grid rootGrid)
        {
            _infoBar = infoBar;
            _rootGrid = rootGrid;

            // Инициализируем таймер
            _autoHideTimer = _infoBar.DispatcherQueue.CreateTimer();
            _autoHideTimer.Tick += (s, e) => HideNotification();
        }

        public static void ShowNotification(
            string message,
            string title = "",
            InfoBarSeverity severity = InfoBarSeverity.Informational,
            int? timeoutMs = null)
        {
            if (_infoBar == null) return;

            _infoBar.DispatcherQueue.TryEnqueue(() =>
            {
                // Останавливаем предыдущий таймер (если был)
                _autoHideTimer!.Stop();

                _infoBar.Title = title;
                _infoBar.Message = message;
                _infoBar.Severity = severity;
                _infoBar.IsOpen = true;

                // Устанавливаем таймаут только для Informational и Success
                if (timeoutMs.HasValue ||
                    severity == InfoBarSeverity.Informational ||
                    severity == InfoBarSeverity.Success)
                {
                    _autoHideTimer.Interval = TimeSpan.FromMilliseconds(timeoutMs ?? 5000);
                    _autoHideTimer.Start();
                }
            });
        }

        public static void HideNotification()
        {
            if (_infoBar == null) return;

            _infoBar.DispatcherQueue.TryEnqueue(() =>
            {
                _autoHideTimer!.Stop();
                _infoBar.IsOpen = false;
            });
        }

        // Удобные методы-обёртки для разных типов уведомлений
        public static void ShowSuccess(string message, string title = "Успех")
            => ShowNotification(message, title, InfoBarSeverity.Success);

        public static void ShowError(string message, string title = "Ошибка")
            => ShowNotification(message, title, InfoBarSeverity.Error);

        public static void ShowWarning(string message, string title = "Внимание")
            => ShowNotification(message, title, InfoBarSeverity.Warning);

        public static void ShowInfo(string message, string title = "Информация")
            => ShowNotification(message, title, InfoBarSeverity.Informational);
    }
}
