using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using enet;

namespace Enet
{
    /// <summary>
    ///     ENet packet structure.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An ENet data packet that may be sent to or received from a peer. The shown
    ///         fields should only be read and never modified. The data field contains the
    ///         allocated data for the packet. The dataLength fields specifies the length
    ///         of the allocated data. The flags field is either 0 (specifying no flags),
    ///         or a bitwise-or of any combination of the following flags:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <c>ENET_PACKET_FLAG_RELIABLE</c> - packet must be received by the target peer
    ///                 and resend attempts should be made until the packet is delivered
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>ENET_PACKET_FLAG_UNSEQUENCED</c> - packet will not be sequenced with other packets
    ///                 (not supported for reliable packets)
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>ENET_PACKET_FLAG_NO_ALLOCATE</c> - packet will not allocate data, and user must supply it
    ///                 instead
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>ENET_PACKET_FLAG_UNRELIABLE_FRAGMENT</c> - packet will be fragmented using unreliable
    ///                 (instead of reliable) sends if it exceeds the MTU
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>ENET_PACKET_FLAG_SENT</c> - whether the packet has been sent from all queues it has been
    ///                 entered into
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="EnetPacketFlag" />
    public unsafe struct EnetPacket : IIsCreated, IDisposable
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly ENetPacket* _handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public EnetPacket(ENetPacket* handle) => _handle = handle;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ENetPacket* GetInner() => _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _handle != null;

        /// <summary>
        ///     internal use only
        /// </summary>
        public readonly nuint ReferenceCount => _handle->referenceCount;

        /// <summary>
        ///     bitwise-or of ENetPacketFlag constants
        /// </summary>
        public readonly uint Flags => _handle->flags;

        /// <summary>
        ///     allocated data for packet
        /// </summary>
        public readonly byte* Data => _handle->data;

        /// <summary>
        ///     length of data
        /// </summary>
        public readonly nuint DataLength => _handle->dataLength;

        /// <summary>
        ///     function to be called when the packet is no longer in use
        /// </summary>
        public readonly delegate* managed<ENetPacket*, void> FreeCallback => _handle->freeCallback;

