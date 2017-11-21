using CreateAR.SpirePlayer.Assets;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Indicator for a piece of content being loaded.
    /// </summary>
    public class LoadProgressBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Container to manipulate scale.
        /// </summary>
        public Transform Container;

        /// <summary>
        /// To spin!
        /// </summary>
        public Transform Spinner;

        /// <summary>
        /// Speed at which to spin.
        /// </summary>
        public float MinSpeed = 100f;
        public float MaxSpeed = 1000f;

        /// <summary>
        /// World space bounds of object we're loading.
        /// </summary>
        public Bounds Bounds = new Bounds(
            Vector3.zero,
            Vector3.one);

        /// <summary>
        /// Load progress object.
        /// </summary>
        public LoadProgress Progress;
        
        /// <summary>
        /// Called every frame.
        /// </summary>
        private void Update()
        {
            // place + scale
            transform.position = Bounds.center;
            transform.localScale = Bounds.size;

            var scale = Mathf.Min(
                1,
                Bounds.size.x,
                Bounds.size.y,
                Bounds.size.z);
            Container.localScale = new Vector3(
                scale / Bounds.size.x,
                scale / Bounds.size.y,
                scale / Bounds.size.z);

            var value = Mathf.Lerp(
                MinSpeed,
                MaxSpeed,
                null == Progress ? 0f : Progress.Value) * Time.deltaTime;

            Spinner.Rotate(
                Spinner.worldToLocalMatrix.MultiplyVector(Vector3.up),
                value);
        }
    }
}