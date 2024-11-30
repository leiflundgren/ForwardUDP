using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForwardUDP.Components
{
    public static class Extensions
    {
        public static T StaticCast<T>(this T o)
            where T : class
        { 
            return o; 
        }

    }
}
