using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Configuration for rendering mesh capture.
    /// </summary>
    public class MeshCaptureConfig : MonoBehaviour
    {
        [Header("Rendering")]
        [Tooltip("Material to apply to surface geometry.")]
        public Material SurfaceMaterial;

        [Header("Mock Service")]
        [Tooltip("Max milliseconds the mock servive should wait between generating new data.")]
        public int MockGenerateMaxMillis = 750;

        [Tooltip("Min milliseconds the mock servive should wait between generating new data.")]
        public int MockGenerateMinMillis = 250;
    }
}