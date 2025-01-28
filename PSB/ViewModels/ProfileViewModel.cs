using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PSB.Utils;

namespace PSB.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        [RelayCommand]
        private void Logout()
        {
            _ = AuthData.ExitAndNavigate();
        }
    }
}
