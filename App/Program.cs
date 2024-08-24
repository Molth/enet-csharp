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
                var address = (ENetAddress*)enet_malloc(sizeof(ENetAddress));
                enet_set_ip(address, "0.0.0.0");
                address->port = 7777;

                host = enet_host_create(address, 4095, 0, 0, 0);

                var netEvent = (ENetEvent*)enet_malloc(sizeof(ENetEvent));
                memset(netEvent, 0, sizeof(ENetEvent));

                while (_running)
                {
                    var polled = false;
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
                                Console.WriteLine("1 Connected");
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_DISCONNECT:
                                Console.WriteLine("1 Disconnected");
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_RECEIVE:
                                Console.WriteLine("1 Received");
                                break;
                        }
                    }

                    Thread.Sleep(1);
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
                var address = (ENetAddress*)enet_malloc(sizeof(ENetAddress));
                enet_set_ip(address, "127.0.0.1");
                address->port = 7777;

                host = enet_host_create(null, 1, 0, 0, 0);

                enet_host_connect(host, address, 0, 0);

                var netEvent = (ENetEvent*)enet_malloc(sizeof(ENetEvent));
                memset(netEvent, 0, sizeof(ENetEvent));

                while (_running)
                {
                    var polled = false;
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
                                Console.WriteLine("2 Connected");
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_DISCONNECT:
                                Console.WriteLine("2 Disconnected");
                                break;
                            case ENetEventType.ENET_EVENT_TYPE_RECEIVE:
                                Console.WriteLine("2 Received");
                                break;
                        }
                    }

                    Thread.Sleep(1);
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