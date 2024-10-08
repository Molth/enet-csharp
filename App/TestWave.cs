﻿using System.Security.Cryptography;
using static enet.ENet;

#pragma warning disable CA1806

// ReSharper disable ALL

namespace enet
{
    public sealed unsafe class TestWave
    {
        private static bool _running;

        public const int INTERVAL = 1;

        public static void Start()
        {
            Console.CancelKeyPress += (sender, args) => _running = false;
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
                ENetAddress address = new ENetAddress();
                enet_set_ip(&address, "0.0.0.0");
                address.port = 7777;

                host = enet_host_create(&address, 4095, 0, 0, 0);

                ENetPeer* peer = null;

                ENetEvent netEvent = new ENetEvent();

                byte* buffer = stackalloc byte[1024];

                while (_running)
                {
                    bool polled = false;
                    while (!polled)
                    {
                        if (enet_host_check_events(host, &netEvent) <= 0)
                        {
                            if (enet_host_service(host, &netEvent, 1) <= 0)
                                break;
                            polled = true;
                        }

                        switch (netEvent.type)
                        {
                            case ENetEventType.ENET_EVENT_TYPE_NONE:
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_CONNECT:
                                peer = netEvent.peer;
                                Console.WriteLine($"server Connected {peer->address.host.ToString()}");
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_DISCONNECT:
                                peer = null;
                                Console.WriteLine("server Disconnected");
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_RECEIVE:
                                enet_peer_send(peer, 0, netEvent.packet);
                                break;
                        }
                    }

                    enet_host_flush(host);
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
                ENetAddress address = new ENetAddress();
                enet_set_ip(&address, "127.0.0.1");
                address.port = 7777;

                host = enet_host_create(null, 1, 0, 0, 0);

                ENetPeer* peer = enet_host_connect(host, &address, 0, 0);

                ENetEvent netEvent = new ENetEvent();

                bool connected = false;
                byte* buffer = stackalloc byte[2048];
                var sent = false;
                var reached = false;
                var count = 0;

                while (_running)
                {
                    bool polled = false;
                    while (!polled)
                    {
                        if (enet_host_check_events(host, &netEvent) <= 0)
                        {
                            if (enet_host_service(host, &netEvent, 1) <= 0)
                                break;
                            polled = true;
                        }

                        switch (netEvent.type)
                        {
                            case ENetEventType.ENET_EVENT_TYPE_NONE:
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_CONNECT:
                                connected = true;
                                Console.WriteLine("client Connected");
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_DISCONNECT:
                                connected = false;
                                Console.WriteLine("client Disconnected");
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_RECEIVE:
                                sent = false;
                                if ((int)netEvent.packet->dataLength == count)
                                {
                                    for (int i = 0; i < count; ++i)
                                    {
                                        if (netEvent.packet->data[i] != buffer[i])
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("data not same");
                                            Console.ForegroundColor = ConsoleColor.White;
                                            goto label;
                                        }
                                    }

                                    Console.WriteLine("same");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"length not same");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine((int)netEvent.packet->dataLength + " " + count);
                                }

                                label:
                                enet_packet_destroy(netEvent.packet);
                                break;
                        }
                    }

                    if (connected && !sent)
                    {
                        sent = true;
                        if (!reached)
                        {
                            count++;
                            if (count == 1200)
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
                        ENetPacket* packet = enet_packet_create(buffer, count, (uint)ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
                        enet_peer_send(peer, 0, packet);
                    }

                    enet_host_flush(host);
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