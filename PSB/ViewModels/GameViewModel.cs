using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PSB.Models;

namespace PSB.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        public GameViewModel(ulong gameId) 
        {
            Debug.WriteLine("Игра" +  gameId);
        }

    }
}
