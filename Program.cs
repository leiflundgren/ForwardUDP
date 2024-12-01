using ForwardUDP.Components;
using ForwardUDP.Properties;
using Microsoft.Extensions.Configuration;
namespace ForwardUDP
{
    internal static class Program
    {


       
        static void Main(string[] args)
        {

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            Settings? settings = config.GetRequiredSection("Settings").Get<Settings>();
                        

            bool run_console = args.GetCommandLineArg("debug") != null || args.GetCommandLineArg("console") != null;

            string? logpath = args.GetCommandLineArg("logpath") ?? settings?.LogPath ?? System.Environment.GetEnvironmentVariable("TEMP");

            int loglevel;
            if ( !int.TryParse(args.GetCommandLineArg("loglevel"), out loglevel) )
                loglevel = settings?.LogLevel ?? 5;
            Log.LogLevel = loglevel;


            UdpForwardService service = new UdpForwardService(settings);
            if (run_console)
            {
                using (var ms_exitEvent = new AutoResetEvent(false))
                {
                    void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
                    {
                        Log.Msg(3, "Cancel key press detected, setting event to terminte proccess. ");
                        ms_exitEvent.Set();
                    }
                
                    Console.CancelKeyPress += Console_CancelKeyPress; ;

                    service.ConsoleStart(args.Skip(1).ToArray());
                    Log.Msg(3, "Waiting for event : ");
                    ms_exitEvent.WaitOne();

                    Console.CancelKeyPress -= Console_CancelKeyPress;
                    service.ConsoleStop();
                }
            }
            else
            {
               // ServiceBase.Run(service);
            }
        }

    }
}
