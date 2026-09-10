// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        /// <summary>
        ///     Returns the larger of two unsigned 32-bit values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="y">The second value.</param>
        /// <returns><paramref name="x" /> when it is greater than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
        public static uint ENET_MAX(uint x, uint y) => ((x) > (y) ? (x) : (y));

        /// <summary>
        ///     Returns the smaller of two unsigned 32-bit values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="y">The second value.</param>
        /// <returns><paramref name="x" /> when it is less than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
        public static uint ENET_MIN(uint x, uint y) => ((x) < (y) ? (x) : (y));

        /// <summary>
        ///     Returns the absolute difference between two unsigned 32-bit values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="y">The second value.</param>
        /// <returns>The non-negative difference between <paramref name="x" /> and <paramref name="y" />.</returns>
        public static uint ENET_DIFFERENCE(uint x, uint y) => ((x) < (y) ? (y) - (x) : (x) - (y));
    }
}