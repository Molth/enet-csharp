using Enet;
using NativeCollections;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     Shared state that connects the public host API with its dedicated background thread.
    /// </summary>
    internal sealed class EnetHostStates
    {
        /// <summary>
        ///     The underlying managed ENet host driven by the background thread.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ManagedEnetHost Host;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        /// <summary>
        ///     The configuration the host was started with.
        /// </summary>
        public EnetHostConfig Config;

        /// <summary>
        ///     An atomic reference counter tracking the active users of this state;
        ///     reaching zero signals the background thread to stop and release all resources.
        /// </summary>
        public UnsafeAtomicUsize Threads;

        /// <summary>
        ///     The lock-free queue carrying events from the background thread to <c>PollEvents</c>.
        /// </summary>
        public NativeSegQueue<EnetIncomingEvent> IncomingEvents;

        /// <summary>
        ///     The lock-free queue carrying commands from the public API to the background thread.
        /// </summary>
        public NativeSegQueue<EnetOutgoingEvent> OutgoingEvents;

        /// <summary>
        ///     The user data attached to the disconnect requests sent to all peers when the host shuts down.
        /// </summary>
        public uint ShutdownEventData;
    }
}