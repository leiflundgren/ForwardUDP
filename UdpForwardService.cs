using ForwardUDP.Components;
using ForwardUDP.Properties;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace ForwardUDP
{
    public class UdpForwardService : BackgroundService, IDisposable
    {
        private readonly Settings settings;


        public UdpForwardService(Settings settings)
        {
            if ( settings is null ) throw new ArgumentNullException(nameof(settings), "Without settings, cannot run");

            this.settings = settings;
        }

        public Task ConsoleExecuteAsync(CancellationToken stoppingToken)
        {
            return ExecuteAsync(stoppingToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IPEndPoint listenTo = IPEndPoint.Parse(settings.Local);
            List<IPEndPoint> targets = settings.Targets.ToList().ConvertAll(s => IPEndPoint.Parse(s));
            if ( targets.Count == 0) throw new ArgumentException("Local not specified/invalid IP");

            using (FwdUDP forwarder = new FwdUDP(listenTo, targets))
            using (var ctx = Log.LogContext("Running service", 3))
            {
                Task runFwd = forwarder.RunAsync(stoppingToken);
                Task stopTask = Task.Delay(Timeout.Infinite, stoppingToken);
                if ( stopTask == await Task.WhenAny(runFwd, stopTask) )
                {
                    Log.Msg(3, "Terminating service");
                }
                else
                {
                    runFwd.Forget("Service terminated by exception");
                }
            }
        }


    }

}
