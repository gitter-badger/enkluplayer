using System;
using CreateAR.SpirePlayer.AR;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IArService</c> implementation for HoloLens.
    /// </summary>
    public class HoloLensArService : IArService
    {
        /// <inheritdoc />
        public event Action OnTrackingOffline;
        
        /// <inheritdoc />
        public event Action OnTrackingOnline;

        /// <inheritdoc />
        public ArAnchor[] Anchors { get; private set; }

        /// <inheritdoc />
        public ArServiceConfiguration Config { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HoloLensArService()
        {
            Anchors = new[]
            {
                new ArAnchor("floor")
                {
                    Extents = new Vec3(1000, 1000, 1000),
                    Position = new Vec3(0, 0, 0),
                    Rotation = Quat.Euler(0, 0, 0)
                },
            };
        }

        /// <inheritdoc />
        public void Setup(ArServiceConfiguration config)
        {
            
        }

        /// <inheritdoc />
        public void Teardown()
        {
            
        }
    }
}