using DNS.Protocol;
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
        private readonly Disposer disposer;
        private bool disposedValue;

        public UdpForwardService(Settings settings)
        {
            if ( settings is null ) throw new ArgumentNullException(nameof(settings), "Without settings, cannot run");

            this.settings = settings;
            this.disposer = new Disposer();
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
                forwarder.DataReceived += OnFwdDataReceived;
                

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

        private void OnFwdDataReceived(object sender, FwdUDP.UDPFwdArgs args)
        {
            FwdUDP fwdUDP = (FwdUDP)sender;
            if (settings.DataType == "DNS")
            {
                try
                {               
                    if ( args.Sender.Equals(fwdUDP.Local) ) 
                    {
                        try
                        {
                            DNS.Protocol.Request req = DNS.Protocol.Request.FromArray(args.Data);
                            LogDnsMessage(req);
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            var rsp = DNS.Protocol.Response.FromArray(args.Data);
                            LogDnsMessage(rsp);
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                }
            }
        }

        private void LogDnsMessage(Request req)
        {
            Log.Msg(4, req.ToString());
        }

        private void LogDnsMessage(Response rsp)
        {
            Log.Msg(4, rsp.ToString());
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.disposer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Disposer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public override void Dispose()
        {
            base.Dispose();
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

}
