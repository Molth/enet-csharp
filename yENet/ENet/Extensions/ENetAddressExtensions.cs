using System;
using System.Net.Sockets;
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Provides extension methods for <see cref="ENetAddress" />.
    /// </summary>
    public static class ENetAddressExtensions
    {
        /// <summary>
        ///     Attempts to format this address as an ip endpoint ("ip:port", or "[ipv6]:port" with
        ///     the scope id when non-zero) into the specified character span.
        /// </summary>
        /// <param name="address">The address to format.</param>
        /// <param name="destination">The span to write the formatted endpoint to.</param>
        /// <param name="charsWritten">
        ///     When this method returns, contains the number of characters written to
        ///     <paramref name="destination" />; set to zero when formatting fails.
        /// </param>
        /// <returns>true if the address was formatted; otherwise, false.</returns>
        public static bool TryFormatAsIpEndPoint(in this ENetAddress address, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;

            if (!address.IsIpv4 && !address.IsIpv6)
                return false;

            var position = 0;

            if (address.IsIpv6)
            {
                if (!TryMoveNext(destination, ref position, '['))
                    return false;
            }

            var ip = destination[position..];
            if (address.GetIp(ref ip) != SocketError.Success)
                return false;

            position += ip.Length;

            if (address.IsIpv6)
            {
                if (address.ScopeId != 0)
                {
                    if (!TryMoveNext(destination, ref position, '%'))
                        return false;

                    if (!address.ScopeId.TryFormat(destination[position..], out var scopeIdWritten))
                        return false;

                    position += scopeIdWritten;
                }

                if (!TryMoveNext(destination, ref position, ']'))
                    return false;
            }

            if (!TryMoveNext(destination, ref position, ':'))
                return false;

            if (!address.Port.TryFormat(destination[position..], out var portWritten))
                return false;

            position += portWritten;
            charsWritten = position;
            return true;
        }

        /// <summary>
        ///     Appends a single character to the destination span, advancing the position on success.
        /// </summary>
        /// <param name="destination">The span to write to.</param>
        /// <param name="position">The current write position; advanced on success.</param>
        /// <param name="value">The character to append.</param>
        /// <returns>true if the character was appended; otherwise, false.</returns>
        private static bool TryMoveNext(Span<char> destination, ref int position, char value)
        {
            if (position >= destination.Length)
                return false;

            destination[position++] = value;
            return true;
        }
    }
}