using System;
using static enet.ENetPeerState;
using static enet.ENetProtocolCommand;
using static enet.ENetPeerFlag;
using static enet.ENetEventType;
using static enet.ENetPacketFlag;
using static enet.ENetProtocolFlag;
using static enet.ENetSocketWait;

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public static ReadOnlySpan<nint> commandSizes => new nint[(int)ENET_PROTOCOL_COMMAND_COUNT]
        {
            0,
            sizeof(ENetProtocolAcknowledge),
            sizeof(ENetProtocolConnect),
            sizeof(ENetProtocolVerifyConnect),
            sizeof(ENetProtocolDisconnect),
            sizeof(ENetProtocolPing),
            sizeof(ENetProtocolSendReliable),
            sizeof(ENetProtocolSendUnreliable),
            sizeof(ENetProtocolSendFragment),
            sizeof(ENetProtocolSendUnsequenced),
            sizeof(ENetProtocolBandwidthLimit),
            sizeof(ENetProtocolThrottleConfigure),
            sizeof(ENetProtocolSendFragment)
        };

        public static nint enet_protocol_command_size(byte commandNumber) => commandSizes[commandNumber & (int)ENET_PROTOCOL_COMMAND_MASK];

        public static void enet_protocol_change_state(ENetHost* host, ENetPeer* peer, ENetPeerState state)
        {
            if (state == ENET_PEER_STATE_CONNECTED || state == ENET_PEER_STATE_DISCONNECT_LATER)
                enet_peer_on_connect(peer);
            else
                enet_peer_on_disconnect(peer);

            peer->state = state;
        }

        public static void enet_protocol_dispatch_state(ENetHost* host, ENetPeer* peer, ENetPeerState state)
        {
            enet_protocol_change_state(host, peer, state);

            if (!((peer->flags & (uint)ENET_PEER_FLAG_NEEDS_DISPATCH) != 0))
            {
                enet_list_insert(enet_list_end(&host->dispatchQueue), &peer->dispatchList);

                uint flags = peer->flags;
                flags |= (uint)ENET_PEER_FLAG_NEEDS_DISPATCH;
                peer->flags = (ushort)flags;
            }
        }

        public static int enet_protocol_dispatch_incoming_commands(ENetHost* host, ENetEvent* @event)
        {
            while (!enet_list_empty(&host->dispatchQueue))
            {
                ENetPeer* peer = (ENetPeer*)enet_list_remove(enet_list_begin(&host->dispatchQueue));

                uint flags = peer->flags;
                flags &= ~(uint)ENET_PEER_FLAG_NEEDS_DISPATCH;
                peer->flags = (ushort)flags;

                switch (peer->state)
                {
                    case ENET_PEER_STATE_CONNECTION_PENDING:
                    case ENET_PEER_STATE_CONNECTION_SUCCEEDED:
                        enet_protocol_change_state(host, peer, ENET_PEER_STATE_CONNECTED);

                        @event->type = ENET_EVENT_TYPE_CONNECT;
                        @event->peer = peer;
                        @event->data = peer->eventData;

                        return 1;

                    case ENET_PEER_STATE_ZOMBIE:
                        host->recalculateBandwidthLimits = 1;

                        @event->type = ENET_EVENT_TYPE_DISCONNECT;
                        @event->peer = peer;
                        @event->data = peer->eventData;

                        enet_peer_reset(peer);

                        return 1;

                    case ENET_PEER_STATE_CONNECTED:
                        if (enet_list_empty(&peer->dispatchedCommands))
                            continue;

                        @event->packet = enet_peer_receive(peer, &@event->channelID);
                        if (@event->packet == null)
                            continue;

                        @event->type = ENET_EVENT_TYPE_RECEIVE;
                        @event->peer = peer;

                        if (!enet_list_empty(&peer->dispatchedCommands))
                        {
                            flags = peer->flags;
                            flags |= (uint)ENET_PEER_FLAG_NEEDS_DISPATCH;
                            peer->flags = (ushort)flags;

                            enet_list_insert(enet_list_end(&host->dispatchQueue), &peer->dispatchList);
                        }

                        return 1;

                    default:
                        break;
                }
            }

            return 0;
        }

        public static void enet_protocol_notify_connect(ENetHost* host, ENetPeer* peer, ENetEvent* @event)
        {
            host->recalculateBandwidthLimits = 1;

            if (@event != null)
            {
                enet_protocol_change_state(host, peer, ENET_PEER_STATE_CONNECTED);

                @event->type = ENET_EVENT_TYPE_CONNECT;
                @event->peer = peer;
                @event->data = peer->eventData;
            }
            else
                enet_protocol_dispatch_state(host, peer, peer->state == ENET_PEER_STATE_CONNECTING ? ENET_PEER_STATE_CONNECTION_SUCCEEDED : ENET_PEER_STATE_CONNECTION_PENDING);
        }

        public static void enet_protocol_notify_disconnect(ENetHost* host, ENetPeer* peer, ENetEvent* @event)
        {
            if (peer->state >= ENET_PEER_STATE_CONNECTION_PENDING)
                host->recalculateBandwidthLimits = 1;

            if (peer->state != ENET_PEER_STATE_CONNECTING && peer->state < ENET_PEER_STATE_CONNECTION_SUCCEEDED)
                enet_peer_reset(peer);
            else if (@event != null)
            {
                @event->type = ENET_EVENT_TYPE_DISCONNECT;
                @event->peer = peer;
                @event->data = 0;

                enet_peer_reset(peer);
            }
            else
            {
                peer->eventData = 0;

                enet_protocol_dispatch_state(host, peer, ENET_PEER_STATE_ZOMBIE);
            }
        }

        public static void enet_protocol_remove_sent_unreliable_commands(ENetPeer* peer, ENetList* sentUnreliableCommands)
        {
            ENetOutgoingCommand* outgoingCommand;

            if (enet_list_empty(sentUnreliableCommands))
                return;

            do
            {
                outgoingCommand = (ENetOutgoingCommand*)enet_list_front(sentUnreliableCommands);

                enet_list_remove(&outgoingCommand->outgoingCommandList);

                if (outgoingCommand->packet != null)
                {
                    --outgoingCommand->packet->referenceCount;

                    if (outgoingCommand->packet->referenceCount == 0)
                    {
                        outgoingCommand->packet->flags |= (uint)ENET_PACKET_FLAG_SENT;

                        enet_packet_destroy(outgoingCommand->packet);
                    }
                }

                enet_free(outgoingCommand);
            } while (!enet_list_empty(sentUnreliableCommands));

            if (peer->state == ENET_PEER_STATE_DISCONNECT_LATER &&
                !(enet_peer_has_outgoing_commands(peer) != 0))
                enet_peer_disconnect(peer, peer->eventData);
        }

        public static ENetOutgoingCommand* enet_protocol_find_sent_reliable_command(ENetList* list, ushort reliableSequenceNumber, byte channelID)
        {
            ENetListNode* currentCommand;

            for (currentCommand = enet_list_begin(list);
                 currentCommand != enet_list_end(list);
                 currentCommand = enet_list_next(currentCommand))
            {
                ENetOutgoingCommand* outgoingCommand = (ENetOutgoingCommand*)currentCommand;

                if (!((outgoingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE) != 0))
                    continue;

                if (outgoingCommand->sendAttempts < 1)
                    break;

                if (outgoingCommand->reliableSequenceNumber == reliableSequenceNumber &&
                    outgoingCommand->command.header.channelID == channelID)
                    return outgoingCommand;
            }

            return null;
        }

        public static ENetProtocolCommand enet_protocol_remove_sent_reliable_command(ENetPeer* peer, ushort reliableSequenceNumber, byte channelID)
        {
            ENetOutgoingCommand* outgoingCommand = null;
            ENetListNode* currentCommand;
            ENetProtocolCommand commandNumber;
            int wasSent = 1;

            for (currentCommand = enet_list_begin(&peer->sentReliableCommands);
                 currentCommand != enet_list_end(&peer->sentReliableCommands);
                 currentCommand = enet_list_next(currentCommand))
            {
                outgoingCommand = (ENetOutgoingCommand*)currentCommand;

                if (outgoingCommand->reliableSequenceNumber == reliableSequenceNumber &&
                    outgoingCommand->command.header.channelID == channelID)
                    break;
            }

            if (currentCommand == enet_list_end(&peer->sentReliableCommands))
            {
                outgoingCommand = enet_protocol_find_sent_reliable_command(&peer->outgoingCommands, reliableSequenceNumber, channelID);
                if (outgoingCommand == null)
                    outgoingCommand = enet_protocol_find_sent_reliable_command(&peer->outgoingSendReliableCommands, reliableSequenceNumber, channelID);

                wasSent = 0;
            }

            if (outgoingCommand == null)
                return ENET_PROTOCOL_COMMAND_NONE;

            if (channelID < peer->channelCount)
            {
                ENetChannel* channel = &peer->channels[channelID];
                ushort reliableWindow = (ushort)(reliableSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE);
                if (channel->reliableWindows[reliableWindow] > 0)
                {
                    --channel->reliableWindows[reliableWindow];
                    if (!(channel->reliableWindows[reliableWindow] != 0))
                    {
                        uint usedReliableWindows = channel->usedReliableWindows;
                        usedReliableWindows &= ~ (1u << reliableWindow);
                        channel->usedReliableWindows = (ushort)usedReliableWindows;
                    }
                }
            }

            commandNumber = (ENetProtocolCommand)(outgoingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_MASK);

            enet_list_remove(&outgoingCommand->outgoingCommandList);

            if (outgoingCommand->packet != null)
            {
                if (wasSent != 0)
                    peer->reliableDataInTransit -= outgoingCommand->fragmentLength;

                --outgoingCommand->packet->referenceCount;

                if (outgoingCommand->packet->referenceCount == 0)
                {
                    outgoingCommand->packet->flags |= (uint)ENET_PACKET_FLAG_SENT;

                    enet_packet_destroy(outgoingCommand->packet);
                }
            }

            enet_free(outgoingCommand);

            if (enet_list_empty(&peer->sentReliableCommands))
                return commandNumber;

            outgoingCommand = (ENetOutgoingCommand*)enet_list_front(&peer->sentReliableCommands);

            peer->nextTimeout = outgoingCommand->sentTime + outgoingCommand->roundTripTimeout;

            return commandNumber;
        }

        public static ENetPeer* enet_protocol_handle_connect(ENetHost* host, ENetProtocolHeader* header, ENetProtocol* command)
        {
            byte incomingSessionID, outgoingSessionID;
            uint mtu, windowSize;
            ENetChannel* channel;
            nint channelCount, duplicatePeers = 0;
            ENetPeer* currentPeer, peer = null;
            ENetProtocol verifyCommand;

            channelCount = (nint)ENET_NET_TO_HOST_32(command->connect.channelCount);

            if (channelCount < ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT ||
                channelCount > ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT)
                return null;

            for (currentPeer = host->peers;
                 currentPeer < &host->peers[host->peerCount];
                 ++currentPeer)
            {
                if (currentPeer->state == ENET_PEER_STATE_DISCONNECTED)
                {
                    if (peer == null)
                        peer = currentPeer;
                }
                else if (currentPeer->state != ENET_PEER_STATE_CONNECTING &&
                         currentPeer->address.host == host->receivedAddress.host)
                {
                    if (currentPeer->address.port == host->receivedAddress.port &&
                        currentPeer->connectID == command->connect.connectID)
                        return null;

                    ++duplicatePeers;
                }
            }

            if (peer == null || duplicatePeers >= host->duplicatePeers)
                return null;

            if (channelCount > host->channelLimit)
                channelCount = host->channelLimit;
            peer->channels = (ENetChannel*)enet_malloc(channelCount * sizeof(ENetChannel));

            peer->channelCount = channelCount;
            peer->state = ENET_PEER_STATE_ACKNOWLEDGING_CONNECT;
            peer->connectID = command->connect.connectID;
            peer->address = host->receivedAddress;
            peer->mtu = host->mtu;
            peer->outgoingPeerID = ENET_NET_TO_HOST_16(command->connect.outgoingPeerID);
            peer->incomingBandwidth = ENET_NET_TO_HOST_32(command->connect.incomingBandwidth);
            peer->outgoingBandwidth = ENET_NET_TO_HOST_32(command->connect.outgoingBandwidth);
            peer->packetThrottleInterval = ENET_NET_TO_HOST_32(command->connect.packetThrottleInterval);
            peer->packetThrottleAcceleration = ENET_NET_TO_HOST_32(command->connect.packetThrottleAcceleration);
            peer->packetThrottleDeceleration = ENET_NET_TO_HOST_32(command->connect.packetThrottleDeceleration);
            peer->eventData = ENET_NET_TO_HOST_32(command->connect.data);

            incomingSessionID = command->connect.incomingSessionID == 0xFF ? peer->outgoingSessionID : command->connect.incomingSessionID;
            incomingSessionID = (byte)((incomingSessionID + 1) & ((int)ENET_PROTOCOL_HEADER_SESSION_MASK >> (int)ENET_PROTOCOL_HEADER_SESSION_SHIFT));
            if (incomingSessionID == peer->outgoingSessionID)
                incomingSessionID = (byte)((incomingSessionID + 1) & ((int)ENET_PROTOCOL_HEADER_SESSION_MASK >> (int)ENET_PROTOCOL_HEADER_SESSION_SHIFT));
            peer->outgoingSessionID = incomingSessionID;

            outgoingSessionID = command->connect.outgoingSessionID == 0xFF ? peer->incomingSessionID : command->connect.outgoingSessionID;
            outgoingSessionID = (byte)((outgoingSessionID + 1) & ((int)ENET_PROTOCOL_HEADER_SESSION_MASK >> (int)ENET_PROTOCOL_HEADER_SESSION_SHIFT));
            if (outgoingSessionID == peer->incomingSessionID)
                outgoingSessionID = (byte)((outgoingSessionID + 1) & ((int)ENET_PROTOCOL_HEADER_SESSION_MASK >> (int)ENET_PROTOCOL_HEADER_SESSION_SHIFT));
            peer->incomingSessionID = outgoingSessionID;

            for (channel = peer->channels;
                 channel < &peer->channels[channelCount];
                 ++channel)
            {
                channel->outgoingReliableSequenceNumber = 0;
                channel->outgoingUnreliableSequenceNumber = 0;
                channel->incomingReliableSequenceNumber = 0;
                channel->incomingUnreliableSequenceNumber = 0;

                enet_list_clear(&channel->incomingReliableCommands);
                enet_list_clear(&channel->incomingUnreliableCommands);

                channel->usedReliableWindows = 0;
                memset(channel->reliableWindows, 0, (nint)(ENET_PEER_RELIABLE_WINDOWS * sizeof(ushort)));
            }

            mtu = ENET_NET_TO_HOST_32(command->connect.mtu);

            if (mtu < ENET_PROTOCOL_MINIMUM_MTU)
                mtu = ENET_PROTOCOL_MINIMUM_MTU;
            else if (mtu > ENET_PROTOCOL_MAXIMUM_MTU)
                mtu = ENET_PROTOCOL_MAXIMUM_MTU;

            if (mtu < peer->mtu)
                peer->mtu = mtu;

            if (host->outgoingBandwidth == 0 &&
                peer->incomingBandwidth == 0)
                peer->windowSize = ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE;
            else if (host->outgoingBandwidth == 0 ||
                     peer->incomingBandwidth == 0)
                peer->windowSize = (ENET_MAX(host->outgoingBandwidth, peer->incomingBandwidth) /
                                    ENET_PEER_WINDOW_SIZE_SCALE) *
                                   ENET_PROTOCOL_MINIMUM_WINDOW_SIZE;
            else
                peer->windowSize = (ENET_MIN(host->outgoingBandwidth, peer->incomingBandwidth) /
                                    ENET_PEER_WINDOW_SIZE_SCALE) *
                                   ENET_PROTOCOL_MINIMUM_WINDOW_SIZE;

            if (peer->windowSize < ENET_PROTOCOL_MINIMUM_WINDOW_SIZE)
                peer->windowSize = ENET_PROTOCOL_MINIMUM_WINDOW_SIZE;
            else if (peer->windowSize > ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE)
                peer->windowSize = ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE;

            if (host->incomingBandwidth == 0)
                windowSize = ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE;
            else
                windowSize = (host->incomingBandwidth / ENET_PEER_WINDOW_SIZE_SCALE) *
                             ENET_PROTOCOL_MINIMUM_WINDOW_SIZE;

            if (windowSize > ENET_NET_TO_HOST_32(command->connect.windowSize))
                windowSize = ENET_NET_TO_HOST_32(command->connect.windowSize);

            if (windowSize < ENET_PROTOCOL_MINIMUM_WINDOW_SIZE)
                windowSize = ENET_PROTOCOL_MINIMUM_WINDOW_SIZE;
            else if (windowSize > ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE)
                windowSize = ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE;

            verifyCommand.header.command = (byte)((uint)ENET_PROTOCOL_COMMAND_VERIFY_CONNECT | (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE);
            verifyCommand.header.channelID = 0xFF;
            verifyCommand.verifyConnect.outgoingPeerID = ENET_HOST_TO_NET_16(peer->incomingPeerID);
            verifyCommand.verifyConnect.incomingSessionID = incomingSessionID;
            verifyCommand.verifyConnect.outgoingSessionID = outgoingSessionID;
            verifyCommand.verifyConnect.mtu = ENET_HOST_TO_NET_32(peer->mtu);
            verifyCommand.verifyConnect.windowSize = ENET_HOST_TO_NET_32(windowSize);
            verifyCommand.verifyConnect.channelCount = ENET_HOST_TO_NET_32((uint)channelCount);
            verifyCommand.verifyConnect.incomingBandwidth = ENET_HOST_TO_NET_32(host->incomingBandwidth);
            verifyCommand.verifyConnect.outgoingBandwidth = ENET_HOST_TO_NET_32(host->outgoingBandwidth);
            verifyCommand.verifyConnect.packetThrottleInterval = ENET_HOST_TO_NET_32(peer->packetThrottleInterval);
            verifyCommand.verifyConnect.packetThrottleAcceleration = ENET_HOST_TO_NET_32(peer->packetThrottleAcceleration);
            verifyCommand.verifyConnect.packetThrottleDeceleration = ENET_HOST_TO_NET_32(peer->packetThrottleDeceleration);
            verifyCommand.verifyConnect.connectID = peer->connectID;

            enet_peer_queue_outgoing_command(peer, &verifyCommand, null, 0, 0);

            return peer;
        }

        public static int enet_protocol_handle_send_reliable(ENetHost* host, ENetPeer* peer, ENetProtocol* command, byte** currentData)
        {
            nint dataLength;

            if (command->header.channelID >= peer->channelCount ||
                (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER))
                return -1;

            dataLength = ENET_NET_TO_HOST_16(command->sendReliable.dataLength);
            *currentData += dataLength;
            if (dataLength > host->maximumPacketSize ||
                *currentData < host->receivedData ||
                *currentData > &host->receivedData[host->receivedDataLength])
                return -1;

            if (enet_peer_queue_incoming_command(peer, command, (byte*)command + sizeof(ENetProtocolSendReliable), dataLength, (uint)ENET_PACKET_FLAG_RELIABLE, 0) == null)
                return -1;

            return 0;
        }

        public static int enet_protocol_handle_send_unsequenced(ENetHost* host, ENetPeer* peer, ENetProtocol* command, byte** currentData)
        {
            uint unsequencedGroup, index;
            nint dataLength;

            if (command->header.channelID >= peer->channelCount ||
                (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER))
                return -1;

            dataLength = ENET_NET_TO_HOST_16(command->sendUnsequenced.dataLength);
            *currentData += dataLength;
            if (dataLength > host->maximumPacketSize ||
                *currentData < host->receivedData ||
                *currentData > &host->receivedData[host->receivedDataLength])
                return -1;

            unsequencedGroup = ENET_NET_TO_HOST_16(command->sendUnsequenced.unsequencedGroup);
            index = unsequencedGroup % ENET_PEER_UNSEQUENCED_WINDOW_SIZE;

            if (unsequencedGroup < peer->incomingUnsequencedGroup)
                unsequencedGroup += 0x10000;

            if (unsequencedGroup >= peer->incomingUnsequencedGroup + ENET_PEER_FREE_UNSEQUENCED_WINDOWS * ENET_PEER_UNSEQUENCED_WINDOW_SIZE)
                return 0;

            unsequencedGroup &= 0xFFFF;

            if (unsequencedGroup - index != peer->incomingUnsequencedGroup)
            {
                peer->incomingUnsequencedGroup = (ushort)(unsequencedGroup - index);

                memset(peer->unsequencedWindow, 0, (nint)(ENET_PEER_UNSEQUENCED_WINDOW_SIZE / 32 * sizeof(uint)));
            }
            else if ((peer->unsequencedWindow[index / 32] & (1u << (int)(index % 32))) != 0)
                return 0;

            if (enet_peer_queue_incoming_command(peer, command, (byte*)command + sizeof(ENetProtocolSendUnsequenced), dataLength, (uint)ENET_PACKET_FLAG_UNSEQUENCED, 0) == null)
                return -1;

            peer->unsequencedWindow[index / 32] |= 1u << (int)(index % 32);

            return 0;
        }

        public static int enet_protocol_handle_send_unreliable(ENetHost* host, ENetPeer* peer, ENetProtocol* command, byte** currentData)
        {
            nint dataLength;

            if (command->header.channelID >= peer->channelCount ||
                (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER))
                return -1;

            dataLength = ENET_NET_TO_HOST_16(command->sendUnreliable.dataLength);
            *currentData += dataLength;
            if (dataLength > host->maximumPacketSize ||
                *currentData < host->receivedData ||
                *currentData > &host->receivedData[host->receivedDataLength])
                return -1;

            if (enet_peer_queue_incoming_command(peer, command, (byte*)command + sizeof(ENetProtocolSendUnreliable), dataLength, 0, 0) == null)
                return -1;

            return 0;
        }

        public static int enet_protocol_handle_send_fragment(ENetHost* host, ENetPeer* peer, ENetProtocol* command, byte** currentData)
        {
            uint fragmentNumber,
                fragmentCount,
                fragmentOffset,
                fragmentLength,
                startSequenceNumber,
                totalLength;
            ENetChannel* channel;
            ushort startWindow, currentWindow;
            ENetListNode* currentCommand;
            ENetIncomingCommand* startCommand = null;

            if (command->header.channelID >= peer->channelCount ||
                (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER))
                return -1;

            fragmentLength = ENET_NET_TO_HOST_16(command->sendFragment.dataLength);
            *currentData += fragmentLength;
            if (fragmentLength <= 0 ||
                fragmentLength > host->maximumPacketSize ||
                *currentData < host->receivedData ||
                *currentData > &host->receivedData[host->receivedDataLength])
                return -1;

            channel = &peer->channels[command->header.channelID];
            startSequenceNumber = ENET_NET_TO_HOST_16(command->sendFragment.startSequenceNumber);
            startWindow = (ushort)(startSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE);
            currentWindow = (ushort)(channel->incomingReliableSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE);

            if (startSequenceNumber < channel->incomingReliableSequenceNumber)
                startWindow += (ushort)ENET_PEER_RELIABLE_WINDOWS;

            if (startWindow < currentWindow || startWindow >= currentWindow + ENET_PEER_FREE_RELIABLE_WINDOWS - 1)
                return 0;

            fragmentNumber = ENET_NET_TO_HOST_32(command->sendFragment.fragmentNumber);
            fragmentCount = ENET_NET_TO_HOST_32(command->sendFragment.fragmentCount);
            fragmentOffset = ENET_NET_TO_HOST_32(command->sendFragment.fragmentOffset);
            totalLength = ENET_NET_TO_HOST_32(command->sendFragment.totalLength);

            if (fragmentCount > ENET_PROTOCOL_MAXIMUM_FRAGMENT_COUNT ||
                fragmentNumber >= fragmentCount ||
                totalLength > host->maximumPacketSize ||
                totalLength < fragmentCount ||
                fragmentOffset >= totalLength ||
                fragmentLength > totalLength - fragmentOffset)
                return -1;

            for (currentCommand = enet_list_previous(enet_list_end(&channel->incomingReliableCommands));
                 currentCommand != enet_list_end(&channel->incomingReliableCommands);
                 currentCommand = enet_list_previous(currentCommand))
            {
                ENetIncomingCommand* incomingCommand = (ENetIncomingCommand*)currentCommand;

                if (startSequenceNumber >= channel->incomingReliableSequenceNumber)
                {
                    if (incomingCommand->reliableSequenceNumber < channel->incomingReliableSequenceNumber)
                        continue;
                }
                else if (incomingCommand->reliableSequenceNumber >= channel->incomingReliableSequenceNumber)
                    break;

                if (incomingCommand->reliableSequenceNumber <= startSequenceNumber)
                {
                    if (incomingCommand->reliableSequenceNumber < startSequenceNumber)
                        break;

                    if ((incomingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_MASK) != (uint)ENET_PROTOCOL_COMMAND_SEND_FRAGMENT ||
                        totalLength != incomingCommand->packet->dataLength ||
                        fragmentCount != incomingCommand->fragmentCount)
                        return -1;

                    startCommand = incomingCommand;
                    break;
                }
            }

            if (startCommand == null)
            {
                ENetProtocol hostCommand = *command;

                hostCommand.header.reliableSequenceNumber = (ushort)startSequenceNumber;

                startCommand = enet_peer_queue_incoming_command(peer, &hostCommand, null, (nint)totalLength, (uint)ENET_PACKET_FLAG_RELIABLE, fragmentCount);
                if (startCommand == null)
                    return -1;
            }

            if ((startCommand->fragments[fragmentNumber / 32] & (1u << (int)(fragmentNumber % 32))) == 0)
            {
                --startCommand->fragmentsRemaining;

                startCommand->fragments[fragmentNumber / 32] |= (1u << (int)(fragmentNumber % 32));

                if (fragmentOffset + fragmentLength > startCommand->packet->dataLength)
                    fragmentLength = (uint)(startCommand->packet->dataLength - fragmentOffset);

                memcpy(startCommand->packet->data + fragmentOffset,
                    (byte*)command + sizeof(ENetProtocolSendFragment),
                    (nint)fragmentLength);

                if (startCommand->fragmentsRemaining <= 0)
                    enet_peer_dispatch_incoming_reliable_commands(peer, channel, null);
            }

            return 0;
        }

        public static int enet_protocol_handle_send_unreliable_fragment(ENetHost* host, ENetPeer* peer, ENetProtocol* command, byte** currentData)
        {
            uint fragmentNumber,
                fragmentCount,
                fragmentOffset,
                fragmentLength,
                reliableSequenceNumber,
                startSequenceNumber,
                totalLength;
            ushort reliableWindow, currentWindow;
            ENetChannel* channel;
            ENetListNode* currentCommand;
            ENetIncomingCommand* startCommand = null;

            if (command->header.channelID >= peer->channelCount ||
                (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER))
                return -1;

            fragmentLength = ENET_NET_TO_HOST_16(command->sendFragment.dataLength);
            *currentData += fragmentLength;
            if (fragmentLength > host->maximumPacketSize ||
                *currentData < host->receivedData ||
                *currentData > &host->receivedData[host->receivedDataLength])
                return -1;

            channel = &peer->channels[command->header.channelID];
            reliableSequenceNumber = command->header.reliableSequenceNumber;
            startSequenceNumber = ENET_NET_TO_HOST_16(command->sendFragment.startSequenceNumber);

            reliableWindow = (ushort)(reliableSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE);
            currentWindow = (ushort)(channel->incomingReliableSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE);

            if (reliableSequenceNumber < channel->incomingReliableSequenceNumber)
                reliableWindow += (ushort)ENET_PEER_RELIABLE_WINDOWS;

            if (reliableWindow < currentWindow || reliableWindow >= currentWindow + ENET_PEER_FREE_RELIABLE_WINDOWS - 1)
                return 0;

            if (reliableSequenceNumber == channel->incomingReliableSequenceNumber &&
                startSequenceNumber <= channel->incomingUnreliableSequenceNumber)
                return 0;

            fragmentNumber = ENET_NET_TO_HOST_32(command->sendFragment.fragmentNumber);
            fragmentCount = ENET_NET_TO_HOST_32(command->sendFragment.fragmentCount);
            fragmentOffset = ENET_NET_TO_HOST_32(command->sendFragment.fragmentOffset);
            totalLength = ENET_NET_TO_HOST_32(command->sendFragment.totalLength);

            if (fragmentCount > ENET_PROTOCOL_MAXIMUM_FRAGMENT_COUNT ||
                fragmentNumber >= fragmentCount ||
                totalLength > host->maximumPacketSize ||
                fragmentOffset >= totalLength ||
                fragmentLength > totalLength - fragmentOffset)
                return -1;

            for (currentCommand = enet_list_previous(enet_list_end(&channel->incomingUnreliableCommands));
                 currentCommand != enet_list_end(&channel->incomingUnreliableCommands);
                 currentCommand = enet_list_previous(currentCommand))
            {
                ENetIncomingCommand* incomingCommand = (ENetIncomingCommand*)currentCommand;

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

                if (incomingCommand->unreliableSequenceNumber <= startSequenceNumber)
                {
                    if (incomingCommand->unreliableSequenceNumber < startSequenceNumber)
                        break;

                    if ((incomingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_MASK) != (uint)ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE_FRAGMENT ||
                        totalLength != incomingCommand->packet->dataLength ||
                        fragmentCount != incomingCommand->fragmentCount)
                        return -1;

                    startCommand = incomingCommand;
                    break;
                }
            }

            if (startCommand == null)
            {
                startCommand = enet_peer_queue_incoming_command(peer, command, null, (nint)totalLength, (uint)ENET_PACKET_FLAG_UNRELIABLE_FRAGMENT, fragmentCount);
                if (startCommand == null)
                    return -1;
            }

            if ((startCommand->fragments[fragmentNumber / 32] & (1u << (int)(fragmentNumber % 32))) == 0)
            {
                --startCommand->fragmentsRemaining;

                startCommand->fragments[fragmentNumber / 32] |= (1u << (int)(fragmentNumber % 32));

                if (fragmentOffset + fragmentLength > startCommand->packet->dataLength)
                    fragmentLength = (uint)(startCommand->packet->dataLength - fragmentOffset);

                memcpy(startCommand->packet->data + fragmentOffset,
                    (byte*)command + sizeof(ENetProtocolSendFragment),
                    (nint)fragmentLength);

                if (startCommand->fragmentsRemaining <= 0)
                    enet_peer_dispatch_incoming_unreliable_commands(peer, channel, null);
            }

            return 0;
        }

        public static int enet_protocol_handle_ping(ENetHost* host, ENetPeer* peer, ENetProtocol* command)
        {
            if (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER)
                return -1;

            return 0;
        }

        public static int enet_protocol_handle_bandwidth_limit(ENetHost* host, ENetPeer* peer, ENetProtocol* command)
        {
            if (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER)
                return -1;

            if (peer->incomingBandwidth != 0)
                --host->bandwidthLimitedPeers;

            peer->incomingBandwidth = ENET_NET_TO_HOST_32(command->bandwidthLimit.incomingBandwidth);
            peer->outgoingBandwidth = ENET_NET_TO_HOST_32(command->bandwidthLimit.outgoingBandwidth);

            if (peer->incomingBandwidth != 0)
                ++host->bandwidthLimitedPeers;

            if (peer->incomingBandwidth == 0 && host->outgoingBandwidth == 0)
                peer->windowSize = ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE;
            else if (peer->incomingBandwidth == 0 || host->outgoingBandwidth == 0)
                peer->windowSize = (ENET_MAX(peer->incomingBandwidth, host->outgoingBandwidth) /
                                    ENET_PEER_WINDOW_SIZE_SCALE) * ENET_PROTOCOL_MINIMUM_WINDOW_SIZE;
            else
                peer->windowSize = (ENET_MIN(peer->incomingBandwidth, host->outgoingBandwidth) /
                                    ENET_PEER_WINDOW_SIZE_SCALE) * ENET_PROTOCOL_MINIMUM_WINDOW_SIZE;

            if (peer->windowSize < ENET_PROTOCOL_MINIMUM_WINDOW_SIZE)
                peer->windowSize = ENET_PROTOCOL_MINIMUM_WINDOW_SIZE;
            else if (peer->windowSize > ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE)
                peer->windowSize = ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE;

            return 0;
        }

        public static int enet_protocol_handle_throttle_configure(ENetHost* host, ENetPeer* peer, ENetProtocol* command)
        {
            if (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER)
                return -1;

            peer->packetThrottleInterval = ENET_NET_TO_HOST_32(command->throttleConfigure.packetThrottleInterval);
            peer->packetThrottleAcceleration = ENET_NET_TO_HOST_32(command->throttleConfigure.packetThrottleAcceleration);
            peer->packetThrottleDeceleration = ENET_NET_TO_HOST_32(command->throttleConfigure.packetThrottleDeceleration);

            return 0;
        }

        public static int enet_protocol_handle_disconnect(ENetHost* host, ENetPeer* peer, ENetProtocol* command)
        {
            if (peer->state == ENET_PEER_STATE_DISCONNECTED || peer->state == ENET_PEER_STATE_ZOMBIE || peer->state == ENET_PEER_STATE_ACKNOWLEDGING_DISCONNECT)
                return 0;

            enet_peer_reset_queues(peer);

            if (peer->state == ENET_PEER_STATE_CONNECTION_SUCCEEDED || peer->state == ENET_PEER_STATE_DISCONNECTING || peer->state == ENET_PEER_STATE_CONNECTING)
                enet_protocol_dispatch_state(host, peer, ENET_PEER_STATE_ZOMBIE);
            else if (peer->state != ENET_PEER_STATE_CONNECTED && peer->state != ENET_PEER_STATE_DISCONNECT_LATER)
            {
                if (peer->state == ENET_PEER_STATE_CONNECTION_PENDING) host->recalculateBandwidthLimits = 1;

                enet_peer_reset(peer);
            }
            else if ((command->header.command & (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE) != 0)
                enet_protocol_change_state(host, peer, ENET_PEER_STATE_ACKNOWLEDGING_DISCONNECT);
            else
                enet_protocol_dispatch_state(host, peer, ENET_PEER_STATE_ZOMBIE);

            if (peer->state != ENET_PEER_STATE_DISCONNECTED)
                peer->eventData = ENET_NET_TO_HOST_32(command->disconnect.data);

            return 0;
        }

        public static int enet_protocol_handle_acknowledge(ENetHost* host, ENetEvent* @event, ENetPeer* peer, ENetProtocol* command)
        {
            uint roundTripTime,
                receivedSentTime,
                receivedReliableSequenceNumber;
            ENetProtocolCommand commandNumber;

            if (peer->state == ENET_PEER_STATE_DISCONNECTED || peer->state == ENET_PEER_STATE_ZOMBIE)
                return 0;

            receivedSentTime = ENET_NET_TO_HOST_16(command->acknowledge.receivedSentTime);
            receivedSentTime |= host->serviceTime & 0xFFFF0000;
            if ((receivedSentTime & 0x8000) > (host->serviceTime & 0x8000))
                receivedSentTime -= 0x10000;

            if (ENET_TIME_LESS(host->serviceTime, receivedSentTime))
                return 0;

            roundTripTime = ENET_TIME_DIFFERENCE(host->serviceTime, receivedSentTime);
            roundTripTime = ENET_MAX(roundTripTime, 1);

            if (peer->lastReceiveTime > 0)
            {
                enet_peer_throttle(peer, roundTripTime);

                peer->roundTripTimeVariance -= peer->roundTripTimeVariance / 4;

                if (roundTripTime >= peer->roundTripTime)
                {
                    uint diff = roundTripTime - peer->roundTripTime;
                    peer->roundTripTimeVariance += diff / 4;
                    peer->roundTripTime += diff / 8;
                }
                else
                {
                    uint diff = peer->roundTripTime - roundTripTime;
                    peer->roundTripTimeVariance += diff / 4;
                    peer->roundTripTime -= diff / 8;
                }
            }
            else
            {
                peer->roundTripTime = roundTripTime;
                peer->roundTripTimeVariance = (roundTripTime + 1) / 2;
            }

            if (peer->roundTripTime < peer->lowestRoundTripTime)
                peer->lowestRoundTripTime = peer->roundTripTime;

            if (peer->roundTripTimeVariance > peer->highestRoundTripTimeVariance)
                peer->highestRoundTripTimeVariance = peer->roundTripTimeVariance;

            if (peer->packetThrottleEpoch == 0 ||
                ENET_TIME_DIFFERENCE(host->serviceTime, peer->packetThrottleEpoch) >= peer->packetThrottleInterval)
            {
                peer->lastRoundTripTime = peer->lowestRoundTripTime;
                peer->lastRoundTripTimeVariance = ENET_MAX(peer->highestRoundTripTimeVariance, 1);
                peer->lowestRoundTripTime = peer->roundTripTime;
                peer->highestRoundTripTimeVariance = peer->roundTripTimeVariance;
                peer->packetThrottleEpoch = host->serviceTime;
            }

            peer->lastReceiveTime = ENET_MAX(host->serviceTime, 1);
            peer->earliestTimeout = 0;

            receivedReliableSequenceNumber = ENET_NET_TO_HOST_16(command->acknowledge.receivedReliableSequenceNumber);

            commandNumber = enet_protocol_remove_sent_reliable_command(peer, (ushort)receivedReliableSequenceNumber, command->header.channelID);

            switch (peer->state)
            {
                case ENET_PEER_STATE_ACKNOWLEDGING_CONNECT:
                    if (commandNumber != ENET_PROTOCOL_COMMAND_VERIFY_CONNECT)
                        return -1;

                    enet_protocol_notify_connect(host, peer, @event);
                    break;

                case ENET_PEER_STATE_DISCONNECTING:
                    if (commandNumber != ENET_PROTOCOL_COMMAND_DISCONNECT)
                        return -1;

                    enet_protocol_notify_disconnect(host, peer, @event);
                    break;

                case ENET_PEER_STATE_DISCONNECT_LATER:
                    if (!(enet_peer_has_outgoing_commands(peer) != 0))
                        enet_peer_disconnect(peer, peer->eventData);
                    break;

                default:
                    break;
            }

            return 0;
        }

        public static int enet_protocol_handle_verify_connect(ENetHost* host, ENetEvent* @event, ENetPeer* peer, ENetProtocol* command)
        {
            uint mtu, windowSize;
            nint channelCount;

            if (peer->state != ENET_PEER_STATE_CONNECTING)
                return 0;

            channelCount = (nint)ENET_NET_TO_HOST_32(command->verifyConnect.channelCount);

            if (channelCount < ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT || channelCount > ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT ||
                ENET_NET_TO_HOST_32(command->verifyConnect.packetThrottleInterval) != peer->packetThrottleInterval ||
                ENET_NET_TO_HOST_32(command->verifyConnect.packetThrottleAcceleration) != peer->packetThrottleAcceleration ||
                ENET_NET_TO_HOST_32(command->verifyConnect.packetThrottleDeceleration) != peer->packetThrottleDeceleration ||
                command->verifyConnect.connectID != peer->connectID)
            {
                peer->eventData = 0;

                enet_protocol_dispatch_state(host, peer, ENET_PEER_STATE_ZOMBIE);

                return -1;
            }

            enet_protocol_remove_sent_reliable_command(peer, 1, 0xFF);

            if (channelCount < peer->channelCount)
                peer->channelCount = channelCount;

            peer->outgoingPeerID = ENET_NET_TO_HOST_16(command->verifyConnect.outgoingPeerID);
            peer->incomingSessionID = command->verifyConnect.incomingSessionID;
            peer->outgoingSessionID = command->verifyConnect.outgoingSessionID;

            mtu = ENET_NET_TO_HOST_32(command->verifyConnect.mtu);

            if (mtu < ENET_PROTOCOL_MINIMUM_MTU)
                mtu = ENET_PROTOCOL_MINIMUM_MTU;
            else if (mtu > ENET_PROTOCOL_MAXIMUM_MTU)
                mtu = ENET_PROTOCOL_MAXIMUM_MTU;

            if (mtu < peer->mtu)
                peer->mtu = mtu;

            windowSize = ENET_NET_TO_HOST_32(command->verifyConnect.windowSize);

            if (windowSize < ENET_PROTOCOL_MINIMUM_WINDOW_SIZE)
                windowSize = ENET_PROTOCOL_MINIMUM_WINDOW_SIZE;

            if (windowSize > ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE)
                windowSize = ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE;

            if (windowSize < peer->windowSize)
                peer->windowSize = windowSize;

            peer->incomingBandwidth = ENET_NET_TO_HOST_32(command->verifyConnect.incomingBandwidth);
            peer->outgoingBandwidth = ENET_NET_TO_HOST_32(command->verifyConnect.outgoingBandwidth);

            enet_protocol_notify_connect(host, peer, @event);
            return 0;
        }

        public static int enet_protocol_handle_incoming_commands(ENetHost* host, ENetEvent* @event)
        {
            ENetProtocolHeader* header;
            ENetProtocol* command;
            ENetPeer* peer;
            byte* currentData;
            nint headerSize;
            ushort peerID, flags;
            byte sessionID;

            if (host->receivedDataLength < 2)
                return 0;

            header = (ENetProtocolHeader*)host->receivedData;

            peerID = ENET_NET_TO_HOST_16(header->peerID);
            sessionID = (byte)((peerID & (uint)ENET_PROTOCOL_HEADER_SESSION_MASK) >> (int)ENET_PROTOCOL_HEADER_SESSION_SHIFT);
            flags = (ushort)(peerID & (uint)ENET_PROTOCOL_HEADER_FLAG_MASK);
            uint newPeerID = peerID;
            newPeerID &= ~ ((uint)ENET_PROTOCOL_HEADER_FLAG_MASK | (uint)ENET_PROTOCOL_HEADER_SESSION_MASK);
            peerID = (ushort)newPeerID;
            headerSize = (((flags & (uint)ENET_PROTOCOL_HEADER_FLAG_SENT_TIME) != 0) ? sizeof(ENetProtocolHeader) : 2);
            if (host->checksum != null)
                headerSize += sizeof(uint);

            if (peerID == ENET_PROTOCOL_MAXIMUM_PEER_ID)
                peer = null;
            else if (peerID >= host->peerCount)
                return 0;
            else
            {
                peer = &host->peers[peerID];

                if (peer->state == ENET_PEER_STATE_DISCONNECTED ||
                    peer->state == ENET_PEER_STATE_ZOMBIE ||
                    ((host->receivedAddress.host != peer->address.host ||
                      host->receivedAddress.port != peer->address.port) &&
                     peer->address.host != ENET_HOST_BROADCAST) ||
                    (peer->outgoingPeerID < ENET_PROTOCOL_MAXIMUM_PEER_ID &&
                     sessionID != peer->incomingSessionID))
                    return 0;
            }

            if ((flags & (uint)ENET_PROTOCOL_HEADER_FLAG_COMPRESSED) != 0)
            {
                nint originalSize;
                if (host->compressor.context == null || host->compressor.decompress == null)
                    return 0;

                originalSize = host->compressor.decompress(host->compressor.context,
                    host->receivedData + headerSize,
                    host->receivedDataLength - headerSize,
                    host->packetData[1] + headerSize,
                    4096 - headerSize);
                if (originalSize <= 0 || originalSize > 4096 - headerSize)
                    return 0;

                memcpy(host->packetData[1], header, headerSize);
                host->receivedData = host->packetData[1];
                host->receivedDataLength = headerSize + originalSize;
            }

            if (host->checksum != null)
            {
                uint* checksum = (uint*)&host->receivedData[headerSize - sizeof(uint)];
                uint desiredChecksum, newChecksum;
                ENetBuffer buffer;

                memcpy(&desiredChecksum, checksum, sizeof(uint));

                newChecksum = peer != null ? peer->connectID : 0;
                memcpy(checksum, &newChecksum, sizeof(uint));

                buffer.data = host->receivedData;
                buffer.dataLength = host->receivedDataLength;

                if (host->checksum(&buffer, 1) != desiredChecksum)
                    return 0;
            }

            if (peer != null)
            {
                peer->address.host = host->receivedAddress.host;
                peer->address.port = host->receivedAddress.port;
                peer->incomingDataTotal += (uint)host->receivedDataLength;
            }

            currentData = host->receivedData + headerSize;

            while (currentData < &host->receivedData[host->receivedDataLength])
            {
                byte commandNumber;
                nint commandSize;

                command = (ENetProtocol*)currentData;

                if (currentData + sizeof(ENetProtocolCommandHeader) > &host->receivedData[host->receivedDataLength])
                    break;

                commandNumber = (byte)(command->header.command & (uint)ENET_PROTOCOL_COMMAND_MASK);
                if (commandNumber >= (uint)ENET_PROTOCOL_COMMAND_COUNT)
                    break;

                commandSize = commandSizes[commandNumber];
                if (commandSize == 0 || currentData + commandSize > &host->receivedData[host->receivedDataLength])
                    break;

                currentData += commandSize;

                if (peer == null && commandNumber != (uint)ENET_PROTOCOL_COMMAND_CONNECT)
                    break;

                command->header.reliableSequenceNumber = ENET_NET_TO_HOST_16(command->header.reliableSequenceNumber);

                switch (commandNumber)
                {
                    case (byte)ENET_PROTOCOL_COMMAND_ACKNOWLEDGE:
                        if (enet_protocol_handle_acknowledge(host, @event, peer, command) != 0)
                            goto commandError;
                        break;

                    case (byte)ENET_PROTOCOL_COMMAND_CONNECT:
                        if (peer != null)
                            goto commandError;
                        peer = enet_protocol_handle_connect(host, header, command);
                        if (peer == null)
                            goto commandError;
                        break;

                    case (byte)ENET_PROTOCOL_COMMAND_VERIFY_CONNECT:
                        if (enet_protocol_handle_verify_connect(host, @event, peer, command) != 0)
                            goto commandError;
                        break;

                    case (byte)ENET_PROTOCOL_COMMAND_DISCONNECT:
                        if (enet_protocol_handle_disconnect(host, peer, command) != 0)
                            goto commandError;
                        break;

                    case (byte)ENET_PROTOCOL_COMMAND_PING:
                        if (enet_protocol_handle_ping(host, peer, command) != 0)
                            goto commandError;
                        break;

                    case (byte)ENET_PROTOCOL_COMMAND_SEND_RELIABLE:
                        if (enet_protocol_handle_send_reliable(host, peer, command, &currentData) != 0)
                            goto commandError;
                        break;

                    case (byte)ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE:
                        if (enet_protocol_handle_send_unreliable(host, peer, command, &currentData) != 0)
                            goto commandError;
                        break;

                    case (byte)ENET_PROTOCOL_COMMAND_SEND_UNSEQUENCED:
                        if (enet_protocol_handle_send_unsequenced(host, peer, command, &currentData) != 0)
                            goto commandError;
                        break;

                    case (byte)ENET_PROTOCOL_COMMAND_SEND_FRAGMENT:
                        if (enet_protocol_handle_send_fragment(host, peer, command, &currentData) != 0)
                            goto commandError;
                        break;

                    case (byte)ENET_PROTOCOL_COMMAND_BANDWIDTH_LIMIT:
                        if (enet_protocol_handle_bandwidth_limit(host, peer, command) != 0)
                            goto commandError;
                        break;

                    case (byte)ENET_PROTOCOL_COMMAND_THROTTLE_CONFIGURE:
                        if (enet_protocol_handle_throttle_configure(host, peer, command) != 0)
                            goto commandError;
                        break;

                    case (byte)ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE_FRAGMENT:
                        if (enet_protocol_handle_send_unreliable_fragment(host, peer, command, &currentData) != 0)
                            goto commandError;
                        break;

                    default:
                        goto commandError;
                }

                if (peer != null &&
                    (command->header.command & (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE) != 0)
                {
                    ushort sentTime;

                    if (!((flags & (uint)ENET_PROTOCOL_HEADER_FLAG_SENT_TIME) != 0))
                        break;

                    sentTime = ENET_NET_TO_HOST_16(header->sentTime);

                    switch (peer->state)
                    {
                        case ENET_PEER_STATE_DISCONNECTING:
                        case ENET_PEER_STATE_ACKNOWLEDGING_CONNECT:
                        case ENET_PEER_STATE_DISCONNECTED:
                        case ENET_PEER_STATE_ZOMBIE:
                            break;

                        case ENET_PEER_STATE_ACKNOWLEDGING_DISCONNECT:
                            if ((command->header.command & (uint)ENET_PROTOCOL_COMMAND_MASK) == (uint)ENET_PROTOCOL_COMMAND_DISCONNECT)
                                enet_peer_queue_acknowledgement(peer, command, sentTime);
                            break;

                        default:
                            enet_peer_queue_acknowledgement(peer, command, sentTime);
                            break;
                    }
                }
            }

            commandError:
            if (@event != null && @event->type != ENET_EVENT_TYPE_NONE)
                return 1;

            return 0;
        }

        public static int enet_protocol_receive_incoming_commands(ENetHost* host, ENetEvent* @event)
        {
            int packets;

            for (packets = 0; packets < 256; ++packets)
            {
                int receivedLength;
                ENetBuffer buffer;

                buffer.data = host->packetData[0];
                buffer.dataLength = 4096;

                receivedLength = enet_socket_receive(host->socket,
                    &host->receivedAddress,
                    &buffer);

                if (receivedLength == -2)
                    continue;

                if (receivedLength < 0)
                    return -1;

                if (receivedLength == 0)
                    return 0;

                host->receivedData = host->packetData[0];
                host->receivedDataLength = receivedLength;

                host->totalReceivedData += (uint)receivedLength;
                host->totalReceivedPackets++;

                if (host->intercept != null)
                {
                    switch (host->intercept(host, @event))
                    {
                        case 1:
                            if (@event != null && @event->type != ENET_EVENT_TYPE_NONE)
                                return 1;

                            continue;

                        case -1:
                            return -1;

                        default:
                            break;
                    }
                }

                switch (enet_protocol_handle_incoming_commands(host, @event))
                {
                    case 1:
                        return 1;

                    case -1:
                        return -1;

                    default:
                        break;
                }
            }

            return 0;
        }

        public static void enet_protocol_send_acknowledgements(ENetHost* host, ENetPeer* peer)
        {
            ENetProtocol* command = &host->commands[host->commandCount];
            ENetBuffer* buffer = &host->buffers[host->bufferCount];
            ENetAcknowledgement* acknowledgement;
            ENetListNode* currentAcknowledgement;
            ushort reliableSequenceNumber;

            currentAcknowledgement = enet_list_begin(&peer->acknowledgements);

            while (currentAcknowledgement != enet_list_end(&peer->acknowledgements))
            {
                if (command >= &host->commands[ENET_PROTOCOL_MAXIMUM_PACKET_COMMANDS] ||
                    buffer >= &host->buffers[ENET_BUFFER_MAXIMUM] ||
                    peer->mtu - host->packetSize < sizeof(ENetProtocolAcknowledge))
                {
                    uint flags = peer->flags;
                    flags |= (uint)ENET_PEER_FLAG_CONTINUE_SENDING;
                    peer->flags = (ushort)flags;

                    break;
                }

                acknowledgement = (ENetAcknowledgement*)currentAcknowledgement;

                currentAcknowledgement = enet_list_next(currentAcknowledgement);

                buffer->data = command;
                buffer->dataLength = sizeof(ENetProtocolAcknowledge);

                host->packetSize += buffer->dataLength;

                reliableSequenceNumber = ENET_HOST_TO_NET_16(acknowledgement->command.header.reliableSequenceNumber);

                command->header.command = (byte)ENET_PROTOCOL_COMMAND_ACKNOWLEDGE;
                command->header.channelID = acknowledgement->command.header.channelID;
                command->header.reliableSequenceNumber = reliableSequenceNumber;
                command->acknowledge.receivedReliableSequenceNumber = reliableSequenceNumber;
                command->acknowledge.receivedSentTime = ENET_HOST_TO_NET_16((ushort)acknowledgement->sentTime);

                if ((acknowledgement->command.header.command & (uint)ENET_PROTOCOL_COMMAND_MASK) == (uint)ENET_PROTOCOL_COMMAND_DISCONNECT)
                    enet_protocol_dispatch_state(host, peer, ENET_PEER_STATE_ZOMBIE);

                enet_list_remove(&acknowledgement->acknowledgementList);
                enet_free(acknowledgement);

                ++command;
                ++buffer;
            }

            host->commandCount = (nint)(command - host->commands);
            host->bufferCount = (nint)(buffer - host->buffers);
        }

        public static int enet_protocol_check_timeouts(ENetHost* host, ENetPeer* peer, ENetEvent* @event)
        {
            ENetOutgoingCommand* outgoingCommand;
            ENetListNode* currentCommand, insertPosition, insertSendReliablePosition;

            currentCommand = enet_list_begin(&peer->sentReliableCommands);
            insertPosition = enet_list_begin(&peer->outgoingCommands);
            insertSendReliablePosition = enet_list_begin(&peer->outgoingSendReliableCommands);

            while (currentCommand != enet_list_end(&peer->sentReliableCommands))
            {
                outgoingCommand = (ENetOutgoingCommand*)currentCommand;

                currentCommand = enet_list_next(currentCommand);

                if (ENET_TIME_DIFFERENCE(host->serviceTime, outgoingCommand->sentTime) < outgoingCommand->roundTripTimeout)
                    continue;

                if (peer->earliestTimeout == 0 ||
                    ENET_TIME_LESS(outgoingCommand->sentTime, peer->earliestTimeout))
                    peer->earliestTimeout = outgoingCommand->sentTime;

                if (peer->earliestTimeout != 0 &&
                    (ENET_TIME_DIFFERENCE(host->serviceTime, peer->earliestTimeout) >= peer->timeoutMaximum ||
                     ((1u << (outgoingCommand->sendAttempts - 1)) >= peer->timeoutLimit &&
                      ENET_TIME_DIFFERENCE(host->serviceTime, peer->earliestTimeout) >= peer->timeoutMinimum)))
                {
                    enet_protocol_notify_disconnect(host, peer, @event);

                    return 1;
                }

                ++peer->packetsLost;

                outgoingCommand->roundTripTimeout *= 2;

                if (outgoingCommand->packet != null)
                {
                    peer->reliableDataInTransit -= outgoingCommand->fragmentLength;

                    enet_list_insert(insertSendReliablePosition, enet_list_remove(&outgoingCommand->outgoingCommandList));
                }
                else
                    enet_list_insert(insertPosition, enet_list_remove(&outgoingCommand->outgoingCommandList));

                if (currentCommand == enet_list_begin(&peer->sentReliableCommands) &&
                    !enet_list_empty(&peer->sentReliableCommands))
                {
                    outgoingCommand = (ENetOutgoingCommand*)currentCommand;

                    peer->nextTimeout = outgoingCommand->sentTime + outgoingCommand->roundTripTimeout;
                }
            }

            return 0;
        }

        public static int enet_protocol_check_outgoing_commands(ENetHost* host, ENetPeer* peer, ENetList* sentUnreliableCommands)
        {
            ENetProtocol* command = &host->commands[host->commandCount];
            ENetBuffer* buffer = &host->buffers[host->bufferCount];
            ENetOutgoingCommand* outgoingCommand;
            ENetListNode* currentCommand, currentSendReliableCommand;
            ENetChannel* channel = null;
            ushort reliableWindow = 0;
            nint commandSize;
            int windowWrap = 0, canPing = 1;

            currentCommand = enet_list_begin(&peer->outgoingCommands);
            currentSendReliableCommand = enet_list_begin(&peer->outgoingSendReliableCommands);

            for (;;)
            {
                if (currentCommand != enet_list_end(&peer->outgoingCommands))
                {
                    outgoingCommand = (ENetOutgoingCommand*)currentCommand;

                    if (currentSendReliableCommand != enet_list_end(&peer->outgoingSendReliableCommands) &&
                        ENET_TIME_LESS(((ENetOutgoingCommand*)currentSendReliableCommand)->queueTime, outgoingCommand->queueTime))
                    {
                        outgoingCommand = (ENetOutgoingCommand*)currentSendReliableCommand;
                        currentSendReliableCommand = enet_list_next(currentSendReliableCommand);
                    }
                    else
                    {
                        currentCommand = enet_list_next(currentCommand);
                    }
                }
                else if (currentSendReliableCommand != enet_list_end(&peer->outgoingSendReliableCommands))
                {
                    outgoingCommand = (ENetOutgoingCommand*)currentSendReliableCommand;
                    currentSendReliableCommand = enet_list_next(currentSendReliableCommand);
                }
                else
                    break;

                if ((outgoingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE) != 0)
                {
                    channel = outgoingCommand->command.header.channelID < peer->channelCount ? &peer->channels[outgoingCommand->command.header.channelID] : null;
                    reliableWindow = (ushort)(outgoingCommand->reliableSequenceNumber / ENET_PEER_RELIABLE_WINDOW_SIZE);
                    if (channel != null)
                    {
                        if (windowWrap != 0)
                            continue;
                        else if (outgoingCommand->sendAttempts < 1 &&
                                 !((outgoingCommand->reliableSequenceNumber % ENET_PEER_RELIABLE_WINDOW_SIZE) != 0) &&
                                 (channel->reliableWindows[(reliableWindow + ENET_PEER_RELIABLE_WINDOWS - 1) % ENET_PEER_RELIABLE_WINDOWS] >= ENET_PEER_RELIABLE_WINDOW_SIZE ||
                                  (channel->usedReliableWindows & ((((1u << (int)(ENET_PEER_FREE_RELIABLE_WINDOWS + 2)) - 1) << reliableWindow) |
                                                                   (((1u << (int)(ENET_PEER_FREE_RELIABLE_WINDOWS + 2)) - 1) >> (int)(ENET_PEER_RELIABLE_WINDOWS - reliableWindow)))) != 0))
                        {
                            windowWrap = 1;
                            currentSendReliableCommand = enet_list_end(&peer->outgoingSendReliableCommands);

                            continue;
                        }
                    }

                    if (outgoingCommand->packet != null)
                    {
                        uint windowSize = (peer->packetThrottle * peer->windowSize) / ENET_PEER_PACKET_THROTTLE_SCALE;

                        if (peer->reliableDataInTransit + outgoingCommand->fragmentLength > ENET_MAX(windowSize, peer->mtu))
                        {
                            currentSendReliableCommand = enet_list_end(&peer->outgoingSendReliableCommands);

                            continue;
                        }
                    }

                    canPing = 0;
                }

                commandSize = commandSizes[(int)(outgoingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_MASK)];
                if (command >= &host->commands[ENET_PROTOCOL_MAXIMUM_PACKET_COMMANDS] ||
                    buffer + 1 >= &host->buffers[ENET_BUFFER_MAXIMUM] ||
                    peer->mtu - host->packetSize < commandSize ||
                    (outgoingCommand->packet != null &&
                     (ushort)(peer->mtu - host->packetSize) < (ushort)(commandSize + outgoingCommand->fragmentLength)))
                {
                    uint flags = peer->flags;
                    flags |= (uint)ENET_PEER_FLAG_CONTINUE_SENDING;
                    peer->flags = (ushort)flags;

                    break;
                }

                if ((outgoingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE) != 0)
                {
                    if (channel != null && outgoingCommand->sendAttempts < 1)
                    {
                        uint usedReliableWindows = channel->usedReliableWindows;
                        usedReliableWindows |= 1u << reliableWindow;
                        channel->usedReliableWindows = (ushort)usedReliableWindows;
                        ++channel->reliableWindows[reliableWindow];
                    }

                    ++outgoingCommand->sendAttempts;

                    if (outgoingCommand->roundTripTimeout == 0)
                        outgoingCommand->roundTripTimeout = peer->roundTripTime + 4 * peer->roundTripTimeVariance;

                    if (enet_list_empty(&peer->sentReliableCommands))
                        peer->nextTimeout = host->serviceTime + outgoingCommand->roundTripTimeout;

                    enet_list_insert(enet_list_end(&peer->sentReliableCommands),
                        enet_list_remove(&outgoingCommand->outgoingCommandList));

                    outgoingCommand->sentTime = host->serviceTime;

                    uint headerFlags = host->headerFlags;
                    headerFlags |= (uint)ENET_PROTOCOL_HEADER_FLAG_SENT_TIME;
                    host->headerFlags = (ushort)headerFlags;

                    peer->reliableDataInTransit += outgoingCommand->fragmentLength;
                }
                else
                {
                    if (outgoingCommand->packet != null && outgoingCommand->fragmentOffset == 0)
                    {
                        peer->packetThrottleCounter += ENET_PEER_PACKET_THROTTLE_COUNTER;
                        peer->packetThrottleCounter %= ENET_PEER_PACKET_THROTTLE_SCALE;

                        if (peer->packetThrottleCounter > peer->packetThrottle)
                        {
                            ushort reliableSequenceNumber = outgoingCommand->reliableSequenceNumber,
                                unreliableSequenceNumber = outgoingCommand->unreliableSequenceNumber;
                            for (;;)
                            {
                                --outgoingCommand->packet->referenceCount;

                                if (outgoingCommand->packet->referenceCount == 0)
                                    enet_packet_destroy(outgoingCommand->packet);

                                enet_list_remove(&outgoingCommand->outgoingCommandList);
                                enet_free(outgoingCommand);

                                if (currentCommand == enet_list_end(&peer->outgoingCommands))
                                    break;

                                outgoingCommand = (ENetOutgoingCommand*)currentCommand;
                                if (outgoingCommand->reliableSequenceNumber != reliableSequenceNumber ||
                                    outgoingCommand->unreliableSequenceNumber != unreliableSequenceNumber)
                                    break;

                                currentCommand = enet_list_next(currentCommand);
                            }

                            continue;
                        }
                    }

                    enet_list_remove(&outgoingCommand->outgoingCommandList);

                    if (outgoingCommand->packet != null)
                        enet_list_insert(enet_list_end(sentUnreliableCommands), outgoingCommand);
                }

                buffer->data = command;
                buffer->dataLength = commandSize;

                host->packetSize += buffer->dataLength;

                *command = outgoingCommand->command;

                if (outgoingCommand->packet != null)
                {
                    ++buffer;

                    buffer->data = outgoingCommand->packet->data + outgoingCommand->fragmentOffset;
                    buffer->dataLength = outgoingCommand->fragmentLength;

                    host->packetSize += outgoingCommand->fragmentLength;
                }
                else if (!((outgoingCommand->command.header.command & (uint)ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE) != 0))
                    enet_free(outgoingCommand);

                ++peer->packetsSent;

                ++command;
                ++buffer;
            }

            host->commandCount = (nint)(command - host->commands);
            host->bufferCount = (nint)(buffer - host->buffers);

            if (peer->state == ENET_PEER_STATE_DISCONNECT_LATER &&
                !(enet_peer_has_outgoing_commands(peer) != 0) &&
                enet_list_empty(sentUnreliableCommands))
                enet_peer_disconnect(peer, peer->eventData);

            return canPing;
        }

        public static int enet_protocol_send_outgoing_commands(ENetHost* host, ENetEvent* @event, int checkForTimeouts)
        {
            byte* headerData = stackalloc byte[sizeof(ENetProtocolHeader) + sizeof(uint)];
            ENetProtocolHeader* header = (ENetProtocolHeader*)headerData;
            int sentLength = 0;
            nint shouldCompress = 0;
            ENetList sentUnreliableCommands;

            enet_list_clear(&sentUnreliableCommands);

            for (int sendPass = 0, continueSending = 0; sendPass <= continueSending; ++sendPass)
            for (ENetPeer* currentPeer = host->peers;
                 currentPeer < &host->peers[host->peerCount];
                 ++currentPeer)
            {
                if (currentPeer->state == ENET_PEER_STATE_DISCONNECTED ||
                    currentPeer->state == ENET_PEER_STATE_ZOMBIE ||
                    (sendPass > 0 && !((currentPeer->flags & (uint)ENET_PEER_FLAG_CONTINUE_SENDING) != 0)))
                    continue;

                uint flags = currentPeer->flags;
                flags &= ~ (uint)ENET_PEER_FLAG_CONTINUE_SENDING;
                currentPeer->flags = (ushort)flags;

                host->headerFlags = 0;
                host->commandCount = 0;
                host->bufferCount = 1;
                host->packetSize = sizeof(ENetProtocolHeader);

                if (!enet_list_empty(&currentPeer->acknowledgements))
                    enet_protocol_send_acknowledgements(host, currentPeer);

                if (checkForTimeouts != 0 &&
                    !enet_list_empty(&currentPeer->sentReliableCommands) &&
                    ENET_TIME_GREATER_EQUAL(host->serviceTime, currentPeer->nextTimeout) &&
                    enet_protocol_check_timeouts(host, currentPeer, @event) == 1)
                {
                    if (@event != null && @event->type != ENET_EVENT_TYPE_NONE)
                        return 1;
                    else
                        goto nextPeer;
                }

                if (((enet_list_empty(&currentPeer->outgoingCommands) &&
                      enet_list_empty(&currentPeer->outgoingSendReliableCommands)) ||
                     enet_protocol_check_outgoing_commands(host, currentPeer, &sentUnreliableCommands) != 0) &&
                    enet_list_empty(&currentPeer->sentReliableCommands) &&
                    ENET_TIME_DIFFERENCE(host->serviceTime, currentPeer->lastReceiveTime) >= currentPeer->pingInterval &&
                    currentPeer->mtu - host->packetSize >= sizeof(ENetProtocolPing))
                {
                    enet_peer_ping(currentPeer);
                    enet_protocol_check_outgoing_commands(host, currentPeer, &sentUnreliableCommands);
                }

                if (host->commandCount == 0)
                    goto nextPeer;

                if (currentPeer->packetLossEpoch == 0)
                    currentPeer->packetLossEpoch = host->serviceTime;
                else if (ENET_TIME_DIFFERENCE(host->serviceTime, currentPeer->packetLossEpoch) >= ENET_PEER_PACKET_LOSS_INTERVAL &&
                         currentPeer->packetsSent > 0)
                {
                    uint packetLoss = currentPeer->packetsLost * ENET_PEER_PACKET_LOSS_SCALE / currentPeer->packetsSent;

                    currentPeer->packetLossVariance = (currentPeer->packetLossVariance * 3 + ENET_DIFFERENCE(packetLoss, currentPeer->packetLoss)) / 4;
                    currentPeer->packetLoss = (currentPeer->packetLoss * 7 + packetLoss) / 8;

                    currentPeer->packetLossEpoch = host->serviceTime;
                    currentPeer->packetsSent = 0;
                    currentPeer->packetsLost = 0;
                }

                host->buffers->data = headerData;
                if ((host->headerFlags & (uint)ENET_PROTOCOL_HEADER_FLAG_SENT_TIME) != 0)
                {
                    header->sentTime = ENET_HOST_TO_NET_16((ushort)(host->serviceTime & 0xFFFF));

                    host->buffers->dataLength = sizeof(ENetProtocolHeader);
                }
                else
                    host->buffers->dataLength = 2;

                shouldCompress = 0;
                if (host->compressor.context != null && host->compressor.compress != null)
                {
                    nint originalSize = host->packetSize - sizeof(ENetProtocolHeader),
                        compressedSize = host->compressor.compress(host->compressor.context,
                            &host->buffers[1], host->bufferCount - 1,
                            originalSize,
                            host->packetData[1],
                            originalSize);
                    if (compressedSize > 0 && compressedSize < originalSize)
                    {
                        uint headerFlags = host->headerFlags;
                        headerFlags |= (uint)ENET_PROTOCOL_HEADER_FLAG_COMPRESSED;
                        host->headerFlags = (ushort)headerFlags;
                        shouldCompress = compressedSize;
                    }
                }

                if (currentPeer->outgoingPeerID < ENET_PROTOCOL_MAXIMUM_PEER_ID)
                {
                    uint headerFlags = host->headerFlags;
                    headerFlags |= (uint)currentPeer->outgoingSessionID << (int)ENET_PROTOCOL_HEADER_SESSION_SHIFT;
                    host->headerFlags = (ushort)headerFlags;
                }

                header->peerID = ENET_HOST_TO_NET_16((ushort)(currentPeer->outgoingPeerID | host->headerFlags));
                if (host->checksum != null)
                {
                    uint* checksum = (uint*)&headerData[host->buffers->dataLength];
                    uint newChecksum = currentPeer->outgoingPeerID < ENET_PROTOCOL_MAXIMUM_PEER_ID ? currentPeer->connectID : 0;
                    memcpy(checksum, &newChecksum, sizeof(uint));
                    host->buffers->dataLength += sizeof(uint);
                    newChecksum = host->checksum(host->buffers, host->bufferCount);
                    memcpy(checksum, &newChecksum, sizeof(uint));
                }

                if (shouldCompress > 0)
                {
                    host->buffers[1].data = host->packetData[1];
                    host->buffers[1].dataLength = shouldCompress;
                    host->bufferCount = 2;
                }

                currentPeer->lastSendTime = host->serviceTime;

                sentLength = enet_socket_send(host->socket, &currentPeer->address, host->buffers, host->bufferCount);

                enet_protocol_remove_sent_unreliable_commands(currentPeer, &sentUnreliableCommands);

                if (sentLength < 0)
                    return -1;

                host->totalSentData += (uint)sentLength;
                host->totalSentPackets++;

                nextPeer:
                if ((currentPeer->flags & (uint)ENET_PEER_FLAG_CONTINUE_SENDING) != 0)
                    continueSending = sendPass + 1;
            }

            return 0;
        }

        public static void enet_host_flush(ENetHost* host)
        {
            host->serviceTime = enet_time_get();

            enet_protocol_send_outgoing_commands(host, null, 0);
        }

        public static int enet_host_check_events(ENetHost* host, ENetEvent* @event)
        {
            if (@event == null) return -1;

            @event->type = ENET_EVENT_TYPE_NONE;
            @event->peer = null;
            @event->packet = null;

            return enet_protocol_dispatch_incoming_commands(host, @event);
        }

        public static int enet_host_service(ENetHost* host, ENetEvent* @event, uint timeout)
        {
            uint waitCondition;

            if (@event != null)
            {
                @event->type = ENET_EVENT_TYPE_NONE;
                @event->peer = null;
                @event->packet = null;

                switch (enet_protocol_dispatch_incoming_commands(host, @event))
                {
                    case 1:
                        return 1;

                    case -1:

                        return -1;

                    default:
                        break;
                }
            }

            host->serviceTime = enet_time_get();

            timeout += host->serviceTime;

            do
            {
                if (ENET_TIME_DIFFERENCE(host->serviceTime, host->bandwidthThrottleEpoch) >= ENET_HOST_BANDWIDTH_THROTTLE_INTERVAL)
                    enet_host_bandwidth_throttle(host);

                switch (enet_protocol_send_outgoing_commands(host, @event, 1))
                {
                    case 1:
                        return 1;

                    case -1:

                        return -1;

                    default:
                        break;
                }

                switch (enet_protocol_receive_incoming_commands(host, @event))
                {
                    case 1:
                        return 1;

                    case -1:

                        return -1;

                    default:
                        break;
                }

                switch (enet_protocol_send_outgoing_commands(host, @event, 1))
                {
                    case 1:
                        return 1;

                    case -1:

                        return -1;

                    default:
                        break;
                }

                if (@event != null)
                {
                    switch (enet_protocol_dispatch_incoming_commands(host, @event))
                    {
                        case 1:
                            return 1;

                        case -1:

                            return -1;

                        default:
                            break;
                    }
                }

                if (ENET_TIME_GREATER_EQUAL(host->serviceTime, timeout))
                    return 0;

                do
                {
                    host->serviceTime = enet_time_get();

                    if (ENET_TIME_GREATER_EQUAL(host->serviceTime, timeout))
                        return 0;

                    waitCondition = ((uint)ENET_SOCKET_WAIT_RECEIVE | (uint)ENET_SOCKET_WAIT_INTERRUPT);

                    if (enet_socket_wait(host->socket, &waitCondition, ENET_TIME_DIFFERENCE(timeout, host->serviceTime)) != 0)
                        return -1;
                } while ((waitCondition & (uint)ENET_SOCKET_WAIT_INTERRUPT) != 0);

                host->serviceTime = enet_time_get();
            } while ((waitCondition & (uint)ENET_SOCKET_WAIT_RECEIVE) != 0);

            return 0;
        }
    }
}