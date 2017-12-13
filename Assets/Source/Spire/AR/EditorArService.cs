namespace CreateAR.SpirePlayer.AR
{
    /// <summary>
    /// Editor implementation of <c>IArService</c> that provides a floor.
    /// </summary>
    public class EditorArService : IArService
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
                        Position = new Vec3(3, -2, 6.4f),
                        Rotation = Quat.Euler(0, 45, 0)
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