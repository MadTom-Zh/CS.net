using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.App.Ctrls
{
    internal class Interfaces
    {
        public interface SelectableUIItem
        {
            public bool IsSelectable { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
