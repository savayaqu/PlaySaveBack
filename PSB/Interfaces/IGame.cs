using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSB.Interfaces
{
    public interface IGame
    {
        ulong Id { get; set; }
        string Name { get; set; }
        string Type { get; }
    }
}
