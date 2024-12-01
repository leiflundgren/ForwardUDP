using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ForwardUDP
{
    public class Log : IDisposable
    {
        public static int LogLevel = 5;


        private bool disposedValue;

        public static void Init()
        {
            
        }

        //public static NLog.LogLevel TranslateLogLevel(int lvl)
        //{
        //    switch (lvl)
        //    {
        //        case 0: return NLog.LogLevel.Fatal;
        //        case 1: return NLog.LogLevel.Error;
        //        case 2: return NLog.LogLevel.Warn;
        //        case 3: return NLog.LogLevel.Info;
        //        case 4: return NLog.LogLevel.Debug;
        //        default: return NLog.LogLevel.Trace;
        //    }
        //}

        public static void Exception(Exception e, string? msg = null, bool logCallStack = true, int loglevel = 1, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (ShouldLog(loglevel))
            {
                var a = e as AggregateException;
                if (a != null && a.InnerException != null && a.InnerExceptions.Count == 1)
                    e = a.InnerException;

                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(msg))
                {
                    sb.Append(msg);
                    sb.Append(' '); // Makes it look nicer in logreader
                    sb.AppendLine();
                }

                for (Exception? e0 = e; e0 != null; e0 = e0.InnerException)
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
            if (ShouldLog(loglevel))
            {
                ConsoleLog(msg, loglevel, memberName, sourceFilePath, sourceLineNumber);
                //NLog.LogLevel nlvl = TranslateLogLevel(loglevel);
                //logger.Log(nlvl, msg);
            }
        }

        public static bool ShouldLog(int logLevel)
        {
            return logLevel <= LogLevel;
        }

        public static void ConsoleLog(string msg, int level = 3, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            const string format = "HH:mm:ss.fff ";
                        
            string ts = DateTime.Now.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
            Console.WriteLine(ts + msg);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                //    LogManager.Shutdown();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Log()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
