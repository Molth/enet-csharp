using static enet.ENetPeerState;
using static enet.ENetProtocolCommand;
using static enet.ENetProtocolFlag;
using static enet.ENetPacketFlag;
using static enet.ENetPeerFlag;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        /// <summary>
        ///     Configures throttle parameter for a peer.
        /// </summary>
        /// <remarks>
        ///     Unreliable packets are dropped by ENet in response to the varying conditions
        ///     of the Internet connection to the peer. The throttle represents a probability
        ///     that an unreliable packet should not be dropped and thus sent by ENet to the peer.
        ///     The lowest mean round trip time from the sending of a reliable packet to the
        ///     receipt of its acknowledgement is measured over an amount of time specified by
        ///     the interval parameter in milliseconds. If a measured round trip time happens to
        ///     be significantly less than the mean round trip time measured over the interval,
        ///     then the throttle probability is increased to allow more traffic by an amount
        ///     specified in the acceleration parameter, which is a ratio to the ENET_PEER_PACKET_THROTTLE_SCALE
        ///     constant. If a measured round trip time happens to be significantly greater than
        ///     the mean round trip time measured over the interval, then the throttle probability
        ///     is decreased to limit traffic by an amount specified in the deceleration parameter, which
        ///     is a ratio to the ENET_PEER_PACKET_THROTTLE_SCALE constant. When the throttle has
        ///     a value of ENET_PEER_PACKET_THROTTLE_SCALE, no unreliable packets are dropped by
        ///     ENet, and so 100% of all unreliable packets will be sent. When the throttle has a
        ///     value of 0, all unreliable packets are dropped by ENet, and so 0% of all unreliable
        ///     packets will be sent. Intermediate values for the throttle represent intermediate
        ///     probabilities between 0% and 100% of unreliable packets being sent. The bandwidth
        ///     limits of the local and foreign hosts are taken into account to determine a
        ///     sensible limit for the throttle probability above which it should not raise even in
        ///     the best of conditions.
        /// </remarks>
        /// <param name="peer">peer to configure</param>
        /// <param name="interval">
        ///     interval, in milliseconds, over which to measure lowest mean RTT; the default value is
        ///     ENET_PEER_PACKET_THROTTLE_INTERVAL.
        /// </param>
        /// <param name="acceleration">rate at which to increase the throttle probability as mean RTT declines</param>
        /// <param name="deceleration">rate at which to decrease the throttle probability as mean RTT increases</param>
        public static void enet_peer_throttle_configure(ENetPeer* peer, uint interval, uint acceleration, uint deceleration)
        {
            ENetProtocol command;

            peer->packetThrottleInterval = interval;
            peer->packetThrottleAcceleration = acceleration;
            peer->packetThrottleDeceleration = deceleration;

            command.header.command = (byte)((uint)ENET_PROTOCOL_COMMAND_THROTTLE_CONFIGURE | (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE);
            command.header.channelID = 0xFF;

            command.throttleConfigure.packetThrottleInterval = ENET_HOST_TO_NET_32(interval);
            command.throttleConfigure.packetThrottleAcceleration = ENET_HOST_TO_NET_32(acceleration);
            command.throttleConfigure.packetThrottleDeceleration = ENET_HOST_TO_NET_32(deceleration);

            enet_peer_queue_outgoing_command(peer, &command, null, 0, 0);
        }

        public static int enet_peer_throttle(ENetPeer* peer, uint rtt)
        {
            if (peer->lastRoundTripTime <= peer->lastRoundTripTimeVariance)
            {
                peer->packetThrottle = peer->packetThrottleLimit;
            }
            else if (rtt <= peer->lastRoundTripTime)
            {
                peer->packetThrottle += peer->packetThrottleAcceleration;

                if (peer->packetThrottle > peer->packetThrottleLimit)
                    peer->packetThrottle = peer->packetThrottleLimit;

                return 1;
            }
            else if (rtt > peer->lastRoundTripTime + 2 * peer->lastRoundTripTimeVariance)
            {
                if (peer->packetThrottle > peer->packetThrottleDeceleration)
                    peer->packetThrottle -= peer->packetThrottleDeceleration;
                else
                    peer->packetThrottle = 0;

                return -1;
            }

            return 0;
        }

        /// <summary>
        ///     Queues a packet to be sent.
        /// </summary>
        /// <remarks>
        ///     On success, ENet will assume ownership of the packet, and so enet_packet_destroy
        ///     should not be called on it thereafter. On failure, the caller still must destroy
        ///     the packet on its own as ENet has not queued the packet. The caller can also
        ///     check the packet's referenceCount field after sending to check if ENet queued
        ///     the packet and thus incremented the referenceCount.
        /// </remarks>
        /// <param name="peer">destination for the packet</param>
        /// <param name="channelID">channel on which to send</param>
        /// <param name="packet">packet to send</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public static int enet_peer_send(ENetPeer* peer, byte channelID, ENetPacket* packet)
        {
            ENetChannel* channel;
            ENetProtocol command;
            nuint fragmentLength;

            if (peer->state != ENET_PEER_STATE_CONNECTED ||
                channelID >= peer->channelCount ||
                packet->dataLength > peer->host->maximumPacketSize)
                return -1;

            channel = &peer->channels[channelID];
            fragmentLength = (nuint)(peer->mtu - sizeof(ENetProtocolHeader) - sizeof(ENetProtocolSendFragment));
            if (peer->host->checksum != null)
                fragmentLength -= sizeof(uint);

            if (packet->dataLength > fragmentLength)
            {
                uint fragmentCount = (uint)((packet->dataLength + fragmentLength - 1) / fragmentLength),
                    fragmentNumber,
                    fragmentOffset;
                byte commandNumber;
                ushort startSequenceNumber;
                ENetList fragments;
                ENetOutgoingCommand* fragment;

                if (fragmentCount > ENET_PROTOCOL_MAXIMUM_FRAGMENT_COUNT)
                    return -1;

                if ((packet->flags & ((uint)ENET_PACKET_FLAG_RELIABLE | (uint)ENET_PACKET_FLAG_UNRELIABLE_FRAGMENT)) == (uint)ENET_PACKET_FLAG_UNRELIABLE_FRAGMENT &&
                    channel->outgoingUnreliableSequenceNumber < 0xFFFF)
                {
                    commandNumber = (byte)ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE_FRAGMENT;
                    startSequenceNumber = ENET_HOST_TO_NET_16((ushort)(channel->outgoingUnreliableSequenceNumber + 1));
                }
                else
                {
                    commandNumber = (byte)((uint)ENET_PROTOCOL_COMMAND_SEND_FRAGMENT | (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE);
                    startSequenceNumber = ENET_HOST_TO_NET_16((ushort)(channel->outgoingReliableSequenceNumber + 1));
                }

                enet_list_clear(&fragments);

                for (fragmentNumber = 0,
                     fragmentOffset = 0;
                     fragmentOffset < packet->dataLength;
                     ++fragmentNumber,
                     fragmentOffset += (uint)fragmentLength)
                {
                    if (packet->dataLength - fragmentOffset < fragmentLength)
                        fragmentLength = (nuint)(packet->dataLength - fragmentOffset);

                    fragment = (ENetOutgoingCommand*)enet_malloc((nuint)sizeof(ENetOutgoingCommand));

                    fragment->fragmentOffset = fragmentOffset;
                    fragment->fragmentLength = (ushort)fragmentLength;
                    fragment->packet = packet;
                    fragment->command.header.command = commandNumber;
                    fragment->command.header.channelID = channelID;
                    fragment->command.sendFragment.startSequenceNumber = startSequenceNumber;
                    fragment->command.sendFragment.dataLength = ENET_HOST_TO_NET_16((ushort)fragmentLength);
                    fragment->command.sendFragment.fragmentCount = ENET_HOST_TO_NET_32(fragmentCount);
                    fragment->command.sendFragment.fragmentNumber = ENET_HOST_TO_NET_32(fragmentNumber);
                    fragment->command.sendFragment.totalLength = ENET_HOST_TO_NET_32((uint)packet->dataLength);
                    fragment->command.sendFragment.fragmentOffset = ENET_NET_TO_HOST_32(fragmentOffset);

                    enet_list_insert(enet_list_end(&fragments), fragment);
                }

                packet->referenceCount += (nuint)fragmentNumber;

                while (!enet_list_empty(&fragments))
                {
                    fragment = (ENetOutgoingCommand*)enet_list_remove(enet_list_begin(&fragments));

                    enet_peer_setup_outgoing_command(peer, fragment);
                }

                return 0;
            }

            command.header.channelID = channelID;

            if ((packet->flags & ((uint)ENET_PACKET_FLAG_RELIABLE | (uint)ENET_PACKET_FLAG_UNSEQUENCED)) == (uint)ENET_PACKET_FLAG_UNSEQUENCED)
            {
                command.header.command = (byte)((uint)ENET_PROTOCOL_COMMAND_SEND_UNSEQUENCED | (uint)ENET_PROTOCOL_COMMAND_FLAG_UNSEQUENCED);
                command.sendUnsequenced.dataLength = ENET_HOST_TO_NET_16((ushort)packet->dataLength);
            }
            else if (((packet->flags & (uint)ENET_PACKET_FLAG_RELIABLE) != 0) || channel->outgoingUnreliableSequenceNumber >= 0xFFFF)
            {
                command.header.command = (byte)((uint)ENET_PROTOCOL_COMMAND_SEND_RELIABLE | (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE);
                command.sendReliable.dataLength = ENET_HOST_TO_NET_16((ushort)packet->dataLength);
            }
            else
            {
                command.header.command = (byte)ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE;
                command.sendUnreliable.dataLength = ENET_HOST_TO_NET_16((ushort)packet->dataLength);
            }

            if (enet_peer_queue_outgoing_command(peer, &command, packet, 0, (ushort)packet->dataLength) == null)
                return -1;

            return 0;
        }

        /// <summary>
        ///     Attempts to dequeue any incoming queued packet.
        /// </summary>
        /// <param name="peer">peer to dequeue packets from</param>
        /// <param name="channelID">holds the channel ID of the channel the packet was received on success</param>
        /// <returns>a pointer to the packet, or NULL if there are no available incoming queued packets</returns>
        public static ENetPacket* enet_peer_receive(ENetPeer* peer, byte* channelID)
        {
            ENetIncomingCommand* incomingCommand;
            ENetPacket* packet;

            if (enet_list_empty(&peer->dispatchedCommands))
                return null;

            incomingCommand = (ENetIncomingCommand*)enet_list_remove(enet_list_begin(&peer->dispatchedCommands));

            if (channelID != null)
                *channelID = incomingCommand->command.header.channelID;

            packet = incomingCommand->packet;

            --packet->referenceCount;

            if (incomingCommand->fragments != null)
                enet_free(incomingCommand->fragments);

            enet_free(incomingCommand);

            peer->totalWaitingData -= (nuint)ENET_MIN((uint)peer->totalWaitingData, (uint)packet->dataLength);

            return packet;
        }

        public static void enet_peer_reset_outgoing_commands(ENetPeer* peer, ENetList* queue)
        {
            ENetOutgoingCommand* outgoingCommand;

            while (!enet_list_empty(queue))
            {
                outgoingCommand = (ENetOutgoingCommand*)enet_list_remove(enet_list_begin(queue));

                if (outgoingCommand->packet != null)
                {
                    --outgoingCommand->packet->referenceCount;

                    if (outgoingCommand->packet->referenceCount == 0)
                        enet_packet_destroy(outgoingCommand->packet);
                }

                enet_free(outgoingCommand);
            }
        }

        public static void enet_peer_remove_incoming_commands(ENetPeer* peer, ENetList* queue, ENetListNode* startCommand, ENetListNode* endCommand, ENetIncomingCommand* excludeCommand)
        {
            ENetListNode* currentCommand;

            for (currentCommand = startCommand; currentCommand != endCommand;)
            {
                ENetIncomingCommand* incomingCommand = (ENetIncomingCommand*)currentCommand;

                currentCommand = enet_list_next(currentCommand);

                if (incomingCommand == excludeCommand)
                    continue;

                enet_list_remove(&incomingCommand->incomingCommandList);

                if (incomingCommand->packet != null)
                {
                    --incomingCommand->packet->referenceCount;

                    peer->totalWaitingData -= (nuint)ENET_MIN((uint)peer->totalWaitingData, (uint)incomingCommand->packet->dataLength);

                    if (incomingCommand->packet->referenceCount == 0)
                        enet_packet_destroy(incomingCommand->packet);
                }

                if (incomingCommand->fragments != null)
                    enet_free(incomingCommand->fragments);

                enet_free(incomingCommand);
            }
        }

        public static void enet_peer_reset_incoming_commands(ENetPeer* peer, ENetList* queue)
        {
            enet_peer_remove_incoming_commands(peer, queue, enet_list_begin(queue), enet_list_end(queue), null);
        }

        public static void enet_peer_reset_queues(ENetPeer* peer)
        {
            ENetChannel* channel;

            if ((peer->flags & (uint)ENET_PEER_FLAG_NEEDS_DISPATCH) != 0)
            {
                enet_list_remove(&peer->dispatchList);

                uint flags = peer->flags;
                flags &= ~ ((uint)ENET_PEER_FLAG_NEEDS_DISPATCH);
                peer->flags = (ushort)flags;
            }

            while (!enet_list_empty(&peer->acknowledgements))
                enet_free(enet_list_remove(enet_list_begin(&peer->acknowledgements)));

            enet_peer_reset_outgoing_commands(peer, &peer->sentReliableCommands);
            enet_peer_reset_outgoing_commands(peer, &peer->outgoingCommands);
            enet_peer_reset_outgoing_commands(peer, &peer->outgoingSendReliableCommands);
            enet_peer_reset_incoming_commands(peer, &peer->dispatchedCommands);

            if (peer->channels != null && peer->channelCount > 0)
            {
                for (channel = peer->channels;
                     channel < &peer->channels[peer->channelCount];
                     ++channel)
                {
                    enet_peer_reset_incoming_commands(peer, &channel->incomingReliableCommands);
                    enet_peer_reset_incoming_commands(peer, &channel->incomingUnreliableCommands);
                }

                enet_free(peer->channels);
            }

            peer->channels = null;
            peer->channelCount = 0;
        }

        public static void enet_peer_on_connect(ENetPeer* peer)
        {
            if (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER)
            {
                if (peer->incomingBandwidth != 0)
                    ++peer->host->bandwidthLimitedPeers;

                ++peer->host->connectedPeers;
            }
        }

        public static void enet_peer_on_disconnect(ENetPeer* peer)
        {
            if (peer->state == ENET_PEER_STATE_CONNECTED || peer->state == ENET_PEER_STATE_DISCONNECT_LATER)
            {
                if (peer->incomingBandwidth != 0)
                    --peer->host->bandwidthLimitedPeers;

                --peer->host->connectedPeers;
            }
        }

        /// <summary>
        ///     Forcefully disconnects a peer.
        /// </summary>
        /// <param name="peer">peer to forcefully disconnect</param>
        /// <remarks>
        ///     The foreign host represented by the peer is not notified of the disconnection and will timeout
        ///     on its connection to the local host.
        /// </remarks>
        public static void enet_peer_reset(ENetPeer* peer)
        {
            enet_peer_on_disconnect(peer);

            peer->outgoingPeerID = (ushort)ENET_PROTOCOL_MAXIMUM_PEER_ID;
            peer->connectID = 0;

            peer->state = ENET_PEER_STATE_DISCONNECTED;

            peer->incomingBandwidth = 0;
            peer->outgoingBandwidth = 0;
            peer->incomingBandwidthThrottleEpoch = 0;
            peer->outgoingBandwidthThrottleEpoch = 0;
            peer->incomingDataTotal = 0;
            peer->outgoingDataTotal = 0;
            peer->lastSendTime = 0;
            peer->lastReceiveTime = 0;
            peer->nextTimeout = 0;
            peer->earliestTimeout = 0;
            peer->packetLossEpoch = 0;
            peer->packetsSent = 0;
            peer->packetsLost = 0;
            peer->packetLoss = 0;
            peer->packetLossVariance = 0;
            peer->packetThrottle = ENET_PEER_DEFAULT_PACKET_THROTTLE;
            peer->packetThrottleLimit = ENET_PEER_PACKET_THROTTLE_SCALE;
            peer->packetThrottleCounter = 0;
            peer->packetThrottleEpoch = 0;
            peer->packetThrottleAcceleration = ENET_PEER_PACKET_THROTTLE_ACCELERATION;
            peer->packetThrottleDeceleration = ENET_PEER_PACKET_THROTTLE_DECELERATION;
            peer->packetThrottleInterval = ENET_PEER_PACKET_THROTTLE_INTERVAL;
            peer->pingInterval = ENET_PEER_PING_INTERVAL;
            peer->timeoutLimit = ENET_PEER_TIMEOUT_LIMIT;
            peer->timeoutMinimum = ENET_PEER_TIMEOUT_MINIMUM;
            peer->timeoutMaximum = ENET_PEER_TIMEOUT_MAXIMUM;
            peer->lastRoundTripTime = ENET_PEER_DEFAULT_ROUND_TRIP_TIME;
            peer->lowestRoundTripTime = ENET_PEER_DEFAULT_ROUND_TRIP_TIME;
            peer->lastRoundTripTimeVariance = 0;
            peer->highestRoundTripTimeVariance = 0;
            peer->roundTripTime = ENET_PEER_DEFAULT_ROUND_TRIP_TIME;
            peer->roundTripTimeVariance = 0;
            peer->mtu = peer->host->mtu;
            peer->reliableDataInTransit = 0;
            peer->outgoingReliableSequenceNumber = 0;
            peer->windowSize = ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE;
            peer->incomingUnsequencedGroup = 0;
            peer->outgoingUnsequencedGroup = 0;
            peer->eventData = 0;
            peer->totalWaitingData = 0;
            peer->flags = 0;

            memset(peer->unsequencedWindow, 0, (nuint)(ENET_PEER_UNSEQUENCED_WINDOW_SIZE / 32 * sizeof(uint)));

            enet_peer_reset_queues(peer);
        }

        /// <summary>
        ///     Sends a ping request to a peer.
        /// </summary>
        /// <param name="peer">destination for the ping request</param>
        /// <remarks>
        ///     ping requests factor into the mean round trip time as designated by the
        ///     roundTripTime field in the ENetPeer structure. ENet automatically pings all connected
        ///     peers at regular intervals, however, this function may be called to ensure more
        ///     frequent ping requests.
        /// </remarks>
        public static void enet_peer_ping(ENetPeer* peer)
        {
            ENetProtocol command;

            if (peer->state != ENET_PEER_STATE_CONNECTED)
                return;

            command.header.command = (byte)((uint)ENET_PROTOCOL_COMMAND_PING | (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE);
            command.header.channelID = 0xFF;

            enet_peer_queue_outgoing_command(peer, &command, null, 0, 0);
        }

        /// <summary>
        ///     Sets the interval at which pings will be sent to a peer.
        /// </summary>
        /// <remarks>
        ///     Pings are used both to monitor the liveness of the connection and also to dynamically
        ///     adjust the throttle during periods of low traffic so that the throttle has reasonable
        ///     responsiveness during traffic spikes.
        /// </remarks>
        /// <param name="peer">the peer to adjust</param>
        /// <param name="pingInterval">the interval at which to send pings; defaults to ENET_PEER_PING_INTERVAL if 0</param>
        public static void enet_peer_ping_interval(ENetPeer* peer, uint pingInterval)
        {
            peer->pingInterval = pingInterval != 0 ? pingInterval : ENET_PEER_PING_INTERVAL;
        }

        /// <summary>
        ///     Sets the timeout parameters for a peer.
        /// </summary>
        /// <remarks>
        ///     The timeout parameter control how and when a peer will timeout from a failure to acknowledge
        ///     reliable traffic. Timeout values use an exponential backoff mechanism, where if a reliable
        ///     packet is not acknowledge within some multiple of the average RTT plus a variance tolerance,
        ///     the timeout will be doubled until it reaches a set limit. If the timeout is thus at this
        ///     limit and reliable packets have been sent but not acknowledged within a certain minimum time
        ///     period, the peer will be disconnected. Alternatively, if reliable packets have been sent
        ///     but not acknowledged for a certain maximum time period, the peer will be disconnected regardless
        ///     of the current timeout limit value.
        /// </remarks>
        /// <param name="peer">the peer to adjust</param>
        /// <param name="timeoutLimit">the timeout limit; defaults to ENET_PEER_TIMEOUT_LIMIT if 0</param>
        /// <param name="timeoutMinimum">the timeout minimum; defaults to ENET_PEER_TIMEOUT_MINIMUM if 0</param>
        /// <param name="timeoutMaximum">the timeout maximum; defaults to ENET_PEER_TIMEOUT_MAXIMUM if 0</param>
        public static void enet_peer_timeout(ENetPeer* peer, uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum)
        {
            peer->timeoutLimit = timeoutLimit != 0 ? timeoutLimit : ENET_PEER_TIMEOUT_LIMIT;
            peer->timeoutMinimum = timeoutMinimum != 0 ? timeoutMinimum : ENET_PEER_TIMEOUT_MINIMUM;
            peer->timeoutMaximum = timeoutMaximum != 0 ? timeoutMaximum : ENET_PEER_TIMEOUT_MAXIMUM;
        }

        /// <summary>
        ///     Force an immediate disconnection from a peer.
        /// </summary>
        /// <param name="peer">peer to disconnect</param>
        /// <param name="data">data describing the disconnection</param>
        /// <remarks>
        ///     No ENET_EVENT_DISCONNECT event will be generated. The foreign peer is not
        ///     guaranteed to receive the disconnect notification, and is reset immediately upon
        ///     return from this function.
        /// </remarks>
        public static void enet_peer_disconnect_now(ENetPeer* peer, uint data)
        {
            ENetProtocol command;

            if (peer->state == ENET_PEER_STATE_DISCONNECTED)
                return;

            if (peer->state != ENET_PEER_STATE_ZOMBIE &&
                peer->state != ENET_PEER_STATE_DISCONNECTING)
            {
                enet_peer_reset_queues(peer);

                command.header.command = (byte)((uint)ENET_PROTOCOL_COMMAND_DISCONNECT | (uint)ENET_PROTOCOL_COMMAND_FLAG_UNSEQUENCED);
                command.header.channelID = 0xFF;
                command.disconnect.data = ENET_HOST_TO_NET_32(data);

                enet_peer_queue_outgoing_command(peer, &command, null, 0, 0);

                enet_host_flush(peer->host);
            }

            enet_peer_reset(peer);
        }

        /// <summary>
        ///     Request a disconnection from a peer.
        /// </summary>
        /// <param name="peer">peer to request a disconnection</param>
        /// <param name="data">data describing the disconnection</param>
        /// <remarks>
        ///     An ENET_EVENT_DISCONNECT event will be generated by enet_host_service()
        ///     once the disconnection is complete.
        /// </remarks>
        public static void enet_peer_disconnect(ENetPeer* peer, uint data)
        {
            ENetProtocol command;

            if (peer->state == ENET_PEER_STATE_DISCONNECTING ||
                peer->state == ENET_PEER_STATE_DISCONNECTED ||
                peer->state == ENET_PEER_STATE_ACKNOWLEDGING_DISCONNECT ||
                peer->state == ENET_PEER_STATE_ZOMBIE)
                return;

            enet_peer_reset_queues(peer);

            command.header.command = (byte)ENET_PROTOCOL_COMMAND_DISCONNECT;
            command.header.channelID = 0xFF;
            command.disconnect.data = ENET_HOST_TO_NET_32(data);

            uint newCommand = command.header.command;

            if (peer->state == ENET_PEER_STATE_CONNECTED || peer->state == ENET_PEER_STATE_DISCONNECT_LATER)
                newCommand |= (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE;
            else
                newCommand |= (uint)ENET_PROTOCOL_COMMAND_FLAG_UNSEQUENCED;

            command.header.command = (byte)newCommand;

            enet_peer_queue_outgoing_command(peer, &command, null, 0, 0);

            if (peer->state == ENET_PEER_STATE_CONNECTED || peer->state == ENET_PEER_STATE_DISCONNECT_LATER)
            {
                enet_peer_on_disconnect(peer);

                peer->state = ENET_PEER_STATE_DISCONNECTING;
            }
            else
            {
                enet_host_flush(peer->host);
                enet_peer_reset(peer);
            }
        }

        public static int enet_peer_has_outgoing_commands(ENetPeer* peer)
        {
            if (enet_list_empty(&peer->outgoingCommands) &&
                enet_list_empty(&peer->outgoingSendReliableCommands) &&
                enet_list_empty(&peer->sentReliableCommands))
                return 0;

            return 1;
        }

        /// <summary>
        ///     Request a disconnection from a peer, but only after all queued outgoing packets are sent.
        /// </summary>
        /// <param name="peer">peer to request a disconnection</param>
        /// <param name="data">data describing the disconnection</param>
        /// <remarks>
        ///     An ENET_EVENT_DISCONNECT event will be generated by enet_host_service()
        ///     once the disconnection is complete.
        /// </remarks>
        public static void enet_peer_disconnect_later(ENetPeer* peer, uint data)
        {
            if ((peer->state == ENET_PEER_STATE_CONNECTED || peer->state == ENET_PEER_STATE_DISCONNECT_LATER) &&
                enet_peer_has_outgoing_commands(peer) != 0)
            {
                peer->state = ENET_PEER_STATE_DISCONNECT_LATER;
                peer->eventData = data;
            }
            else
                enet_peer_disconnect(peer, data);
        }

        public static ENetAcknowledgement* enet_peer_queue_acknowledgement(ENetPeer* peer, ENetProtocol* command, ushort sentTime)
        {
            ENetAcknowledgement* acknowledgement;

            if (command->header.channelID < peer->channelCount)
            {
                ENetChannel* channel = &peer->channels[command->header.channelID];
                ushort reliableWindow = (ushort)(command->header.reliableSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE),
                    currentWindow = (ushort)(channel->incomingReliableSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE);

                if (command->header.reliableSequenceNumber < channel->incomingReliableSequenceNumber)
                    reliableWindow += (ushort)ENET_PEER_RELIABLE_WINDOWS;

                if (reliableWindow >= currentWindow + ENET_PEER_FREE_RELIABLE_WINDOWS - 1 && reliableWindow <= currentWindow + ENET_PEER_FREE_RELIABLE_WINDOWS)
                    return null;
            }

            acknowledgement = (ENetAcknowledgement*)enet_malloc((nuint)sizeof(ENetAcknowledgement));

            peer->outgoingDataTotal += (uint)sizeof(ENetProtocolAcknowledge);

            acknowledgement->sentTime = sentTime;
            acknowledgement->command = *command;

            enet_list_insert(enet_list_end(&peer->acknowledgements), acknowledgement);

            return acknowledgement;
        }

        public static void enet_peer_setup_outgoing_command(ENetPeer* peer, ENetOutgoingCommand* outgoingCommand)
        {
            peer->outgoingDataTotal += (uint)(enet_protocol_command_size(outgoingCommand->command.header.command) + outgoingCommand->fragmentLength);

            if (outgoingCommand->command.header.channelID == 0xFF)
            {
                ++peer->outgoingReliableSequenceNumber;

                outgoingCommand->reliableSequenceNumber = peer->outgoingReliableSequenceNumber;
                outgoingCommand->unreliableSequenceNumber = 0;
            }
            else
            {
                ENetChannel* channel = &peer->channels[outgoingCommand->command.header.channelID];

                if ((outgoingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE) != 0)
                {
                    ++channel->outgoingReliableSequenceNumber;
                    channel->outgoingUnreliableSequenceNumber = 0;

                    outgoingCommand->reliableSequenceNumber = channel->outgoingReliableSequenceNumber;
                    outgoingCommand->unreliableSequenceNumber = 0;
                }
                else if ((outgoingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_FLAG_UNSEQUENCED) != 0)
                {
                    ++peer->outgoingUnsequencedGroup;

                    outgoingCommand->reliableSequenceNumber = 0;
                    outgoingCommand->unreliableSequenceNumber = 0;
                }
                else
                {
                    if (outgoingCommand->fragmentOffset == 0)
                        ++channel->outgoingUnreliableSequenceNumber;

                    outgoingCommand->reliableSequenceNumber = channel->outgoingReliableSequenceNumber;
                    outgoingCommand->unreliableSequenceNumber = channel->outgoingUnreliableSequenceNumber;
                }
            }

            outgoingCommand->sendAttempts = 0;
            outgoingCommand->sentTime = 0;
            outgoingCommand->roundTripTimeout = 0;
            outgoingCommand->command.header.reliableSequenceNumber = ENET_HOST_TO_NET_16(outgoingCommand->reliableSequenceNumber);
            outgoingCommand->queueTime = ++peer->host->totalQueued;

            switch (outgoingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_MASK)
            {
                case (uint)ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE:
                    outgoingCommand->command.sendUnreliable.unreliableSequenceNumber = ENET_HOST_TO_NET_16(outgoingCommand->unreliableSequenceNumber);
                    break;

                case (uint)ENET_PROTOCOL_COMMAND_SEND_UNSEQUENCED:
                    outgoingCommand->command.sendUnsequenced.unsequencedGroup = ENET_HOST_TO_NET_16(peer->outgoingUnsequencedGroup);
                    break;

                default:
                    break;
            }

            if ((outgoingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE) != 0 &&
                outgoingCommand->packet != null)
                enet_list_insert(enet_list_end(&peer->outgoingSendReliableCommands), outgoingCommand);
            else
                enet_list_insert(enet_list_end(&peer->outgoingCommands), outgoingCommand);
        }

        public static ENetOutgoingCommand* enet_peer_queue_outgoing_command(ENetPeer* peer, ENetProtocol* command, ENetPacket* packet, uint offset, ushort length)
        {
            ENetOutgoingCommand* outgoingCommand = (ENetOutgoingCommand*)enet_malloc((nuint)sizeof(ENetOutgoingCommand));

            outgoingCommand->command = *command;
            outgoingCommand->fragmentOffset = offset;
            outgoingCommand->fragmentLength = length;
            outgoingCommand->packet = packet;
            if (packet != null)
                ++packet->referenceCount;

            enet_peer_setup_outgoing_command(peer, outgoingCommand);

            return outgoingCommand;
        }

        public static void enet_peer_dispatch_incoming_unreliable_commands(ENetPeer* peer, ENetChannel* channel, ENetIncomingCommand* queuedCommand)
        {
            ENetListNode* droppedCommand, startCommand, currentCommand;

            for (droppedCommand = startCommand = currentCommand = enet_list_begin(&channel->incomingUnreliableCommands);
                 currentCommand != enet_list_end(&channel->incomingUnreliableCommands);
                 currentCommand = enet_list_next(currentCommand))
            {
                ENetIncomingCommand* incomingCommand = (ENetIncomingCommand*)currentCommand;

                if ((incomingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_MASK) == (uint)ENET_PROTOCOL_COMMAND_SEND_UNSEQUENCED)
                    continue;

                if (incomingCommand->reliableSequenceNumber == channel->incomingReliableSequenceNumber)
                {
                    if (incomingCommand->fragmentsRemaining <= 0)
                    {
                        channel->incomingUnreliableSequenceNumber = incomingCommand->unreliableSequenceNumber;
                        continue;
                    }

                    if (startCommand != currentCommand)
                    {
                        enet_list_move(enet_list_end(&peer->dispatchedCommands), startCommand, enet_list_previous(currentCommand));

                        if (!((peer->flags & (uint)ENET_PEER_FLAG_NEEDS_DISPATCH) != 0))
                        {
                            enet_list_insert(enet_list_end(&peer->host->dispatchQueue), &peer->dispatchList);

                            uint flags = peer->flags;
                            flags |= (uint)ENET_PEER_FLAG_NEEDS_DISPATCH;
                            peer->flags = (ushort)flags;
                        }

                        droppedCommand = currentCommand;
                    }
                    else if (droppedCommand != currentCommand)
                        droppedCommand = enet_list_previous(currentCommand);
                }
                else
                {
                    ushort reliableWindow = (ushort)(incomingCommand->reliableSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE),
                        currentWindow = (ushort)(channel->incomingReliableSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE);
                    if (incomingCommand->reliableSequenceNumber < channel->incomingReliableSequenceNumber)
                        reliableWindow += (ushort)ENET_PEER_RELIABLE_WINDOWS;
                    if (reliableWindow >= currentWindow && reliableWindow < currentWindow + ENET_PEER_FREE_RELIABLE_WINDOWS - 1)
                        break;

                    droppedCommand = enet_list_next(currentCommand);

                    if (startCommand != currentCommand)
                    {
                        enet_list_move(enet_list_end(&peer->dispatchedCommands), startCommand, enet_list_previous(currentCommand));

                        if (!((peer->flags & (uint)ENET_PEER_FLAG_NEEDS_DISPATCH) != 0))
                        {
                            enet_list_insert(enet_list_end(&peer->host->dispatchQueue), &peer->dispatchList);

                            uint flags = peer->flags;
                            flags |= (uint)ENET_PEER_FLAG_NEEDS_DISPATCH;
                            peer->flags = (ushort)flags;
                        }
                    }
                }

                startCommand = enet_list_next(currentCommand);
            }

            if (startCommand != currentCommand)
            {
                enet_list_move(enet_list_end(&peer->dispatchedCommands), startCommand, enet_list_previous(currentCommand));

                if (!((peer->flags & (uint)ENET_PEER_FLAG_NEEDS_DISPATCH) != 0))
                {
                    enet_list_insert(enet_list_end(&peer->host->dispatchQueue), &peer->dispatchList);

                    uint flags = peer->flags;
                    flags |= (uint)ENET_PEER_FLAG_NEEDS_DISPATCH;
                    peer->flags = (ushort)flags;
                }

                droppedCommand = currentCommand;
            }

            enet_peer_remove_incoming_commands(peer, &channel->incomingUnreliableCommands, enet_list_begin(&channel->incomingUnreliableCommands), droppedCommand, queuedCommand);
        }

        public static void enet_peer_dispatch_incoming_reliable_commands(ENetPeer* peer, ENetChannel* channel, ENetIncomingCommand* queuedCommand)
        {
            ENetListNode* currentCommand;

            for (currentCommand = enet_list_begin(&channel->incomingReliableCommands);
                 currentCommand != enet_list_end(&channel->incomingReliableCommands);
                 currentCommand = enet_list_next(currentCommand))
            {
                ENetIncomingCommand* incomingCommand = (ENetIncomingCommand*)currentCommand;

                if (incomingCommand->fragmentsRemaining > 0 ||
                    incomingCommand->reliableSequenceNumber != (ushort)(channel->incomingReliableSequenceNumber + 1))
                    break;

                channel->incomingReliableSequenceNumber = incomingCommand->reliableSequenceNumber;

                if (incomingCommand->fragmentCount > 0)
                    channel->incomingReliableSequenceNumber += (ushort)(incomingCommand->fragmentCount - 1);
            }

            if (currentCommand == enet_list_begin(&channel->incomingReliableCommands))
                return;

            channel->incomingUnreliableSequenceNumber = 0;

            enet_list_move(enet_list_end(&peer->dispatchedCommands), enet_list_begin(&channel->incomingReliableCommands), enet_list_previous(currentCommand));

            if (!((peer->flags & (uint)ENET_PEER_FLAG_NEEDS_DISPATCH) != 0))
            {
                enet_list_insert(enet_list_end(&peer->host->dispatchQueue), &peer->dispatchList);

                uint flags = peer->flags;
                flags |= (uint)ENET_PEER_FLAG_NEEDS_DISPATCH;
                peer->flags = (ushort)flags;
            }

            if (!enet_list_empty(&channel->incomingUnreliableCommands))
                enet_peer_dispatch_incoming_unreliable_commands(peer, channel, queuedCommand);
        }

        public static ENetIncomingCommand* enet_peer_queue_incoming_command(ENetPeer* peer, ENetProtocol* command, void* data, nuint dataLength, uint flags, uint fragmentCount)
        {
            ENetIncomingCommand dummyCommand;

            ENetChannel* channel = &peer->channels[command->header.channelID];
            uint unreliableSequenceNumber = 0, reliableSequenceNumber = 0;
            ushort reliableWindow, currentWindow;
            ENetIncomingCommand* incomingCommand;
            ENetListNode* currentCommand;
            ENetPacket* packet = null;

            if (peer->state == ENET_PEER_STATE_DISCONNECT_LATER)
                goto discardCommand;

            if ((command->header.command & (uint)ENET_PROTOCOL_COMMAND_MASK) != (uint)ENET_PROTOCOL_COMMAND_SEND_UNSEQUENCED)
            {
                reliableSequenceNumber = command->header.reliableSequenceNumber;
                reliableWindow = (ushort)(reliableSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE);
                currentWindow = (ushort)(channel->incomingReliableSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE);

                if (reliableSequenceNumber < channel->incomingReliableSequenceNumber)
                    reliableWindow += (ushort)ENET_PEER_RELIABLE_WINDOWS;

                if (reliableWindow < currentWindow || reliableWindow >= currentWindow + ENET_PEER_FREE_RELIABLE_WINDOWS - 1)
                    goto discardCommand;
            }

            switch (command->header.command & (uint)ENET_PROTOCOL_COMMAND_MASK)
            {
                case (uint)ENET_PROTOCOL_COMMAND_SEND_FRAGMENT:
                case (uint)ENET_PROTOCOL_COMMAND_SEND_RELIABLE:
                    if (reliableSequenceNumber == channel->incomingReliableSequenceNumber)
                        goto discardCommand;

                    for (currentCommand = enet_list_previous(enet_list_end(&channel->incomingReliableCommands));
                         currentCommand != enet_list_end(&channel->incomingReliableCommands);
                         currentCommand = enet_list_previous(currentCommand))
                    {
                        incomingCommand = (ENetIncomingCommand*)currentCommand;

                        if (reliableSequenceNumber >= channel->incomingReliableSequenceNumber)
                        {
                            if (incomingCommand->reliableSequenceNumber < channel->incomingReliableSequenceNumber)
                                continue;
                        }
                        else if (incomingCommand->reliableSequenceNumber >= channel->incomingReliableSequenceNumber)
                            break;

                        if (incomingCommand->reliableSequenceNumber <= reliableSequenceNumber)
                        {
                            if (incomingCommand->reliableSequenceNumber < reliableSequenceNumber)
                                break;

                            goto discardCommand;
                        }
                    }

                    break;

                case (uint)ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE:
                case (uint)ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE_FRAGMENT:
                    unreliableSequenceNumber = ENET_NET_TO_HOST_16(command->sendUnreliable.unreliableSequenceNumber);

                    if (reliableSequenceNumber == channel->incomingReliableSequenceNumber &&
                        unreliableSequenceNumber <= channel->incomingUnreliableSequenceNumber)
                        goto discardCommand;

                    for (currentCommand = enet_list_previous(enet_list_end(&channel->incomingUnreliableCommands));
                         currentCommand != enet_list_end(&channel->incomingUnreliableCommands);
                         currentCommand = enet_list_previous(currentCommand))
                    {
                        incomingCommand = (ENetIncomingCommand*)currentCommand;

                        if ((command->header.command & (uint)ENET_PROTOCOL_COMMAND_MASK) == (uint)ENET_PROTOCOL_COMMAND_SEND_UNSEQUENCED)
                            continue;

                        if (reliableSequenceNumber >= channel->incomingReliableSequenceNumber)
                        {
                            if (incomingCommand->reliableSequenceNumber < channel->incomingReliableSequenceNumber)
                                continue;
                        }
                        else if (incomingCommand->reliableSequenceNumber >= channel->incomingReliableSequenceNumber)
                            break;

                        if (incomingCommand->reliableSequenceNumber < reliableSequenceNumber)
                            break;

                        if (incomingCommand->reliableSequenceNumber > reliableSequenceNumber)
                            continue;

                        if (incomingCommand->unreliableSequenceNumber <= unreliableSequenceNumber)
                        {
                            if (incomingCommand->unreliableSequenceNumber < unreliableSequenceNumber)
                                break;

                            goto discardCommand;
                        }
                    }

                    break;

                case (uint)ENET_PROTOCOL_COMMAND_SEND_UNSEQUENCED:
                    currentCommand = enet_list_end(&channel->incomingUnreliableCommands);
                    break;

                default:
                    goto discardCommand;
            }

            if (peer->totalWaitingData >= peer->host->maximumWaitingData)
                goto notifyError;

            packet = enet_packet_create(data, dataLength, flags);
            if (packet == null)
                goto notifyError;

            incomingCommand = (ENetIncomingCommand*)enet_malloc((nuint)sizeof(ENetIncomingCommand));

            incomingCommand->reliableSequenceNumber = command->header.reliableSequenceNumber;
            incomingCommand->unreliableSequenceNumber = (ushort)(unreliableSequenceNumber & 0xFFFF);
            incomingCommand->command = *command;
            incomingCommand->fragmentCount = fragmentCount;
            incomingCommand->fragmentsRemaining = fragmentCount;
            incomingCommand->packet = packet;
            incomingCommand->fragments = null;

            if (fragmentCount > 0)
            {
                if (fragmentCount <= ENET_PROTOCOL_MAXIMUM_FRAGMENT_COUNT)
                    incomingCommand->fragments = (uint*)enet_malloc((nuint)((fragmentCount + 31) / 32 * sizeof(uint)));

                memset(incomingCommand->fragments, 0, (nuint)((fragmentCount + 31) / 32 * sizeof(uint)));
            }

            if (packet != null)
            {
                ++packet->referenceCount;

                peer->totalWaitingData += packet->dataLength;
            }

            enet_list_insert(enet_list_next(currentCommand), incomingCommand);

            switch (command->header.command & (uint)ENET_PROTOCOL_COMMAND_MASK)
            {
                case (uint)ENET_PROTOCOL_COMMAND_SEND_FRAGMENT:
                case (uint)ENET_PROTOCOL_COMMAND_SEND_RELIABLE:
                    enet_peer_dispatch_incoming_reliable_commands(peer, channel, incomingCommand);
                    break;

                default:
                    enet_peer_dispatch_incoming_unreliable_commands(peer, channel, incomingCommand);
                    break;
            }

            return incomingCommand;

            discardCommand:
            if (fragmentCount > 0)
                goto notifyError;

            if (packet != null && packet->referenceCount == 0)
                enet_packet_destroy(packet);

            return &dummyCommand;

            notifyError:
            if (packet != null && packet->referenceCount == 0)
                enet_packet_destroy(packet);

            return null;
        }
    }
}