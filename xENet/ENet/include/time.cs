// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        /// <summary>
        ///     Half the range of the 32-bit millisecond clock, used to detect wraparound between two time values.
        /// </summary>
        public const uint ENET_TIME_OVERFLOW = 86400000;

        /// <summary>
        ///     Determines whether time <paramref name="a" /> is strictly less than time <paramref name="b" />,
        ///     accounting for 32-bit clock wraparound.
        /// </summary>
        /// <param name="a">The earlier candidate time.</param>
        /// <param name="b">The later candidate time.</param>
        /// <returns>
        ///     <see langword="true" /> when <paramref name="a" /> precedes <paramref name="b" />; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public static bool ENET_TIME_LESS(uint a, uint b) => ((a) - (b) >= ENET_TIME_OVERFLOW);

        /// <summary>
        ///     Determines whether time <paramref name="a" /> is strictly greater than time <paramref name="b" />,
        ///     accounting for 32-bit clock wraparound.
        /// </summary>
        /// <param name="a">The later candidate time.</param>
        /// <param name="b">The earlier candidate time.</param>
        /// <returns>
        ///     <see langword="true" /> when <paramref name="a" /> follows <paramref name="b" />; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public static bool ENET_TIME_GREATER(uint a, uint b) => ((b) - (a) >= ENET_TIME_OVERFLOW);

        /// <summary>
        ///     Determines whether time <paramref name="a" /> is less than or equal to time <paramref name="b" />,
        ///     accounting for 32-bit clock wraparound.
        /// </summary>
        /// <param name="a">The earlier or equal candidate time.</param>
        /// <param name="b">The later or equal candidate time.</param>
        /// <returns>
        ///     <see langword="true" /> when <paramref name="a" /> does not follow <paramref name="b" />; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public static bool ENET_TIME_LESS_EQUAL(uint a, uint b) => (!ENET_TIME_GREATER(a, b));

        /// <summary>
        ///     Determines whether time <paramref name="a" /> is greater than or equal to time <paramref name="b" />,
        ///     accounting for 32-bit clock wraparound.
        /// </summary>
        /// <param name="a">The later or equal candidate time.</param>
        /// <param name="b">The earlier or equal candidate time.</param>
        /// <returns>
        ///     <see langword="true" /> when <paramref name="a" /> does not precede <paramref name="b" />; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public static bool ENET_TIME_GREATER_EQUAL(uint a, uint b) => (!ENET_TIME_LESS(a, b));

        /// <summary>
        ///     Computes the unsigned difference between two times, returning the shortest distance around the clock.
        /// </summary>
        /// <param name="a">The first time.</param>
        /// <param name="b">The second time.</param>
        /// <returns>The absolute difference between <paramref name="a" /> and <paramref name="b" /> in milliseconds.</returns>
        public static uint ENET_TIME_DIFFERENCE(uint a, uint b) => ((a) - (b) >= ENET_TIME_OVERFLOW ? (b) - (a) : (a) - (b));
    }
}