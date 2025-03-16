using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PSB.Services
{
    public class DialogService
    {
        private XamlRoot _xamlRoot;
        private ContentDialog? _currentDialog; // Храним текущий диалог

        public XamlRoot XamlRoot => _xamlRoot;

        public void SetXamlRoot(XamlRoot xamlRoot)
        {
            if (xamlRoot == null)
            {
                Debug.WriteLine("Попытка установить XamlRoot, но он null!");
                return;
            }

            _xamlRoot = xamlRoot;
            Debug.WriteLine($"XamlRoot успешно установлен");
        }

        public async Task ShowDialogAsync(ContentDialog dialog)
        {
            if (_xamlRoot == null)
            {
                Debug.WriteLine("Ошибка: XamlRoot не установлен!");
                throw new InvalidOperationException("XamlRoot is not set. Call SetXamlRoot first.");
            }
            dialog.XamlRoot = _xamlRoot;
            _currentDialog = dialog; // Сохраняем ссылку на диалог
            Debug.WriteLine("Открываем диалог...");
            await dialog.ShowAsync();
            Debug.WriteLine("Диалог закрыт пользователем");
            _currentDialog = null; // Обнуляем после закрытия
        }

        public void HideDialog()
        {
            if (_currentDialog != null)
            {
                Debug.WriteLine("Закрываем диалог...");
                _currentDialog.Hide();
                _currentDialog = null;
            }
            else
            {
                Debug.WriteLine("Ошибка: Нет активного диалога для закрытия");
            }
        }
    }

}
