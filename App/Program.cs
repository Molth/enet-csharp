using System.Runtime.InteropServices;
using System.Text;
using static enet.ENet;

#pragma warning disable CA1806

namespace enet
{
    public sealed unsafe class Program
    {
        private static bool _running;

        private static void Main()
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
                ENetAddress* address = (ENetAddress*)enet_malloc(sizeof(ENetAddress));
                enet_set_ip(address, "0.0.0.0");
                address->port = 7777;

                host = enet_host_create(address, 4095, 0, 0, 0);

                ENetPeer* peer = null;

                ENetEvent* netEvent = (ENetEvent*)enet_malloc(sizeof(ENetEvent));
                memset(netEvent, 0, sizeof(ENetEvent));

                bool connected = false;
                byte* buffer = stackalloc byte[1024];

                while (_running)
                {
                    if (connected)
                    {
                        var size = Encoding.UTF8.GetBytes("server", MemoryMarshal.CreateSpan(ref *buffer, 1024));
                        var packet = enet_packet_create(buffer, size, (uint)ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
                        enet_peer_send(peer, 0, packet);
                    }

                    bool polled = false;
                    while (!polled)
                    {
                        if (enet_host_check_events(host, netEvent) <= 0)
                        {
                            if (enet_host_service(host, netEvent, 1) <= 0)
                                break;
                            polled = true;
                        }

                        switch (netEvent->type)
                        {
                            case ENetEventType.ENET_EVENT_TYPE_NONE:
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_CONNECT:
                                peer = netEvent->peer;
                                connected = true;
                                Console.WriteLine("server Connected");
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_DISCONNECT:
                                peer = null;
                                connected = false;
                                Console.WriteLine("server Disconnected");
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_RECEIVE:
                                Console.WriteLine($"server Received {Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpan(ref *netEvent->packet->data, (int)netEvent->packet->dataLength))}");
                                enet_packet_destroy(netEvent->packet);
                                break;
                        }
                    }

                    Thread.Sleep(1000);
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
                ENetAddress* address = (ENetAddress*)enet_malloc(sizeof(ENetAddress));
                enet_set_ip(address, "127.0.0.1");
                address->port = 7777;

                host = enet_host_create(null, 1, 0, 0, 0);

                var peer = enet_host_connect(host, address, 0, 0);

                ENetEvent* netEvent = (ENetEvent*)enet_malloc(sizeof(ENetEvent));
                memset(netEvent, 0, sizeof(ENetEvent));

                bool connected = false;
                byte* buffer = stackalloc byte[1024];

                while (_running)
                {
                    if (connected)
                    {
                        var size = Encoding.UTF8.GetBytes("client", MemoryMarshal.CreateSpan(ref *buffer, 1024));
                        var packet = enet_packet_create(buffer, size, (uint)ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE);
                        enet_peer_send(peer, 0, packet);
                    }

                    bool polled = false;
                    while (!polled)
                    {
                        if (enet_host_check_events(host, netEvent) <= 0)
                        {
                            if (enet_host_service(host, netEvent, 1) <= 0)
                                break;
                            polled = true;
                        }

                        switch (netEvent->type)
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
                                Console.WriteLine($"client Received {Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpan(ref *netEvent->packet->data, (int)netEvent->packet->dataLength))}");
                                enet_packet_destroy(netEvent->packet);
                                break;
                        }
                    }

                    Thread.Sleep(1000);
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