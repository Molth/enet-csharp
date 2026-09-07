using System;
using System.Net;
using System.Text;
using System.Threading;
using enet;
using Enet;
using ThreadedEnet;

namespace Test3
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

        private static bool ShouldExit()
        {
            try
            {
                return Console.KeyAvailable;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static void StartServer()
        {
            var config = new EnetHostConfig();
            config.LocalAddress.FromIpAddress(IPAddress.IPv6Any, 12345);
            config.PeerCount = 100;
            config.Option = EnetHostOption.Ipv6DualMode;
            config.ServiceTimeout = 15;

            using (var server = new ThreadedManagedEnetHost())
            {
                server.Start(config);

                while (!ShouldExit())
                {
                    server.PollEvents(
                        static (host, uid, address, command, events) =>
                        {
                            address.ToIpEndPoint(out var ipEndPoint);
                            Console.WriteLine("[Server] connected - Uid: " + uid + ", Address: " + ipEndPoint);
                            return true;
                        },
                        static (host, uid, address, command, events) =>
                        {
                            address.ToIpEndPoint(out var ipEndPoint);
                            Console.WriteLine("[Server] disconnected - Uid: " + uid + ", Address: " + ipEndPoint);
                            return true;
                        },
                        static (host, uid, address, command, events) =>
                        {
                            var packet = command.Packet;
                            address.ToIpEndPoint(out var ipEndPoint);
                            Console.WriteLine("[Server] received from - IncomingPeerId: " + uid.IncomingPeerId + ", Address: " + ipEndPoint + ", Data length: " + packet.DataLength);
                            var text = Encoding.UTF8.GetString(packet.AsSpan());
                            Console.WriteLine("[Server] " + text);
                            packet.Dispose();

                            Console.WriteLine();
                            Thread.Sleep(1000);
                            var reply = text + " Hello client!";
                            var replyPacket = EnetPacket.Create(Encoding.UTF8.GetBytes(reply), EnetPacketFlag.Reliable);
                            host.Send(uid, 0, ref replyPacket);
                            return true;
                        },
                        0);

                    Thread.Sleep(15);
                }
            }
        }

        private static void StartClient()
        {
            var config = new EnetHostConfig();
            config.LocalAddress.FromIpAddress(IPAddress.Any, 0);
            config.PeerCount = 1;
            config.Option = EnetHostOption.Ipv4;
            config.ServiceTimeout = 15;

            var serverAddress = new ENetAddress();
            serverAddress.FromIpAddress(IPAddress.Loopback, 12345);

            using (var client = new ThreadedManagedEnetHost())
            {
                client.Start(config);
                client.Connect(serverAddress, 0, 0);

                while (!ShouldExit())
                {
                    client.PollEvents(
                        static (host, uid, address, command, events) =>
                        {
                            address.ToIpEndPoint(out var ipEndPoint);
                            Console.WriteLine("[Client] connected. " + ipEndPoint);

                            var packet = EnetPacket.Create("Hello server!"u8, EnetPacketFlag.Reliable);
                            host.Send(uid, 0, ref packet);
                            return true;
                        },
                        static (host, uid, address, command, events) =>
                        {
                            Console.WriteLine("[Client] disconnected.");
                            return true;
                        },
                        static (host, uid, address, command, events) =>
                        {
                            var packet = command.Packet;
                            Console.WriteLine("[Client] received - Data length: " + packet.DataLength);
                            var text = Encoding.UTF8.GetString(packet.AsSpan());
                            Console.WriteLine("[Client] " + text);
                            packet.Dispose();

                            Console.WriteLine();
                            Thread.Sleep(1500);
                            var reply = text + " Hello server!";
                            var replyPacket = EnetPacket.Create(Encoding.UTF8.GetBytes(reply), EnetPacketFlag.Reliable);
                            host.Send(uid, 0, ref replyPacket);
                            return true;
                        },
                        0);

                    Thread.Sleep(15);
                }
            }
        }
    }
}