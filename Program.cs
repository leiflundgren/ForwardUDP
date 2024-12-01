using ForwardUDP.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForwardUDP
{
    internal static class Program
    {

        private static AutoResetEvent ms_exitEvent;

       
        static void Main(string[] args)
        {
            UdpForwardService service = new UdpForwardService();

            bool run_console = args.GetCommandLineArg("debug") != null || args.GetCommandLineArg("console") != null;

            string logpath = args.GetCommandLineArg("logpath");

            int loglevel;
            if ( !int.TryParse(args.GetCommandLineArg("loglevel"), out loglevel) )
                loglevel = 7;
            Log.LogLevel = loglevel;


            if (run_console)
            {
                
                using (ms_exitEvent = new AutoResetEvent(false))
                {
                    Console.CancelKeyPress += Console_CancelKeyPress;

                    service.ConsoleStart(args.Skip(1).ToArray());
                    Log.Msg(3, "Waiting for event : ");
                    ms_exitEvent.WaitOne();

                    //Console.CancelKeyPress -= Console_CancelKeyPress;
                    service.ConsoleStop();
                }
            }
            else
            {
                ServiceBase.Run(service);
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Log.Msg(3, "Cancel key press detected, setting event to terminte proccess. ");
            ms_exitEvent.Set();
        }
    }
}
