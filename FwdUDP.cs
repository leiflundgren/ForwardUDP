using ForwardUDP.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ForwardUDP
{
    public class FwdUDP : IDisposable
    {

        private class Target
        {
            public IPEndPoint IPEndPoint;
            public UdpClient socket;
        }

        private List<Target> targets;
        

        private Target local;
        private bool disposedValue;

        public FwdUDP(IPEndPoint listenTo, IPEndPoint target)
            : this(listenTo, new[] { target })
        { }

        public FwdUDP(IPEndPoint listenTo, ICollection<IPEndPoint> targets)
        {
            if (listenTo is null) throw new ArgumentNullException(nameof(listenTo));
            if (targets is null) throw new ArgumentNullException(nameof(targets));
            if ( targets.Count == 0) throw new ArgumentOutOfRangeException(nameof(targets));

            this.local = new Target { IPEndPoint = listenTo, socket = new UdpClient(listenTo) };
            this.targets = targets.ToList().ConvertAll(ep => 
                new Target
                {
                    IPEndPoint = ep,
                    socket = new UdpClient(ep.AddressFamily),
                }
            );


            RecieveData().Forget();
        }

        private async Task RecieveData()
        {
            List<Target> targets = new List<Target> { this.local };
            targets.AddRange(this.targets);

            for (; ; )
            {
                IPEndPoint recv_from;
                int recv_bytes;
                byte[] buf;
                try
                {
                    UdpReceiveResult read = await local.socket.ReceiveAsync();
                    recv_from = read.RemoteEndPoint;
                    recv_bytes = read.Buffer.Length;
                    buf = read.Buffer;
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, $"Failed to receive data. Terminating", false, 2);
                    break;
                }

                Log.LogMsg(4, $"Received {recv_bytes} bytes from {recv_from}");

                foreach ( IPEndPoint target in send_to )
                {
                    try
                    {
                        udp.Send(buf, recv_bytes, target);
                        Log.LogMsg(6, $"sent data to {target}");
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex, $"Failed to send data to {target}");
                    }

                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    udp.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FwdUDP()
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
}
