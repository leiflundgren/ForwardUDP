using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
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

        /// <summary>
        /// Parse commnad-line, get argument-to switch. Empty string if no arg. Null if not specified
        /// </summary>
        public static string? GetCommandLineArg(this string[] args, string argname)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                string s = args[i];
                if (s[0] == '-' && s.Contains(argname))
                {
                    int eq = s.IndexOf('=');
                    if (eq > 0)
                        return s.Substring(eq + 1);
                    else if (i + 1 < args.Length && args[i + 1][0] != '-')
                        return args[i + 1];
                    else
                        return string.Empty;
                }
            }

            return null;
        }

        public static List<T> NonNull<T>(this List<T?> list) where T : class 
        {
            List<T> res = new List<T>(list.Count);
            foreach (T? t in list) 
                if ( t is not null) 
                    res.Add(t);
            return res;
        }
    }

    public static class IPEndPointExtensions
    {
        //public static bool TryParse(string s, out IPEndPoint result)
        //{
        //    int addressLength = s.Length;  // If there's no port then send the entire string to the address parser
        //    int lastColonPos = s.LastIndexOf(':');

        //    // Look to see if this is an IPv6 address with a port.
        //    if (lastColonPos > 0)
        //    {
        //        if (s[lastColonPos - 1] == ']')
        //        {
        //            addressLength = lastColonPos;
        //        }
        //        // Look to see if this is IPv4 with a port (IPv6 will have another colon)
        //        else if (s.Substring(0, lastColonPos).LastIndexOf(':') == -1)
        //        {
        //            addressLength = lastColonPos;
        //        }
        //    }

        //    if (IPAddress.TryParse(s.Substring(0, addressLength), out IPAddress address))
        //    {
        //        uint port = 0;

        //        if (addressLength == s.Length ||
        //            (uint.TryParse(s.Substring(addressLength + 1), NumberStyles.None, CultureInfo.InvariantCulture, out port) && port <= IPEndPoint.MaxPort))

        //        {
        //            result = new IPEndPoint(address, (int)port);

        //            return true;
        //        }
        //    }

        //    result = null;

        //    return false;
        //}

        //public static IPEndPoint Parse(string s)
        //{
        //    if (s == null)
        //    {
        //        throw new ArgumentNullException(nameof(s));
        //    }

        //    if (TryParse(s, out IPEndPoint result))
        //    {
        //        return result;
        //    }

        //    throw new FormatException(@"An invalid IPEndPoint was specified.");
//        }
    }
}
