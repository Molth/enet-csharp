using Enet;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     Configuration options used to create and run a threaded ENet host.
    /// </summary>
    public struct ThreadedEnetHostConfig
    {
        /// <summary>
        ///     The base configuration used to create the underlying <see cref="EnetHost" />.
        /// </summary>
        public EnetHostConfig InnerConfig;

        /// <summary>
        ///     The additional configuration applied to the underlying host after it is created.
        /// </summary>
        public EnetHostAdditionalConfig AdditionalConfig;

        /// <summary>
        ///     The maximum time in milliseconds the background thread waits for network events during each service pass.
        /// </summary>
        public uint ServiceTimeout;
    }
}