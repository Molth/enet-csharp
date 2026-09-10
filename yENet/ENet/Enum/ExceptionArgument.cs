// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Identifies parameter names for exception messages.
    /// </summary>
    internal enum ExceptionArgument
    {
        /// <summary>
        ///     The address argument.
        /// </summary>
        address,

        /// <summary>
        ///     The handle argument.
        /// </summary>
        handle,

        /// <summary>
        ///     The option argument.
        /// </summary>
        option,

        /// <summary>
        ///     The peer count argument.
        /// </summary>
        peerCount,

        /// <summary>
        ///     A sentinel value marking the end of the enumeration.
        /// </summary>
        _dummy
    }
}