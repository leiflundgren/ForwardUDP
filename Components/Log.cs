using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;
using NLogLevel = NLog.LogLevel;

namespace ForwardUDP.Components
{
    public class Log : IDisposable
    {
        public static int LogFilterLevel => Instance?.logFilterLevel ?? 0;
        public static int EntryExitLogLevel => Instance?.entryExitLogLevel ?? 100;


        private int logFilterLevel = 5;
        private int entryExitLogLevel = 5;

        private bool disposedValue;
        private Microsoft.Extensions.Logging.ILogger? logger;
        private NLog.Targets.FileTarget? logfile;

        private static Log? Instance;

        public static void Init(int loglevel, string logname, string? logPath, bool LogConsole)
        {
            Instance = new Log();

            Instance.logFilterLevel = loglevel;

            Instance.logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger(logname);
            Instance.logger.LogInformation("Program has started.");

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            Instance.logfile = new NLog.Targets.FileTarget("logfile") 
            { 
                FileName = Path.Combine(logPath, $"{logname}.txt"), 
                MaxArchiveDays = 15, 
                ArchiveDateFormat = "yyMMdd",
            };
            config.AddRule(NLogLevel.Trace, NLogLevel.Fatal, Instance.logfile);

            // Rules for mapping loggers to targets            
            if (LogConsole)
            {
                var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
                config.AddRule(NLogLevel.Trace, NLogLevel.Fatal, logconsole);
            }

            // Apply config           
            NLog.LogManager.Configuration = config;


            Instance.logger.LogInformation("Logging started");
        }

        public static void Shutdown()
        {
            if (Instance is null) return;
            if (Instance.logger is null) return;

            if ( Instance.logfile is not null) 
                Instance.logfile.Dispose();


        }



        public static void LogMessage(LogMsg msg)
        {
            MSLogLevel TranslateLogLevel(int lvl)
            {
                int ord = Math.Max(0, (int)MSLogLevel.Critical- lvl);
                return (MSLogLevel)ord;
                //switch (lvl)
                //{
                //    case 0: return LogLevel.Fatal;
                //    case 1: return LogLevel.Error;
                //    case 2: return LogLevel.Warning;
                //    case 3: return LogLevel.Information;
                //    case 4: return LogLevel.Debug;
                //    default: return LogLevel.Trace;
                //}
            }

            if (ShouldLog(msg.LogLevel))
            {
                // ConsoleLog(msg);

                if (Instance?.logger != null)
                {
                    MSLogLevel mslvl = TranslateLogLevel(msg.LogLevel);
                    string s = FormatLog(msg);
                    Instance.logger.Log(mslvl, s);
                }
            }
        }
        public static bool ShouldLog(int logLevel)
        {
            return logLevel <= LogFilterLevel;
        }


        public static void Msg(int loglevel, string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogMessage(new LogMsg(loglevel, msg, memberName, sourceFilePath, sourceLineNumber));
        }


        public static Func<LogMsg, string> FormatLog = (msg) =>
        {
            //const string format = "HH:mm:ss.fff ";

            //string ts = msg.TimeStamp.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
            return $"[{msg.ThreadId}] | {msg.CallerMemberName} {msg.Msg} |{Path.GetFileName(msg.CallerFilePath)}:{msg.CallerLineNumber}";
        };

        public static void ConsoleLog(LogMsg msg)
        {
            Console.WriteLine(FormatLog(msg));
        }


        public static LogBlock LogContext(string? msg = null, int? level = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            return new LogBlock(level ?? EntryExitLogLevel, msg, null, memberName, sourceFilePath, sourceLineNumber);
        }


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

        #region Disposing
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

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("{Msg}")]
    public class LogMsg
    {
        public int LogLevel;
        public string? Msg;
        public DateTime TimeStamp;
        public int ThreadId;
        public string? CallerMemberName;
        public string? CallerFilePath;
        public int CallerLineNumber;

        public LogMsg() { }

        public LogMsg(int logLevel, string? msg, int? threadId, DateTime? timeStamp, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogLevel = logLevel;
            Msg = msg;
            ThreadId = threadId ?? Thread.CurrentThread.ManagedThreadId;
            TimeStamp = timeStamp ?? DateTime.UtcNow;
            CallerMemberName = memberName;
            CallerFilePath = sourceFilePath;
            CallerLineNumber = sourceLineNumber;
        }

        public LogMsg(int logLevel, string? msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : this(logLevel, msg, null, null, memberName, sourceFilePath, sourceLineNumber)
        { }

    }

    [System.Diagnostics.DebuggerDisplay("{enterMsg}")]
    public class LogBlock : IDisposable
    {
        private LogMsg enterMsg;
        private readonly System.Diagnostics.Stopwatch watch;
        public TimeSpan LongWaitLimit;
        public static TimeSpan LongWaitLimitStatic = TimeSpan.FromSeconds(10);


        public int LogLevel { get => enterMsg.LogLevel; set => enterMsg.LogLevel = value; }

        public string? ExitMsg;
        public static readonly LogBlock SafeDefaultBlock = new LogBlock(100, string.Empty, TimeSpan.MaxValue);

        public Exception? Exception;

        public void ResetStopWatch()
        {
            watch.Reset();
        }


        public LogBlock(int lvl, string textMessage, TimeSpan? longWaitLimit = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            watch = System.Diagnostics.Stopwatch.StartNew();
            LongWaitLimit = longWaitLimit ?? LongWaitLimitStatic;

            // Store enter-msg even if we don't log.
            // IF we exit on exception, then we want to know the entry-arguments
            string s = $"<--entry-- {textMessage}";
            enterMsg = new LogMsg(lvl, s, memberName, sourceFilePath, sourceLineNumber);

            Log.LogMessage(enterMsg);
        }


        public virtual void Dispose()
        {
            TimeSpan elapsed = watch.Elapsed;
            var lvl = enterMsg.LogLevel;

            int lvl_inc = 0;
            int exception_code = 0;

            if (Exception != null)
            {
                lvl_inc = 4;
            }
            else
            {
                try
                {
#pragma warning disable CS0618 // Type or member is obsolete, but use as long as we can!
                    exception_code = System.Runtime.InteropServices.Marshal.GetExceptionCode();
#pragma warning restore CS0618 // Type or member is obsolete
                    if (exception_code == 0 && System.Runtime.InteropServices.Marshal.GetExceptionPointers() != nint.Zero)
                        exception_code = -1;
                }
                catch { }
                if (exception_code != 0)
                {
                    lvl_inc = 4;
                }
                else if (elapsed > LongWaitLimit)
                {
                    lvl_inc = 3;
                }
            }


            int exitLevel = Math.Max(0, lvl - lvl_inc);

            StringBuilder s = new StringBuilder();
            s.Append("--Exit--> ");
            if (elapsed < TimeSpan.FromMinutes(1))
            {
                s.Append(elapsed.TotalMilliseconds.ToString("F0"));
                s.Append("ms ");
            }
            else
            {
                s.Append(elapsed);
                s.Append(' ');
            }

            if (Exception != null)
            {
                s.Append("Exit due to exception " + Exception.GetType().Name + ": " + Exception.Message);
                s.Append("Enter:message  ");
                s.Append(enterMsg);
            }
            else if (exception_code != 0)
            {
                s.Append("Exit due to exception 0x" + exception_code.ToString("X8"));
            }

            s.Append(ExitMsg);

            LogMsg exitMsg = new LogMsg(exitLevel, s.ToString(), enterMsg.CallerMemberName, enterMsg.CallerFilePath, enterMsg.CallerLineNumber);

            Log.LogMessage(exitMsg);
        }
    }
}
