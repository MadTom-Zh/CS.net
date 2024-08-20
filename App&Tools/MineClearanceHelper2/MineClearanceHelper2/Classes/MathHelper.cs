using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.App.Classes
{
    internal class MathHelper
    {
        public static int Round(double source, int baseMultiple)
        {
            if (source == 0) return 0;

            int div = (int)((baseMultiple / 2 + source) / baseMultiple);
            return baseMultiple * div;
        }
        public static double Round(double source, double baseMultiple)
        {
            if (source == 0) return 0;

            int div = (int)((baseMultiple / 2 + source) / baseMultiple);
            return baseMultiple * div;
        }
    }
}
