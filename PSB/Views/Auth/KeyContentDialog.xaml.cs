using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PSB.ViewModels.Auth;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PSB.Views.Auth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class KeyContentDialog : ContentDialog
    {
        public KeyContentDialog(int key)
        {
            this.InitializeComponent();
            TextBlockKey.Text = key.ToString();
            IsPrimaryButtonEnabled = false;
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            IsPrimaryButtonEnabled = CheckBox.IsChecked == true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            IsPrimaryButtonEnabled = false;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
