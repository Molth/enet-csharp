using NativeCollections;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     Provides validation helpers for peer uid tables.
    /// </summary>
    internal static class EnetUidsExtensions
    {
        /// <summary>
        ///     Determines whether the specified uid is currently valid for its peer slot in the uid table.
        /// </summary>
        /// <param name="uids">The uid table maintained by the host.</param>
        /// <param name="uid">The uid to validate.</param>
        /// <returns>
        ///     <see langword="true" /> if the peer slot is within range and still holds the same generation value;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public static bool Validate(ref this NativeArray<EnetUid> uids, EnetUid uid) => uid.IncomingPeerId < uids.Length && uids[uid.IncomingPeerId] == uid;
    }
}