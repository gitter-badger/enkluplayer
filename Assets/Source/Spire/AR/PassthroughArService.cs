using System;

namespace CreateAR.SpirePlayer.AR
{
    /// <summary>
    /// Passthrough implementation of <c>IArService</c> that provides a floor.
    /// </summary>
    public class PassthroughArService : IArService
    {
        /// <inheritdoc />
        public event Action OnTrackingOffline;
        
        /// <inheritdoc />
        public event Action OnTrackingOnline;
        
        /// <inheritdoc />
        public ArAnchor[] Anchors
        {
            get
            {
                return new []
                {
                    new ArAnchor("floor")
                    {
                        Extents = new Vec3(1, 0, 1),
                        Position = new Vec3(0f, 0f, 0f),
                        Rotation = Quat.Euler(0, 0, 0)
                    },
                };
            }
        }

        /// <inheritdoc />
        public ArServiceConfiguration Config { get; private set; }

        /// <inheritdoc />
        public void Setup(ArServiceConfiguration config)
        {
            Config = config;
        }

        /// <inheritdoc />
        public void Teardown()
        {
            //
        }
    }
}