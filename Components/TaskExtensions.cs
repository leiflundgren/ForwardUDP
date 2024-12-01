using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForwardUDP.Components
{
    public static class TaskExtensionsTas
    {
        public static void Forget(this Task task, string? msg = null, bool logCallStack = true, int loglevel = 1)
        {
            Forget(task, TimeSpan.Zero, msg, logCallStack, loglevel);
        }
        public static void Forget(this Task task, TimeSpan waitBeforeForgetting, string? msg = null, bool logCallStack = true, int loglevel = 1)
        {
            static void LogException(Exception? e, string msg)
            {
                if (e is not null)
                {
                    if (e is AggregateException && e.InnerException is not null)
                        e = e.InnerException;

                    Log.Exception(e, msg);
                }
            }
            
            if (task == null)
                return;

            msg = msg ?? $"Explicitly forgotten task {task.Id}";

            if (waitBeforeForgetting > TimeSpan.Zero && !task.IsCompleted && task.Wait(waitBeforeForgetting))
            {
                if (task.IsFaulted )
                    LogException(task.Exception, msg);
            }
            else
            {
                task.ContinueWith(
                    t => LogException(t.Exception, msg), TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        public static Task EmptyTask
        {
            get { return Task.WhenAll(); }
        }

        public static Task Await(this Task task, TimeSpan maxAwait)
        {
            return Task.WhenAny(task, Task.Delay(maxAwait));
        }
    }
}
