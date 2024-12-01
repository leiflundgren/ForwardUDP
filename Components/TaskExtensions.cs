using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForwardUDP.Components
{
    public static class TaskExtensionsTas
    {
        public static void Forget(this Task task)
        {
            task.Forget(TimeSpan.Zero);
        }
        public static void Forget(this Task task, TimeSpan waitBeforeForgetting)
        {
            if (task == null)
                return;

            if (waitBeforeForgetting > TimeSpan.Zero && !task.IsCompleted && task.Wait(waitBeforeForgetting))
            {
                if (task.IsFaulted && task.Exception != null)
                {
                    if (task.Exception is AggregateException && task.Exception.InnerException != null)
                        Log.Exception(task.Exception.InnerException, "Explicitly forgotten task " + task.Id);
                    else
                        Log.Exception(task.Exception, "Explicitly forgotten task " + task.Id);
                }
            }
            else
            {
                task.ContinueWith(
                    t =>
                    {
                        if (t.Exception is null)
                        {}
                        else if (t.Exception is AggregateException && t.Exception?.InnerException != null)
                            Log.Exception(t.Exception.InnerException, "Explicitly forgotten task " + t.Id);
                        else
                            Log.Exception(t.Exception, "Explicitly forgotten task " + t.Id);
                    },
                    TaskContinuationOptions.OnlyOnFaulted);
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
