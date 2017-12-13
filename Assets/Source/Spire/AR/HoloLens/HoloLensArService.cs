using CreateAR.SpirePlayer.AR;

namespace CreateAR.SpirePlayer
{
    public class HoloLensArService : IArService
    {
        public ArAnchor[] Anchors { get; private set; }

        public ArServiceConfiguration Config { get; private set; }

        public void Setup(ArServiceConfiguration config)
        {
            
        }

        public void Teardown()
        {
            
        }
    }
}
