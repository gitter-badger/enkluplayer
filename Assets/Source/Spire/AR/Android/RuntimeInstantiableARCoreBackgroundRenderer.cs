using GoogleARCore;

namespace CreateAR.SpirePlayer.AR
{
    public class RuntimeInstantiableARCoreBackgroundRenderer : ARCoreBackgroundRenderer
    {
        private void Awake()
        {
            enabled = false;
        }
    }
}
