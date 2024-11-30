using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ForwardUDP
{
    public class Log
    {
        public static int LogLevel = 5;



        public static void Exception(Exception e, string msg = null, bool logCallStack = true, int loglevel = 1, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (ShouldLog(loglevel))
            {
                var a = e as AggregateException;
                if (a != null && a.InnerExceptions.Count == 1)
                    e = a.InnerException;

                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(msg))
                {
                    sb.Append(msg);
                    sb.Append(' '); // Makes it look nicer in logreader
                    sb.AppendLine();
                }

                for (Exception e0 = e; e0 != null; e0 = e0.InnerException)
                {
                    sb.Append(e0.GetType().Name);
                    sb.Append(' ');
                    sb.Append(e0.Message);
                    sb.AppendLine();
                    if (logCallStack)
                        sb.AppendLine(e0.StackTrace);
                }

                Msg(loglevel, sb.ToString(), memberName, sourceFilePath, sourceLineNumber);
            }
        }

        public static void Msg(int loglevel, string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            ConsoleLog(msg, loglevel, memberName, sourceFilePath, sourceLineNumber);
        }

        public static bool ShouldLog(int logLevel)
        {
            return logLevel <= LogLevel;
        }

        public static void ConsoleLog(string msg, int level = 3, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!ShouldLog(level))
                return;

            const string format = "HH:mm:ss.fff ";
                        
            string ts = DateTime.Now.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
            Console.WriteLine(ts + msg);
        }

    }
}
