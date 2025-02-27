using System;
using System.Diagnostics;
using System.Drawing;

namespace PSB.Helpers
{
    public static class IconFromExe
    {
        public static Icon? GetExeIcon(string exePath)
        {
            try
            {
                return Icon.ExtractAssociatedIcon(exePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при извлечении иконки: {ex.Message}");
                return null;
            }
        }
    }
}
