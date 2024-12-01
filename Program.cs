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
                    //.AddEnvironmentVariables()
                    .Build();

                //Settings settings = new Settings();
                // IConfigurationSection section = config.GetSection("Settings");
                //Settings? settings_null = section.Get<Settings>();
                //config.Bind("Settings", settings);

                //Settings settings = settings_null ?? new Settings();

                Settings settings = new Settings();


                string? local = args.GetCommandLineArg("local");
                string? target = args.GetCommandLineArg("target");

                if (local is not null && IPEndPoint.TryParse(local, out IPEndPoint? _))
                    settings.Local = local;
                else
                    settings.Local = config.GetSection("Settings:Local").Value;

                if (target is not null && IPEndPoint.TryParse(target, out IPEndPoint? _))
                    settings.Targets = [target];
                else
                {
                    IConfigurationSection configTargets = config.GetSection("Settings:Targets");
                    List<string>? targets = configTargets.GetChildren().ToList().ConvertAll(s => s.Value).NonNull();
                    settings.Targets = targets.ToArray();
                }


                string? lvl_str = args.GetCommandLineArg("loglevel") ?? config.GetSection("Settings:LogLevel").Value;
                if (int.TryParse(lvl_str, out int loglevel))
                    settings.LogLevel = loglevel;

                settings.LogPath = args.GetCommandLineArg("logpath") ?? config.GetSection("Settings:LogPath").Value ?? System.Environment.GetEnvironmentVariable("TEMP");

                bool run_console = args.GetCommandLineArg("debug") != null || args.GetCommandLineArg("console") != null;


                if (settings.Local is null) throw new ArgumentException("Local not specified/invalid IP");
                if (settings.Targets is null || settings.Targets.Length == 0) throw new ArgumentException("Targets not specified/invalid IP");



                Log.Init(loglevel, "ForwardUDP", settings.LogPath);

                Log.Msg(3, "Logging started");

                UdpForwardService service = new UdpForwardService(settings);

                if (run_console)
                {
                    using (var ms_exitEvent = new AutoResetEvent(false))
                    {
                        void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
                        {
                            Log.Msg(3, "Cancel key press detected, setting event to terminate process. ");
                            ms_exitEvent.Set();
                        }

                        Console.CancelKeyPress += Console_CancelKeyPress; ;

                        CancellationTokenSource cts = new CancellationTokenSource();

                        Task serviceTask = service.ConsoleExecuteAsync(cts.Token);
                        Log.Msg(3, "Waiting for event : ");
                        ms_exitEvent.WaitOne();

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


