using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using enet;
using static enet.ENET_API;

namespace Test1
{
    public sealed unsafe class TestWave
    {
        private static bool _running;

        public const int INTERVAL = 1;

        public static void Start()
        {
            Console.CancelKeyPress += (_, _) => _running = false;
            _running = true;
            new Thread(StartServer).Start();
            Thread.Sleep(1000);
            new Thread(StartClient).Start();
            while (true)
                Thread.Sleep(1000);
        }

        private static void StartServer()
        {
            enet_initialize();
            ENetHost* host = null;
            try
            {
                var address = new ENetAddress();
                enet_address_set_from_ipaddress(&address, IPAddress.IPv6Any, 7777);

                Span<char> hostName = stackalloc char[16];
                var error = enet_address_get_hostname(&address, ref hostName);

                if (error == 0)
                    Console.WriteLine(hostName.ToString());

                host = enet_host_create(&address, 4095, 0, 0, 0, ENetHostOption.ENET_HOSTOPT_IPV6_DUALMODE);
                enet_host_compress_with_range_coder(host);
                enet_host_checksum_with_crc32(host);

                ENetPeer* peer = null;

                var @event = new ENetEvent();

                while (_running)
                {
                    if (enet_host_service(host, &@event, 1) > 0)
                    {
                        while (true)
                        {
                            switch (@event.type)
                            {
                                case ENetEventType.ENET_EVENT_TYPE_CONNECT:
                                    peer = @event.peer;
                                    peer->address.ToIpEndPoint(out var endPoint);
                                    Console.WriteLine($"server Connected {endPoint}");
                                    break;
                                case ENetEventType.ENET_EVENT_TYPE_DISCONNECT:
                                    peer = null;
                                    Console.WriteLine("server Disconnected");
                                    break;
                                case ENetEventType.ENET_EVENT_TYPE_RECEIVE:
                                    if (enet_peer_send(peer, 0, @event.packet) != 0)
                                        enet_packet_destroy(@event.packet);
                                    break;
                            }

                            if (enet_host_check_events(host, &@event) <= 0)
                                break;
                        }
                    }

                    Thread.Sleep(INTERVAL);
                }
            }
            finally
            {
                if (host != null)
                    enet_host_destroy(host);
                enet_deinitialize();
            }
        }

        private static void StartClient()
        {
            enet_initialize();
            ENetHost* host = null;
            try
            {
                var address = new ENetAddress();
                enet_address_set_from_ipaddress(&address, IPAddress.Loopback, 7777);

                var local = new ENetAddress();
                enet_address_set_ip_ipv4(&local, "0.0.0.0", 7778);

                host = enet_host_create(&local, 1, 0, 0, 0, ENetHostOption.ENET_HOSTOPT_IPV4);
                enet_host_compress_with_range_coder(host);
                enet_host_checksum_with_crc32(host);

                var peer = enet_host_connect(host, &address, 0, 0);

                var @event = new ENetEvent();

                var connected = false;
                var buffer = stackalloc byte[2048];
                var sent = false;
                var reached = false;
                var count = 0;

                while (_running)
                {
                    if (enet_host_service(host, &@event, 1) > 0)
                    {
                        while (true)
                        {
                            switch (@event.type)
                            {
                                case ENetEventType.ENET_EVENT_TYPE_CONNECT:
                                    connected = true;
                                    @event.peer->address.ToIpEndPoint(out var endPoint);
                                    Console.WriteLine($"client Connected {endPoint}");
                                    break;
                                case ENetEventType.ENET_EVENT_TYPE_DISCONNECT:
                                    connected = false;
                                    Console.WriteLine("client Disconnected");
                                    break;
                                case ENetEventType.ENET_EVENT_TYPE_RECEIVE:
                                    sent = false;
                                    if ((int)@event.packet->dataLength == count)
                                    {
                                        for (var i = 0; i < count; ++i)
                                        {
                                            if (@event.packet->data[i] != buffer[i])
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine("data not same");
                                                Console.ForegroundColor = ConsoleColor.White;
                                                goto label;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("length not same");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Console.WriteLine((int)@event.packet->dataLength + " " + count);
                                    }

                                    label:
                                    enet_packet_destroy(@event.packet);
                                    break;
                            }

                            if (enet_host_check_events(host, &@event) <= 0)
                                break;
                        }
                    }

                    if (connected && !sent)
                    {
                        sent = true;
                        if (!reached)
                        {
                            count++;
                            if (count == 100)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("reached max");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                reached = true;
                            }
                        }
                        else
                        {
                            count--;
                            if (count == 1)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("reached min");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                reached = false;
                            }
                        }

                        RandomNumberGenerator.Fill(new Span<byte>(buffer, count));
                        var packet = enet_packet_create(buffer, (nuint)count, (uint)ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
                        if (enet_peer_send(peer, 0, packet) != 0)
                            enet_packet_destroy(packet);
                    }

                    Thread.Sleep(INTERVAL);
                }
            }
            finally
            {
                if (host != null)
                    enet_host_destroy(host);
                enet_deinitialize();
            }
        }
    }
}