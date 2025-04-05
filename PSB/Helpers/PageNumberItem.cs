using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSB.Helpers
{
    public class PageNumberItem
    {
        public int Number { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsEllipsis { get; set; }
    }
}
