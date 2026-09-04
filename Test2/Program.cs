using System;
using System.Net;
using System.Text;
using System.Threading;
using enet;
using Enet;

namespace Test2
{
    internal class Program
    {
        private static void Main()
        {
            Console.CancelKeyPress += static (_, _) => ENET_API.enet_deinitialize();
            ENET_API.enet_initialize();

            new Thread(StartServer).Start();
            Thread.Sleep(1000);

            new Thread(StartClient).Start();
            Thread.Sleep(2000);

            StartClient();
        }

        private static void StartServer()
        {
            var address = new ENetAddress();
            address.FromIpAddress(IPAddress.IPv6Any, 12345);

            using (var server = ManagedEnetHost.Create(address, 100, 0, 0, 0, EnetHostOption.Ipv6DualMode))
            {
                while (!Console.KeyAvailable)
                {
                    var polled = false;
                    while (!polled)
                    {
                        if (server.CheckEvents(out var @event) <= 0)
                        {
                            if (server.Service(15, out @event) <= 0)
                                break;

                            polled = true;
                        }

                        var peer = @event.Peer;
                        IPEndPoint? ipEndPoint;
                        switch (@event.Type)
                        {
                            case EnetEventType.None:
                                break;

                            case EnetEventType.Connect:
                                peer.Address.ToIpEndPoint(out ipEndPoint);
                                Console.WriteLine("[Server] connected - Id: " + peer.IncomingPeerId + ", Address: " + ipEndPoint);

                                peer.SetTimeout(0, 100_000, 1_000_000);

                                break;

                            case EnetEventType.Disconnect:
                                peer.Address.ToIpEndPoint(out ipEndPoint);
                                Console.WriteLine("[Server] disconnected - Id: " + peer.IncomingPeerId + ", Address: " + ipEndPoint);
                                break;

                            case EnetEventType.Receive:
                                peer.Address.ToIpEndPoint(out ipEndPoint);
                                var packet = @event.Packet;
                                Console.WriteLine("[Server] received from - Id: " + peer.IncomingPeerId + ", Address: " + ipEndPoint + ", Channel Id: " + @event.ChannelId + ", Data length: " + packet.DataLength);
                                var text = Encoding.UTF8.GetString(packet.AsSpan());
                                Console.WriteLine($"[Server] {text}");
                                packet.Dispose();

                                Console.WriteLine();
                                Thread.Sleep(1000);
                                var reply = text + " " + "Hello client!";
                                packet = EnetPacket.Create(Encoding.UTF8.GetBytes(reply), EnetPacketFlag.Reliable);
                                if (!peer.Send(0, ref packet))
                                    packet.Dispose();

                                break;
                        }
                    }

                    server.Flush();
                }
            }
        }

        private static void StartClient()
        {
            var address = new ENetAddress();
            address.FromIpAddress(IPAddress.Any, 0);

            var serverAddress = new ENetAddress();
            serverAddress.FromIpAddress(IPAddress.Loopback, 12345);

            var ip = new char[256];

            using (var client = ManagedEnetHost.Create(address, 1, 0, 0, 0, EnetHostOption.Ipv4))
            {
                client.TryConnect(serverAddress, 0, 0, out _);

                while (!Console.KeyAvailable)
                {
                    var polled = false;
                    while (!polled)
                    {
                        if (client.CheckEvents(out var @event) <= 0)
                        {
                            if (client.Service(15, out @event) <= 0)
                                break;

                            polled = true;
                        }

                        var peer = @event.Peer;
                        EnetPacket packet;
                        switch (@event.Type)
                        {
                            case EnetEventType.None:
                                break;

                            case EnetEventType.Connect:
                                var span = ip.AsSpan();
                                peer.Address.GetIp(ref span);

                                Console.WriteLine($"[Client] connected. {span}:{peer.Address.Port}");

                                packet = EnetPacket.Create("Hello server!"u8, EnetPacketFlag.Reliable);
                                if (!peer.Send(0, ref packet))
                                    packet.Dispose();

                                break;

                            case EnetEventType.Disconnect:
                                Console.WriteLine("[Client] disconnected.");
                                break;

                            case EnetEventType.Receive:
                                packet = @event.Packet;
                                Console.WriteLine("[Client] received - ChannelId: " + @event.ChannelId + ", Data length: " + packet.DataLength);
                                var text = Encoding.UTF8.GetString(packet.AsSpan());
                                Console.WriteLine($"[Client] {text}");
                                packet.Dispose();

                                Console.WriteLine();
                                Thread.Sleep(1500);
                                var reply = text + " " + "Hello server!";
                                packet = EnetPacket.Create(Encoding.UTF8.GetBytes(reply), EnetPacketFlag.Reliable);
                                if (!peer.Send(0, ref packet))
                                    packet.Dispose();
                                break;
                        }
                    }

                    client.Flush();
                }
            }
        }
    }
}