using static enet.ENetSocketOption;
using static enet.ENetSocketType;
using static enet.ENetPeerState;
using static enet.ENetProtocolCommand;
using static enet.ENetProtocolFlag;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        /// <summary>
        ///     Sends a ping request to an address.
        /// </summary>
        /// <param name="host">host ping the address</param>
        /// <param name="address">destination for the ping request</param>
        public static int enet_host_ping(ENetHost* host, ENetAddress* address)
        {
            ENetBuffer buffer;
            byte* data = stackalloc byte[1];
            buffer.data = data;
            buffer.dataLength = 1;
            return enet_socket_send(host->socket, address, &buffer, 1) > 0 ? 0 : -1;
        }

        /// <summary>
        ///     Creates a host for communicating to peers.
        /// </summary>
        /// <param name="address">
        ///     The address at which other peers may connect to this host. If NULL, then no peers may connect to
        ///     the host.
        /// </param>
        /// <param name="peerCount">The maximum number of peers that should be allocated for the host.</param>
        /// <param name="channelLimit">
        ///     The maximum number of channels allowed; if 0, then this is equivalent to
        ///     ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT
        /// </param>
        /// <param name="incomingBandwidth">
        ///     Downstream bandwidth of the host in bytes/second; if 0, ENet will assume unlimited
        ///     bandwidth.
        /// </param>
        /// <param name="outgoingBandwidth">
        ///     Upstream bandwidth of the host in bytes/second; if 0, ENet will assume unlimited
        ///     bandwidth.
        /// </param>
        /// <returns>The host on success and NULL on failure</returns>
        /// <remarks>
        ///     ENet will strategically drop packets on specific sides of a connection between hosts
        ///     to ensure the host's bandwidth is not overwhelmed. The bandwidth parameters also determine
        ///     the window size of a connection which limits the amount of reliable packets that may be in transit
        ///     at any given time.
        /// </remarks>
        public static ENetHost* enet_host_create(ENetAddress* address, nuint peerCount, nuint channelLimit, uint incomingBandwidth, uint outgoingBandwidth)
        {
            ENetHost* host;
            ENetPeer* currentPeer;

            if (peerCount > ENET_PROTOCOL_MAXIMUM_PEER_ID)
                return null;

            host = (ENetHost*)enet_malloc((nuint)sizeof(ENetHost));
            memset(host, 0, (nuint)sizeof(ENetHost));

            host->peers = (ENetPeer*)enet_malloc(peerCount * (nuint)sizeof(ENetPeer));
            memset(host->peers, 0, peerCount * (nuint)sizeof(ENetPeer));

            host->socket = enet_socket_create(ENET_SOCKET_TYPE_DATAGRAM);
            if (host->socket == ENET_SOCKET_NULL || (address != null && enet_socket_bind(host->socket, address) < 0))
            {
                if (host->socket != ENET_SOCKET_NULL)
                    enet_socket_destroy(&host->socket);

                enet_free(host->peers);
                enet_free(host);

                return null;
            }

            enet_socket_set_option(host->socket, ENET_SOCKOPT_NONBLOCK, 1);
            enet_socket_set_option(host->socket, ENET_SOCKOPT_BROADCAST, 1);
            enet_socket_set_option(host->socket, ENET_SOCKOPT_RCVBUF, (int)ENET_HOST_RECEIVE_BUFFER_SIZE);
            enet_socket_set_option(host->socket, ENET_SOCKOPT_SNDBUF, (int)ENET_HOST_SEND_BUFFER_SIZE);

            if (address != null && enet_socket_get_address(host->socket, &host->address) < 0)
                host->address = *address;

            if (!(channelLimit != 0) || channelLimit > ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
                channelLimit = (nuint)ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT;
            else if (channelLimit < ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT)
                channelLimit = (nuint)ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT;

            host->randomSeed = (uint)(nuint)host;
            host->randomSeed += enet_host_random_seed();
            host->randomSeed = (host->randomSeed << 16) | (host->randomSeed >> 16);
            host->channelLimit = channelLimit;
            host->incomingBandwidth = incomingBandwidth;
            host->outgoingBandwidth = outgoingBandwidth;
            host->bandwidthThrottleEpoch = 0;
            host->recalculateBandwidthLimits = 0;
            host->mtu = ENET_HOST_DEFAULT_MTU;
            host->peerCount = peerCount;
            host->commandCount = 0;
            host->bufferCount = 0;
            host->checksum = null;
            host->receivedAddress.host = ENET_HOST_ANY;
            host->receivedAddress.port = 0;
            host->receivedData = null;
            host->receivedDataLength = 0;

            host->totalSentData = 0;
            host->totalSentPackets = 0;
            host->totalReceivedData = 0;
            host->totalReceivedPackets = 0;
            host->totalQueued = 0;

            host->connectedPeers = 0;
            host->bandwidthLimitedPeers = 0;
            host->duplicatePeers = (nuint)ENET_PROTOCOL_MAXIMUM_PEER_ID;
            host->maximumPacketSize = (nuint)ENET_HOST_DEFAULT_MAXIMUM_PACKET_SIZE;
            host->maximumWaitingData = (nuint)ENET_HOST_DEFAULT_MAXIMUM_WAITING_DATA;

            host->compressor.context = null;
            host->compressor.compress = null;
            host->compressor.decompress = null;
            host->compressor.destroy = null;

            host->intercept = null;

            enet_list_clear(&host->dispatchQueue);

            for (currentPeer = host->peers;
                 currentPeer < &host->peers[host->peerCount];
                 ++currentPeer)
            {
                currentPeer->host = host;
                currentPeer->incomingPeerID = (ushort)(currentPeer - host->peers);
                currentPeer->outgoingSessionID = currentPeer->incomingSessionID = 0xFF;
                currentPeer->data = null;

                enet_list_clear(&currentPeer->acknowledgements);
                enet_list_clear(&currentPeer->sentReliableCommands);
                enet_list_clear(&currentPeer->outgoingCommands);
                enet_list_clear(&currentPeer->outgoingSendReliableCommands);
                enet_list_clear(&currentPeer->dispatchedCommands);

                enet_peer_reset(currentPeer);
            }

            return host;
        }

        /// <summary>
        ///     Destroys the host and all resources associated with it.
        /// </summary>
        /// <param name="host">pointer to the host to destroy</param>
        public static void enet_host_destroy(ENetHost* host)
        {
            ENetPeer* currentPeer;

            if (host == null)
                return;

            enet_socket_destroy(&host->socket);

            for (currentPeer = host->peers;
                 currentPeer < &host->peers[host->peerCount];
                 ++currentPeer)
            {
                enet_peer_reset(currentPeer);
            }

            if (host->compressor.context != null && host->compressor.destroy != null)
                (host->compressor.destroy)(host->compressor.context);

            enet_free(host->peers);
            enet_free(host);
        }

        public static uint enet_host_random(ENetHost* host)
        {
            uint n = (host->randomSeed += 0x6D2B79F5U);
            n = (n ^ (n >> 15)) * (n | 1U);
            n ^= n + (n ^ (n >> 7)) * (n | 61U);
            return n ^ (n >> 14);
        }

        /// <summary>
        ///     Initiates a connection to a foreign host.
        /// </summary>
        /// <param name="host">host seeking the connection</param>
        /// <param name="address">destination for the connection</param>
        /// <param name="channelCount">number of channels to allocate</param>
        /// <param name="data">user data supplied to the receiving host</param>
        /// <returns>a peer representing the foreign host on success, NULL on failure</returns>
        /// <remarks>
        ///     The peer returned will have not completed the connection until enet_host_service()
        ///     notifies of an ENET_EVENT_TYPE_CONNECT event for the peer.
        /// </remarks>
        public static ENetPeer* enet_host_connect(ENetHost* host, ENetAddress* address, nuint channelCount, uint data)
        {
            ENetPeer* currentPeer;
            ENetChannel* channel;
            ENetProtocol command;

            if (channelCount < ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT)
                channelCount = (nuint)ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT;
            else if (channelCount > ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
                channelCount = (nuint)ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT;

            for (currentPeer = host->peers;
                 currentPeer < &host->peers[host->peerCount];
                 ++currentPeer)
            {
                if (currentPeer->state == ENET_PEER_STATE_DISCONNECTED)
                    break;
            }

            if (currentPeer >= &host->peers[host->peerCount])
                return null;

            currentPeer->channels = (ENetChannel*)enet_malloc(channelCount * (nuint)sizeof(ENetChannel));

            currentPeer->channelCount = channelCount;
            currentPeer->state = ENET_PEER_STATE_CONNECTING;
            currentPeer->address = *address;
            currentPeer->connectID = enet_host_random(host);
            currentPeer->mtu = host->mtu;

            if (host->outgoingBandwidth == 0)
                currentPeer->windowSize = ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE;
            else
                currentPeer->windowSize = (host->outgoingBandwidth /
                                           ENET_PEER_WINDOW_SIZE_SCALE) *
                                          ENET_PROTOCOL_MINIMUM_WINDOW_SIZE;

            if (currentPeer->windowSize < ENET_PROTOCOL_MINIMUM_WINDOW_SIZE)
                currentPeer->windowSize = ENET_PROTOCOL_MINIMUM_WINDOW_SIZE;
            else if (currentPeer->windowSize > ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE)
                currentPeer->windowSize = ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE;

            for (channel = currentPeer->channels;
                 channel < &currentPeer->channels[channelCount];
                 ++channel)
            {
                channel->outgoingReliableSequenceNumber = 0;
                channel->outgoingUnreliableSequenceNumber = 0;
                channel->incomingReliableSequenceNumber = 0;
                channel->incomingUnreliableSequenceNumber = 0;

                enet_list_clear(&channel->incomingReliableCommands);
                enet_list_clear(&channel->incomingUnreliableCommands);

                channel->usedReliableWindows = 0;
                memset(channel->reliableWindows, 0, (nuint)(ENET_PEER_RELIABLE_WINDOWS * sizeof(ushort)));
            }

            command.header.command = (byte)((uint)ENET_PROTOCOL_COMMAND_CONNECT | (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE);
            command.header.channelID = 0xFF;
            command.connect.outgoingPeerID = ENET_HOST_TO_NET_16(currentPeer->incomingPeerID);
            command.connect.incomingSessionID = currentPeer->incomingSessionID;
            command.connect.outgoingSessionID = currentPeer->outgoingSessionID;
            command.connect.mtu = ENET_HOST_TO_NET_32(currentPeer->mtu);
            command.connect.windowSize = ENET_HOST_TO_NET_32(currentPeer->windowSize);
            command.connect.channelCount = ENET_HOST_TO_NET_32((uint)channelCount);
            command.connect.incomingBandwidth = ENET_HOST_TO_NET_32(host->incomingBandwidth);
            command.connect.outgoingBandwidth = ENET_HOST_TO_NET_32(host->outgoingBandwidth);
            command.connect.packetThrottleInterval = ENET_HOST_TO_NET_32(currentPeer->packetThrottleInterval);
            command.connect.packetThrottleAcceleration = ENET_HOST_TO_NET_32(currentPeer->packetThrottleAcceleration);
            command.connect.packetThrottleDeceleration = ENET_HOST_TO_NET_32(currentPeer->packetThrottleDeceleration);
            command.connect.connectID = currentPeer->connectID;
            command.connect.data = ENET_HOST_TO_NET_32(data);

            enet_peer_queue_outgoing_command(currentPeer, &command, null, 0, 0);

            return currentPeer;
        }

        /// <summary>
        ///     Queues a packet to be sent to all peers associated with the host.
        /// </summary>
        /// <param name="host">host on which to broadcast the packet</param>
        /// <param name="channelID">channel on which to broadcast</param>
        /// <param name="packet">packet to broadcast</param>
        public static void enet_host_broadcast(ENetHost* host, byte channelID, ENetPacket* packet)
        {
            ENetPeer* currentPeer;

            for (currentPeer = host->peers;
                 currentPeer < &host->peers[host->peerCount];
                 ++currentPeer)
            {
                if (currentPeer->state != ENET_PEER_STATE_CONNECTED)
                    continue;

                enet_peer_send(currentPeer, channelID, packet);
            }

            if (packet->referenceCount == 0)
                enet_packet_destroy(packet);
        }

        /// <summary>
        ///     Sets the packet compressor the host should use to compress and decompress packets.
        /// </summary>
        /// <param name="host">host to enable or disable compression for</param>
        /// <param name="compressor">callbacks for for the packet compressor; if NULL, then compression is disabled</param>
        public static void enet_host_compress(ENetHost* host, ENetCompressor* compressor)
        {
            if (host->compressor.context != null && host->compressor.destroy != null)
                (host->compressor.destroy)(host->compressor.context);

            if (compressor != null)
                host->compressor = *compressor;
            else
                host->compressor.context = null;
        }

        /// <summary>
        ///     Limits the maximum allowed channels of future incoming connections.
        /// </summary>
        /// <param name="host">host to limit</param>
        /// <param name="channelLimit">
        ///     the maximum number of channels allowed; if 0, then this is equivalent to
        ///     ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT
        /// </param>
        public static void enet_host_channel_limit(ENetHost* host, nuint channelLimit)
        {
            if (!(channelLimit != 0) || channelLimit > ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
                channelLimit = (nuint)ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT;
            else if (channelLimit < ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT)
                channelLimit = (nuint)ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT;

            host->channelLimit = channelLimit;
        }

        /// <summary>
        ///     Adjusts the bandwidth limits of a host.
        /// </summary>
        /// <param name="host">host to adjust</param>
        /// <param name="incomingBandwidth">new incoming bandwidth</param>
        /// <param name="outgoingBandwidth">new outgoing bandwidth</param>
        /// <remarks>
        ///     the incoming and outgoing bandwidth parameters are identical in function to those
        ///     specified in <see cref="enet_host_create(ENetAddress*, nuint, nuint, uint, uint)" />.
        /// </remarks>
        public static void enet_host_bandwidth_limit(ENetHost* host, uint incomingBandwidth, uint outgoingBandwidth)
        {
            host->incomingBandwidth = incomingBandwidth;
            host->outgoingBandwidth = outgoingBandwidth;
            host->recalculateBandwidthLimits = 1;
        }

        public static void enet_host_bandwidth_throttle(ENetHost* host)
        {
            uint timeCurrent = enet_time_get(),
                elapsedTime = timeCurrent - host->bandwidthThrottleEpoch,
                peersRemaining = (uint)host->connectedPeers,
                dataTotal = unchecked((uint)(~0)),
                bandwidth = unchecked((uint)(~0)),
                throttle = 0,
                bandwidthLimit = 0;
            int needsAdjustment = host->bandwidthLimitedPeers > 0 ? 1 : 0;
            ENetPeer* peer;
            ENetProtocol command;

            if (elapsedTime < ENET_HOST_BANDWIDTH_THROTTLE_INTERVAL)
                return;

            host->bandwidthThrottleEpoch = timeCurrent;

            if (peersRemaining == 0)
                return;

            if (host->outgoingBandwidth != 0)
            {
                dataTotal = 0;
                bandwidth = (host->outgoingBandwidth * elapsedTime) / 1000;

                for (peer = host->peers;
                     peer < &host->peers[host->peerCount];
                     ++peer)
                {
                    if (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER)
                        continue;

                    dataTotal += peer->outgoingDataTotal;
                }
            }

            while (peersRemaining > 0 && needsAdjustment != 0)
            {
                needsAdjustment = 0;

                if (dataTotal <= bandwidth)
                    throttle = ENET_PEER_PACKET_THROTTLE_SCALE;
                else
                    throttle = (bandwidth * ENET_PEER_PACKET_THROTTLE_SCALE) / dataTotal;

                for (peer = host->peers;
                     peer < &host->peers[host->peerCount];
                     ++peer)
                {
                    uint peerBandwidth;

                    if ((peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER) ||
                        peer->incomingBandwidth == 0 ||
                        peer->outgoingBandwidthThrottleEpoch == timeCurrent)
                        continue;

                    peerBandwidth = (peer->incomingBandwidth * elapsedTime) / 1000;
                    if ((throttle * peer->outgoingDataTotal) / ENET_PEER_PACKET_THROTTLE_SCALE <= peerBandwidth)
                        continue;

                    peer->packetThrottleLimit = (peerBandwidth *
                                                 ENET_PEER_PACKET_THROTTLE_SCALE) / peer->outgoingDataTotal;

                    if (peer->packetThrottleLimit == 0)
                        peer->packetThrottleLimit = 1;

                    if (peer->packetThrottle > peer->packetThrottleLimit)
                        peer->packetThrottle = peer->packetThrottleLimit;

                    peer->outgoingBandwidthThrottleEpoch = timeCurrent;

                    peer->incomingDataTotal = 0;
                    peer->outgoingDataTotal = 0;

                    needsAdjustment = 1;
                    --peersRemaining;
                    bandwidth -= peerBandwidth;
                    dataTotal -= peerBandwidth;
                }
            }

            if (peersRemaining > 0)
            {
                if (dataTotal <= bandwidth)
                    throttle = ENET_PEER_PACKET_THROTTLE_SCALE;
                else
                    throttle = (bandwidth * ENET_PEER_PACKET_THROTTLE_SCALE) / dataTotal;

                for (peer = host->peers;
                     peer < &host->peers[host->peerCount];
                     ++peer)
                {
                    if ((peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER) ||
                        peer->outgoingBandwidthThrottleEpoch == timeCurrent)
                        continue;

                    peer->packetThrottleLimit = throttle;

                    if (peer->packetThrottle > peer->packetThrottleLimit)
                        peer->packetThrottle = peer->packetThrottleLimit;

                    peer->incomingDataTotal = 0;
                    peer->outgoingDataTotal = 0;
                }
            }

            if ((host->recalculateBandwidthLimits) != 0)
            {
                host->recalculateBandwidthLimits = 0;

                peersRemaining = (uint)host->connectedPeers;
                bandwidth = host->incomingBandwidth;
                needsAdjustment = 1;

                if (bandwidth == 0)
                    bandwidthLimit = 0;
                else
                {
                    while (peersRemaining > 0 && needsAdjustment != 0)
                    {
                        needsAdjustment = 0;
                        bandwidthLimit = bandwidth / peersRemaining;

                        for (peer = host->peers;
                             peer < &host->peers[host->peerCount];
                             ++peer)
                        {
                            if ((peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER) ||
                                peer->incomingBandwidthThrottleEpoch == timeCurrent)
                                continue;

                            if (peer->outgoingBandwidth > 0 &&
                                peer->outgoingBandwidth >= bandwidthLimit)
                                continue;

                            peer->incomingBandwidthThrottleEpoch = timeCurrent;

                            needsAdjustment = 1;
                            --peersRemaining;
                            bandwidth -= peer->outgoingBandwidth;
                        }
                    }
                }

                for (peer = host->peers;
                     peer < &host->peers[host->peerCount];
                     ++peer)
                {
                    if (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER)
                        continue;

                    command.header.command = (byte)((uint)ENET_PROTOCOL_COMMAND_BANDWIDTH_LIMIT | (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE);
                    command.header.channelID = 0xFF;
                    command.bandwidthLimit.outgoingBandwidth = ENET_HOST_TO_NET_32(host->outgoingBandwidth);

                    if (peer->incomingBandwidthThrottleEpoch == timeCurrent)
                        command.bandwidthLimit.incomingBandwidth = ENET_HOST_TO_NET_32(peer->outgoingBandwidth);
                    else
                        command.bandwidthLimit.incomingBandwidth = ENET_HOST_TO_NET_32(bandwidthLimit);

                    enet_peer_queue_outgoing_command(peer, &command, null, 0, 0);
                }
            }
        }
    }
}