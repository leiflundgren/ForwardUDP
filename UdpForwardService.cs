using ForwardUDP.Components;
using ForwardUDP.Properties;
using System.Net;

namespace ForwardUDP
{
    public class ServiceBase: IDisposable
    {
        private bool disposedValue;

        protected virtual void OnStart(string[] args) { }
        protected virtual void OnStop() { }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ServiceBase()
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

    public partial class UdpForwardService : ServiceBase
    {
        private FwdUDP? forwarder;


        public UdpForwardService(Settings settings)
        {
            if ( settings is null ) throw new ArgumentNullException(nameof(settings), "Without settings, cannot run");

            this.settings = settings;
            InitializeComponent();
        }

        public void ConsoleStart(string[] args)
        {
            OnStart(args);
        }
        public void ConsoleStop()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)        
        {
            IPEndPoint? ParseIP(string? s)
            {
                return (! string.IsNullOrEmpty(s) && IPEndPoint.TryParse(s, out IPEndPoint? ep)) ? ep : null;
            }


            base.OnStart(args);


            string[]? str_targets = settings.Targets;
            if (str_targets == null || str_targets.Length == 0) throw new ArgumentException("Targets not specified");

            IPEndPoint listenTo = ParseIP( args.GetCommandLineArg("local")) ?? ParseIP(settings.Local) ?? throw new ArgumentException("Local not specified/invalid IP");
            List<IPEndPoint> targets = new List<IPEndPoint>();
            IPEndPoint? ep = ParseIP( args.GetCommandLineArg("target"));
            if (ep != null)
                targets.Add(ep);
            else
                foreach (string? s in str_targets)
                {
                    ep = ParseIP(s);
                    if (ep != null) targets.Add(ep);
                }

            if ( targets.Count == 0) throw new ArgumentException("Local not specified/invalid IP");


            forwarder = new FwdUDP(listenTo, targets);
            
        }

        protected override void OnStop()
        {
            FwdUDP? forwarder = this.forwarder;
            this.forwarder = null;
            if (forwarder != null)
            {
                forwarder.Dispose();
            }
        }

        internal void Run()
        {
            throw new NotImplementedException();
        }

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer? components = null;
        private readonly Settings settings;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            //this.ServiceName = "FwdUDP";
        }
    }

}
