using CreateAR.SpirePlayer.AR;

namespace CreateAR.SpirePlayer
{
    public class HoloLensArService : IArService
    {
        public ArAnchor[] Anchors { get; private set; }

        public ArServiceConfiguration Config { get; private set; }

        public HoloLensArService()
        {
            Anchors = new[]
            {
                new ArAnchor("floor")
                {
                    Extents = new Vec3(1, 0, 1),
                    Position = new Vec3(3, -2, 6.4f),
                    Rotation = Quat.Euler(0, 45, 0)
                },
            };
        }

        public void Setup(ArServiceConfiguration config)
        {
            
        }

        public void Teardown()
        {
            
        }
    }
}
