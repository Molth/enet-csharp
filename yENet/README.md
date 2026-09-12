This project is a pure C# translation of [enet](https://github.com/lsalzman/enet), no binaries.

Where enet implementation: [xENet](https://www.nuget.org/packages/xENet).

**It is fully wire‑compatible with the original C library.**

## Why?

The original ENet relies on platform-specific native binaries,

which poses challenges for cross-platform distribution and deployment.

This project eliminates that dependency while preserving full compatibility with the original implementation.

[![NuGet](https://img.shields.io/nuget/v/yENet.svg?style=flat-square)](https://www.nuget.org/packages/yENet/)

## Supporters

This project is sponsored by: no one.

### a lifetime of living in obscurity, struggling to make ends meet, and enduring humiliation.

## Examples

```csharp
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
            const int timeout = 15;
            const int maxClients = 10;

            var address = new ENetAddress();
            address.FromIpAddress(IPAddress.IPv6Any, 12345);

            using (var host = ManagedEnetHost.Create(address, maxClients, 0, 0, 0, EnetHostOption.Ipv6DualMode))
            {
                host.TrySetCompressorWithRangeCoder();
                host.SetChecksumCallbackWithCrc32();

                while (!Console.KeyAvailable)
                {
                    // TODO: service first
                    if (host.Service(timeout, out var @event) > 0)
                    {
                        // TODO: use loop
                        while (true)
                        {
                            var peer = @event.Peer;
                            IPEndPoint? ipEndPoint;
                            switch (@event.Type)
                            {
                                case EnetEventType.Connect:
                                    peer.Address.ToIpEndPoint(out ipEndPoint);
                                    Console.WriteLine($"[Server] connected - IncomingPeerId: {peer.IncomingPeerId}, Address: {ipEndPoint}");

                                    peer.SetTimeout(0, 100_000, 1_000_000);

                                    break;

                                case EnetEventType.Disconnect:
                                    peer.Address.ToIpEndPoint(out ipEndPoint);
                                    Console.WriteLine($"[Server] disconnected - IncomingPeerId: {peer.IncomingPeerId}, Address: {ipEndPoint}");
                                    break;

                                case EnetEventType.Receive:
                                    var packet = @event.Packet;
                                    Console.WriteLine($"[Server] received from - IncomingPeerId: {peer.IncomingPeerId}");
                                    var text = Encoding.UTF8.GetString(packet.AsSpan());
                                    Console.WriteLine($"[Server received] {text}");
                                    Console.WriteLine();
                                    packet.Dispose();

                                    Thread.Sleep(1000);
                                    var reply = $"{text} Hello!";
                                    packet = EnetPacket.Create(Encoding.UTF8.GetBytes(reply), EnetPacketFlag.Reliable);
                                    if (!peer.TrySend(0, ref packet))
                                        packet.Dispose();

                                    break;
                            }

                            if (host.CheckEvents(out @event) <= 0)
                                break;
                        }
                    }
                }
            }
        }

        private static void StartClient()
        {
            const int timeout = 15;
            const int maxClients = 1;

            var address = new ENetAddress();
            address.FromIpAddress(IPAddress.Any, 0);

            var serverAddress = new ENetAddress();
            serverAddress.FromIpAddress(IPAddress.Loopback, 12345);

            var ip = new char[256];

            using (var host = ManagedEnetHost.Create(address, maxClients, 0, 0, 0, EnetHostOption.Ipv4))
            {
                host.TrySetCompressorWithRangeCoder();
                host.SetChecksumCallbackWithCrc32();

                host.TryConnect(serverAddress, 0, 0, out _);

                while (!Console.KeyAvailable)
                {
                    // TODO: service first
                    if (host.Service(timeout, out var @event) > 0)
                    {
                        // TODO: use do-while
                        do
                        {
                            var peer = @event.Peer;
                            EnetPacket packet;
                            switch (@event.Type)
                            {
                                case EnetEventType.Connect:
                                    var span = ip.AsSpan();
                                    peer.Address.GetIp(ref span);

                                    Console.WriteLine("[Client] connected.");

                                    packet = EnetPacket.Create("Hello server!"u8, EnetPacketFlag.Reliable);
                                    if (!peer.TrySend(0, ref packet))
                                        packet.Dispose();

                                    break;

                                case EnetEventType.Disconnect:
                                    Console.WriteLine("[Client] disconnected.");
                                    break;

                                case EnetEventType.Receive:
                                    packet = @event.Packet;

                                    var text = Encoding.UTF8.GetString(packet.AsSpan());
                                    Console.WriteLine($"[Client received] {text}");
                                    Console.WriteLine();
                                    packet.Dispose();

                                    Thread.Sleep(1500);
                                    var reply = $"{text} again!";
                                    packet = EnetPacket.Create(Encoding.UTF8.GetBytes(reply), EnetPacketFlag.Reliable);
                                    if (!peer.TrySend(0, ref packet))
                                        packet.Dispose();

                                    break;
                            }
                        } while (host.CheckEvents(out @event) > 0);
                    }
                }
            }
        }
    }
}
```