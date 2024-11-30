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
    public class FwdUDP
    {
        public IPEndPoint ListenTo { get; }
        public List<IPEndPoint> Targets { get; }

        private readonly UdpClient udp;
        public FwdUDP(IPEndPoint listenTo, IPEndPoint target)
            : this(listenTo, new[] { target })
        { }

        public FwdUDP(IPEndPoint listenTo, ICollection<IPEndPoint> targets)
        {
            if (listenTo is null) throw new ArgumentNullException(nameof(listenTo));
            if (targets is null) throw new ArgumentNullException(nameof(targets));
            if ( targets.Count == 0) throw new ArgumentOutOfRangeException(nameof(targets));

            ListenTo = listenTo;
            Targets = targets.ToList();


            udp = new UdpClient(listenTo);

            RecieveData().Forget();
        }

        private async Task RecieveData()
        {
            IPEndPoint clientEndPoint = null;

            for (; ; )
            {
                UdpReceiveResult read = await udp.ReceiveAsync();

                Log.LogMsg(4, $"Received {read.Buffer?.Length} bytes from {read.RemoteEndPoint}");

                bool from_a_target = Targets.Any(t => read.RemoteEndPoint.Equals(t) );

                if ( !from_a_target && !read.RemoteEndPoint.Equals(clientEndPoint) )
                {
                    clientEndPoint = read.RemoteEndPoint;
                    Log.LogMsg(3, $"New client {clientEndPoint} connected");
                }

                ICollection<IPEndPoint> send_to = from_a_target ? new[]{ clientEndPoint }.StaticCast<ICollection<IPEndPoint>>() : Targets.StaticCast<ICollection<IPEndPoint>>() ;


                foreach ( IPEndPoint target in send_to )
                {
                    try
                    {
                        udp.Send(read.Buffer, read.Buffer.Length, target);
                        Log.LogMsg(6, $"sent data to {target}");
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex, $"Failed to send data to {target}");
                    }

                }
            }
        }
    }
}
