using CreateAR.SpirePlayer.AR;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IArService</c> implementation for HoloLens.
    /// </summary>
    public class HoloLensArService : IArService
    {
        /// <inheritdoc cref="IArService"/>
        public ArAnchor[] Anchors { get; private set; }

        /// <inheritdoc cref="IArService"/>
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

        /// <inheritdoc cref="IArService"/>
        public void Setup(ArServiceConfiguration config)
        {
            
        }

        /// <inheritdoc cref="IArService"/>
        public void Teardown()
        {
            
        }
    }
}