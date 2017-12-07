using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Renders a reticle.
    /// </summary>
    public class ReticleRenderer : MonoBehaviour
    {
        /// <summary>
        /// Widgets
        /// </summary>
        public List<Transform> Spokes = new List<Transform>();

        /// <summary>
        /// Center point
        /// </summary>
        public Transform Center;

        /// <summary>
        /// Updates the reticle.
        /// </summary>
        /// <param name="rotation">Rotation of the reticle.</param>
        /// <param name="spread">Spread of the reticle.</param>
        /// <param name="scale">Reticle scale.</param>
        /// <param name="centerAlpha">Alpha of center point.</param>
        public void Refresh(
            float rotation,
            float spread,
            float scale,
            float centerAlpha)
        {
            for (int i = 0, count = Spokes.Count; i < count; ++i)
            {
                var spoke = Spokes[i];
                if (spoke != null)
                {
                    var theta = Mathf.PI * 2.0f * i / count + rotation;
                    var pointLocalPosition = new Vector3(
                        Mathf.Sin(theta) * spread,
                        Mathf.Cos(theta) * spread,
                        0.0f);

                    spoke.transform.localPosition = pointLocalPosition;
                    spoke.transform.localScale = Vector3.one * scale;
                    spoke.transform.localRotation = Quaternion.AngleAxis(
                        Mathf.Rad2Deg * -theta,
                        Vector3.forward);
                }
            }

            if (Center != null)
            {
                //Center.LocalColor = new Col4(1, 1, 1, centerAlpha);
                Center.transform.localScale = Vector3.one * scale;
            }
        }
    }
}