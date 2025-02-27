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

        public XamlRoot XamlRoot => _xamlRoot;

        public void SetXamlRoot(XamlRoot xamlRoot)
        {
            if (xamlRoot == null)
            {
                Debug.WriteLine("Попытка установить XamlRoot, но он null!");
                return;
            }

            _xamlRoot = xamlRoot;
            Debug.WriteLine($"XamlRoot успешно установлен: {_xamlRoot != null}");
        }


        public async Task ShowDialogAsync(ContentDialog dialog)
        {
            if (_xamlRoot == null)
            {
                throw new InvalidOperationException("XamlRoot is not set. Call SetXamlRoot first.");
            }

            dialog.XamlRoot = _xamlRoot;
            await dialog.ShowAsync();
        }
    }
}
