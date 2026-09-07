using System;
using System.Runtime.CompilerServices;
using NativeCollections;

// ReSharper disable ALL

namespace ENet
{
    /// <summary>
    ///     Uniquely identifies a connection within a host by combining the fixed peer slot
    ///     with a monotonically increasing generation value.
    /// </summary>
    public readonly struct EnetUid : IEquatable<EnetUid>, IComparable<EnetUid>
#if NET6_0_OR_GREATER
        , ISpanFormattable
#else
        , IFormattable
#endif
    {
        /// <summary>
        ///     Internal 64‑bit value:
        ///     low 16 bits store the peer slot index,
        ///     high 48 bits store the generation counter.
        /// </summary>
        private readonly ulong _value;

        /// <summary>
        ///     The local peer slot index within the host.
        /// </summary>
        public ushort IncomingPeerId => (ushort)(_value & 0xFFFF);

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnetUid" /> structure with the specified raw value.
        /// </summary>
        /// <param name="value">The raw 64‑bit value representing the uid.</param>
        internal EnetUid(ulong value) => _value = value;

        /// <summary>
        ///     Returns a new <see cref="EnetUid" /> with the generation counter incremented by one,
        ///     keeping the same peer slot index.
        /// </summary>
        /// <returns>A new uid with the generation advanced.</returns>
        internal EnetUid Next() => new(unchecked(_value + 0x10000));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(EnetUid other) => NativeBitwise.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
        ///     object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     <list type="table">
        ///         <listheader>
        ///             <term> Value</term><description> Meaning</description>
        ///         </listheader>
        ///         <item>
        ///             <term> Less than zero</term>
        ///             <description> This instance precedes <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///         <item>
        ///             <term> Zero</term>
        ///             <description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description>
        ///         </item>
        ///         <item>
        ///             <term> Greater than zero</term>
        ///             <description> This instance follows <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public int CompareTo(EnetUid other) => NativeBitwise.Compare(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is EnetUid other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(EnetUid left, EnetUid right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(EnetUid left, EnetUid right) => !left.Equals(right);

        /// <summary>
        ///     Returns information about the socket address.
        /// </summary>
        /// <returns>A string that contains information about this.</returns>
        public override string ToString() => _value.ToString();

        /// <summary>
        ///     Formats the value of the current instance using the specified format.
        /// </summary>
        /// <param name="format">
        ///     The format to use.
        ///     -or-
        ///     A null reference
        ///     to use the default format defined for the type of the IFormattable implementation.
        /// </param>
        /// <param name="formatProvider">
        ///     The provider to use to format the value.
        ///     -or-
        ///     A null reference
        ///     to obtain the numeric format information from the current locale setting of the operating system.
        /// </param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public string ToString(string? format, IFormatProvider? formatProvider) => _value.ToString(format, formatProvider);

        /// <summary>Tries to format the value of the current instance into the provided span of characters.</summary>
        /// <param name="destination">When this method returns, this instance's value formatted as a span of characters.</param>
        /// <param name="charsWritten">
        ///     When this method returns, the number of characters that were written in
        ///     <paramref name="destination" />.
        /// </param>
        /// <param name="format">
        ///     A span containing the characters that represent a standard or custom format string that defines
        ///     the acceptable format for <paramref name="destination" />.
        /// </param>
        /// <param name="provider">
        ///     An optional object that supplies culture-specific formatting information for
        ///     <paramref name="destination" />.
        /// </param>
        /// <returns><see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        ///     An implementation of this interface should produce the same string of characters as an implementation of
        ///     <see cref="IFormattable.ToString(string?, IFormatProvider?)" />
        ///     on the same type.
        ///     TryFormat should return false only if there is not enough space in the destination buffer. Any other failures
        ///     should throw an exception.
        /// </remarks>
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => _value.TryFormat(destination, out charsWritten, format, provider);

        /// <summary>
        ///     Tries to format the current socket address into the provided span.
        /// </summary>
        /// <param name="destination">When this method returns, the socket address as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, the number of characters written into the span.</param>
        /// <returns>
        ///     <see langword="true" /> if the formatting was successful;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public bool TryFormat(Span<char> destination, out int charsWritten) => _value.TryFormat(destination, out charsWritten);
    }
}