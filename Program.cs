using ForwardUDP.Components;
using ForwardUDP.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using System.Net;
namespace ForwardUDP
{
    internal static class Program
    {


       
        static void Main(string[] args)
        {
            try
            {
                IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
                Settings settings = config.GetRequiredSection("Settings").Get<Settings>() ?? new Settings();


                bool run_console = args.GetCommandLineArg("debug") != null || args.GetCommandLineArg("console") != null;

                settings.LogPath = args.GetCommandLineArg("logpath") ?? System.Environment.GetEnvironmentVariable("TEMP") ?? settings.LogPath;

                settings.LogLevel = int.TryParse(args.GetCommandLineArg("loglevel"), out int loglevel) ? loglevel : settings.LogLevel;

                string? local = args.GetCommandLineArg("local");
                string? target = args.GetCommandLineArg("target");
                if (local is not null && IPEndPoint.TryParse(local, out IPEndPoint? _))
                    settings.Local = local;
                if (target is not null && IPEndPoint.TryParse(target, out IPEndPoint? _))
                    settings.Targets = [target];

                if (settings.Local is null) throw new ArgumentException("Local not specified/invalid IP");
                if (settings.Targets is null || settings.Targets.Length == 0) throw new ArgumentException("Targets not specified/invalid IP");


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

                        CancellationTokenSource cts = new CancellationTokenSource();

                        Task serviceTask = service.ConsoleExecuteAsync(cts.Token);
                        Log.Msg(3, "Waiting for event : ");
                        ms_exitEvent.WaitOne();

                        Console.CancelKeyPress -= Console_CancelKeyPress;
                        Log.Msg(4, "Stopping service by cancelling token");
                        cts.CancelAsync().Forget(TimeSpan.FromSeconds(20));
                    }
                }
                else
                {
                    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
                    builder.Services.AddWindowsService(options =>
                    {
                        options.ServiceName = "ForwardUDP";
                    });

                    //LoggerProviderOptions.RegisterProviderOptions<
                    //    EventLogSettings, EventLogLoggerProvider>(builder.Services);

                    builder.Services.AddSingleton(service);
                    builder.Services.AddHostedService((serviceProvider => service));

                    IHost host = builder.Build();
                    host.Run();

                    // ServiceBase.Run(service);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

    }
}


