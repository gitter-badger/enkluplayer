namespace CreateAR.SpirePlayer.AR
{
    /// <summary>
    /// Passthrough implementation of <c>IArService</c> that provides a floor.
    /// </summary>
    public class PassthroughArService : IArService
    {
        /// <inheritdoc cref="IArService"/>
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

        /// <inheritdoc cref="IArService"/>
        public ArServiceConfiguration Config { get; private set; }

        /// <inheritdoc cref="IArService"/>
        public void Setup(ArServiceConfiguration config)
        {
            Config = config;
        }

        /// <inheritdoc cref="IArService"/>
        public void Teardown()
        {
            //
        }
    }
}