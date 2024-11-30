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
            public IPEndPoint Local;
            public IPEndPoint Remote;
            public UdpClient socket;
            public bool IsServer;
            public Task<UdpReceiveResult> ReceiveTask;

            public override string ToString()
            {
                return $"Remote:{Remote} IsBound:{socket?.Client.IsBound ?? false} ";
            }
        }

        private List<Target> targets;
        

        private Target local;
        private bool disposedValue;
        private readonly TaskCompletionSource<UdpReceiveResult> never_complet = new TaskCompletionSource<UdpReceiveResult>();

        public FwdUDP(IPEndPoint listenTo, IPEndPoint target)
            : this(listenTo, new[] { target })
        { }

        public FwdUDP(IPEndPoint listenTo, ICollection<IPEndPoint> targets)
        {
            if (listenTo is null) throw new ArgumentNullException(nameof(listenTo));
            if (targets is null) throw new ArgumentNullException(nameof(targets));
            if ( targets.Count == 0) throw new ArgumentOutOfRangeException(nameof(targets));


            this.local = new Target { Local = listenTo, socket = new UdpClient(listenTo), IsServer = true };
            this.targets = targets.ToList().ConvertAll(ep => 
                new Target
                {
                    Remote = ep,
                    socket = null,
                    ReceiveTask = never_complet.Task,
                    IsServer = false,
                }
            );


            RecieveData().Forget();
        }

        private async Task RecieveData()
        {
            List<Target> sockets = new List<Target> { this.local };
            sockets.AddRange(this.targets);

            this.local.ReceiveTask = this.local.socket.ReceiveAsync();

            Log.Msg(4, $"Listening to {local.Local}");

            for (; ; )
            {

                Task<UdpReceiveResult> readTask = await Task.WhenAny(sockets.ConvertAll(t=>t.ReceiveTask));
                int idx = sockets.FindIndex(t => ReferenceEquals(readTask, t.ReceiveTask));

                Target readSocket = sockets[idx];

                IPEndPoint recv_from;
                int recv_bytes;
                byte[] buf;
                try
                {
                    UdpReceiveResult read = readTask.Result;
                    recv_from = read.RemoteEndPoint;
                    recv_bytes = read.Buffer.Length;
                    buf = read.Buffer;

                    readSocket.Remote = recv_from;
                }
                catch (Exception ex)
                {
                    Log.Exception(ex, $"Failed to receive data. Terminating", false, 2);
                    break;
                }


                // new read-task, since previous completed
                readSocket.ReceiveTask = readSocket.socket.ReceiveAsync();

                Log.Msg(4, $"Received {recv_bytes} bytes from {recv_from}");

                List<Target> send_to = idx!=0 ? new List<Target> { local } : this.targets;

                foreach ( Target target in send_to )
                {
                    try
                    {
                        IPEndPoint targetEP = target.Remote;

                        Log.Msg(6, $"sending {recv_bytes} bytes to {target} --> {targetEP}");

                        if (target.socket == null)
                            target.socket = new UdpClient(0, targetEP.AddressFamily);
                        bool was_bound = target.socket.Client.IsBound;
                        bool was_completed = target.ReceiveTask.IsCompleted;

                        target.socket.Send(buf, recv_bytes, targetEP);

                        if (target.socket.Client.IsBound && (target.ReceiveTask == never_complet.Task || target.ReceiveTask.IsCompleted))
                        {
                            Log.Msg(7, $"Starting receive on {target}");
                            target.ReceiveTask = target.socket.ReceiveAsync();
                        }
                        else
                        {
                            Log.Msg(7, $"Already ongoing receive on {target}, task {target.ReceiveTask.Id} state {target.ReceiveTask.Status} ");
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex, $"Failed to send data to {target}", logCallStack:!(ex is SocketException));
                    }

                }

            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;

                if (disposing)
                {
                    try { local.socket.Close(); } catch { }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
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
