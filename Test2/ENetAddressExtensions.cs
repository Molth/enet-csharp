using System;
using System.Text;
using enet;

namespace Test2
{
    public static class ENetAddressExtensions
    {
        public static bool TryFormatIpEndPointTo(in this ENetAddress address, StringBuilder builder)
        {
            if (!address.IsIpv4 && !address.IsIpv6)
                return false;

            Span<char> ip = stackalloc char[256];
            address.GetIp(ref ip);

            if (address.IsIpv6)
                builder.Append('[');

            builder.Append(ip);
            if (address.IsIpv6)
            {
                if (address.ScopeId != 0)
                    builder.Append('%').Append(address.ScopeId);

                builder.Append(']');
            }

            builder.Append(':');
            builder.Append(address.Port);

            return true;
        }
    }
}