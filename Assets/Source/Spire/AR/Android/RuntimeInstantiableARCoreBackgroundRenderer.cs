#if !UNITY_WSA && !UNITY_WEBGL

using GoogleARCore;

namespace CreateAR.SpirePlayer.AR
{
    /// <summary>
    /// A helper class that allows us to add GoogleARCore::ARCoreBackgroundRenderer
    /// components at runtime without triggering an unnecessary error due to a field
    /// check in the OnEnable function.
    /// </summary>
    public class RuntimeInstantiableARCoreBackgroundRenderer : ARCoreBackgroundRenderer
    {
        /// <summary>
        /// MonoBehaviour Awake Method. We're disabling the component immediately
        /// to allow the instantiator to assign fields on ARCoreBackgroundRenderer
        /// prior to re-enabling.
        /// </summary>
        private void Awake()
        {
            enabled = false;
        }
    }
}

#endif