        /// <summary>
        ///     application private data, may be freely modified
        /// </summary>
        public readonly void* UserData => _handle->userData;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ENET_API.enet_packet_destroy(_handle);
            this = default;
        }

        /// <summary>
        ///     Attempts to copy the packet's data to the specified destination buffer.
        /// </summary>
        /// <param name="destination">Pointer to the first byte of the destination buffer.</param>
        /// <param name="byteCount">The size of the destination buffer in bytes.</param>
        /// <returns>
        ///     <see langword="true" /> if the packet is valid, has data, and its data length does not exceed
        ///     <paramref name="byteCount" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     This method copies the packet's payload into the caller-provided buffer.
        ///     It ensures that the packet is created,
        ///     has a non-null data pointer,
        ///     and that the data fits within the provided buffer.
        /// </remarks>
        public bool TryCopyTo(void* destination, nuint byteCount)
        {
            var packet = _handle;
            if (packet == null || packet->data == null || packet->dataLength > byteCount)
                return false;

            ENet.memcpy(destination, packet->data, byteCount);
            return true;
        }

        /// <summary>
        ///     Attempts to copy the packet's data to the specified destination buffer.
        /// </summary>
        /// <param name="destination">Reference to the first byte of the destination buffer.</param>
        /// <param name="byteCount">The size of the destination buffer in bytes.</param>
        /// <returns>
        ///     <see langword="true" /> if the packet is valid, has data, and its data length does not exceed
        ///     <paramref name="byteCount" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     This method copies the packet's payload into the caller-provided buffer.
        ///     It ensures that the packet is created,
        ///     has a non-null data pointer,
        ///     and that the data fits within the provided buffer.
        /// </remarks>
        public bool TryCopyTo(ref byte destination, nuint byteCount)
        {
            fixed (byte* pBuffer = &destination)
            {
                return TryCopyTo(pBuffer, byteCount);
            }
        }

        /// <summary>
        ///     Attempts to get a <see cref="Span{Byte}" /> that wraps the packet's data buffer.
        /// </summary>
        /// <param name="result">
        ///     When this method returns, contains a span representing the packet data if successful;
        ///     otherwise, the default span.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the packet is valid, has data, and the data length fits within
        ///     <see cref="int.MaxValue" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     This method does not throw exceptions. If the packet is invalid or the data length is too large,
        ///     it returns <see langword="false" /> and sets <paramref name="result" /> to default.
        /// </remarks>
        public readonly bool TryAsSpan(out Span<byte> result)
        {
            var packet = _handle;
            if (packet == null || packet->data == null || packet->dataLength > int.MaxValue)
            {
                result = default;
                return false;
            }

            result = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(packet->data), (int)packet->dataLength);
            return true;
        }

        /// <summary>
        ///     Returns a <see cref="Span{T}" /> that wraps the packet's data buffer.
        ///     This enables efficient read/write access to the packet payload.
        /// </summary>
        /// <returns>A <see cref="Span{Byte}" /> representing the packet data.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the packet has not been created (i.e., <see cref="IsCreated" /> is <see langword="false" />),
        ///     or if the underlying data pointer is <see langword="null" />.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown if the packet's data length exceeds the maximum representable size of a <see cref="Span{T}" />
        ///     (i.e., greater than <see cref="int.MaxValue" />).
        /// </exception>
        public readonly Span<byte> AsSpan()
        {
            var packet = _handle;
            ThrowHelpers.ThrowIfNull(packet, ExceptionArgument._dummy);
            ThrowHelpers.ThrowIfNull(packet->data, ExceptionArgument._dummy);
            if (packet->dataLength > int.MaxValue)
                ThrowHelpers.ThrowOverflowException();

            return MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(packet->data), (int)packet->dataLength);
        }

        /// <summary>
        ///     Creates a packet that may be sent to a peer.
        /// </summary>
        /// <param name="data">
        ///     initial contents of the packet's data;
        ///     the packet's data will remain uninitialized if data is NULL.
        /// </param>
        /// <param name="dataLength">size of the data allocated for this packet</param>
        /// <param name="flags">flags for this packet as described for the ENetPacket structure.</param>
        /// <param name="freeCallback">function to be called when the packet is no longer in use.</param>
        /// <param name="userData">application private data, may be freely modified</param>
        /// <returns>the packet on success, NULL on failure</returns>
        public static EnetPacket Create(void* data, nuint dataLength, EnetPacketFlag flags, delegate* managed<ENetPacket*, void> freeCallback = null, void* userData = null)
        {
            var packet = ENET_API.enet_packet_create(data, dataLength, (uint)flags);
            packet->freeCallback = freeCallback;
            packet->userData = userData;
            return new EnetPacket(packet);
        }

        /// <summary>
        ///     Creates a packet that may be sent to a peer.
        /// </summary>
        /// <param name="data">
        ///     initial contents of the packet's data;
        ///     the packet's data will remain uninitialized if data is NULL.
        /// </param>
        /// <param name="dataLength">size of the data allocated for this packet</param>
        /// <param name="flags">flags for this packet as described for the ENetPacket structure.</param>
        /// <param name="freeCallback">function to be called when the packet is no longer in use.</param>
        /// <param name="userData">application private data, may be freely modified</param>
        /// <returns>the packet on success, NULL on failure</returns>
        public static EnetPacket Create(ref byte data, nuint dataLength, EnetPacketFlag flags, delegate* managed<ENetPacket*, void> freeCallback, void* userData = null)
        {
            fixed (byte* pData = &data)
            {
                return Create(pData, dataLength, flags, freeCallback, userData);
            }
        }

        /// <summary>
        ///     Creates a packet that may be sent to a peer.
        /// </summary>
        /// <param name="data">
        ///     initial contents of the packet's data;
        ///     the packet's data will remain uninitialized if data is NULL.
        /// </param>
        /// <param name="dataLength">size of the data allocated for this packet</param>
        /// <param name="flags">flags for this packet as described for the ENetPacket structure.</param>
        /// <returns>the packet on success, NULL on failure</returns>
        public static EnetPacket Create(ref byte data, nuint dataLength, EnetPacketFlag flags) => Create(ref data, dataLength, flags, null);

        /// <summary>
        ///     Creates a packet that may be sent to a peer.
        /// </summary>
        /// <param name="data">
        ///     initial contents of the packet's data;
        ///     the packet's data will remain uninitialized if data is NULL.
        /// </param>
        /// <param name="flags">flags for this packet as described for the ENetPacket structure.</param>
        /// <param name="freeCallback">function to be called when the packet is no longer in use.</param>
        /// <param name="userData">application private data, may be freely modified</param>
        /// <returns>the packet on success, NULL on failure</returns>
        public static EnetPacket Create(ReadOnlySpan<byte> data, EnetPacketFlag flags, delegate* managed<ENetPacket*, void> freeCallback, void* userData = null) => Create(ref MemoryMarshal.GetReference(data), (nuint)data.Length, flags, freeCallback, userData);

        /// <summary>
        ///     Creates a packet that may be sent to a peer.
        /// </summary>
        /// <param name="data">
        ///     initial contents of the packet's data;
        ///     the packet's data will remain uninitialized if data is NULL.
        /// </param>
        /// <param name="flags">flags for this packet as described for the ENetPacket structure.</param>
        /// <returns>the packet on success, NULL on failure</returns>
        public static EnetPacket Create(ReadOnlySpan<byte> data, EnetPacketFlag flags) => Create(data, flags, null);

        /// <summary>
        ///     Creates a packet that may be sent to a peer.
        /// </summary>
        /// <param name="dataLength">size of the data allocated for this packet</param>
        /// <param name="flags">flags for this packet as described for the ENetPacket structure.</param>
        /// <param name="freeCallback">function to be called when the packet is no longer in use.</param>
        /// <param name="userData">application private data, may be freely modified</param>
        /// <returns>the packet on success, NULL on failure</returns>
        /// <remarks>the packet's data will remain uninitialized because data is NULL.</remarks>
        public static EnetPacket Create(nuint dataLength, EnetPacketFlag flags, delegate* managed<ENetPacket*, void> freeCallback, void* userData = null) => Create(null, dataLength, flags, freeCallback, userData);

        /// <summary>
        ///     Creates a packet that may be sent to a peer.
        /// </summary>
        /// <param name="dataLength">size of the data allocated for this packet</param>
        /// <param name="flags">flags for this packet as described for the ENetPacket structure.</param>
        /// <returns>the packet on success, NULL on failure</returns>
        /// <remarks>the packet's data will remain uninitialized because data is NULL.</remarks>
        public static EnetPacket Create(nuint dataLength, EnetPacketFlag flags) => Create(dataLength, flags, null);
    }
}