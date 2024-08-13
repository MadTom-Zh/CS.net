using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MLW_Succubus_Storys.Classes
{
    internal class DoubleClickInterval
    {
        public static uint ValueMs
        {
            get
            {
                if (_MouseDoubleClickTimeNotInited)
                {
                    _MouseDoubleClickTime = GetDoubleClickTime();
                    _MouseDoubleClickTimeNotInited = false;
                }
                return _MouseDoubleClickTime;
            }
        }
        private static bool _MouseDoubleClickTimeNotInited = true;
        private static uint _MouseDoubleClickTime = 100;
        [DllImport("user32.dll")]
        private static extern uint GetDoubleClickTime();
    }
}
