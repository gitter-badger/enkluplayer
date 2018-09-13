using System.Linq;
using CreateAR.EnkluPlayer.IUX.Dynamics;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Connects two points in space dynamically.
    /// </summary>
    public class LinkRenderer : MonoBehaviour
    {
        /// <summary>
        /// Time alive
        /// </summary>
        private float _elapsed;

        /// <summary>
        /// Elapsed when fade out was started
        /// </summary>
        private float _fadeOutStartElapsed;

        /// <summary>
        /// Defines the motion of the second point moving towards the first point
        /// </summary>
        public AnimationCurve FadeInCurve = AnimationCurve.EaseInOut(0, 0, 0.4f, 1);

        /// <summary>
        /// Defines the motion of the first point moving towards the second point
        /// </summary>
        public AnimationCurve FadeOutCurve = AnimationCurve.EaseInOut(0, 1, 0.8f, 0);

        /// <summary>
        /// For first endpoint
        /// </summary>
        public Target EndPoint0;

        /// <summary>
        /// For second endpoint
        /// </summary>
        public Target EndPoint1;

        /// <summary>
        /// Renders a line
        /// </summary>
        public LineRenderer LineRenderer;

        /// <summary>
        /// Returns true if has already begun fading out
        /// </summary>
        public bool IsFadingOut
        {
            get
            {
                return _fadeOutStartElapsed > Mathf.Epsilon;
            }
        }

        /// <summary>
        /// Begins fade out procedure
        /// </summary>
        public void FadeOut()
        {
            _fadeOutStartElapsed = _elapsed;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            _elapsed += Time.smoothDeltaTime;

            // calculate elapsed per point
            var fadeInElapsed = _elapsed;
            var fadeOutElapsed = 0.0f;

            if (IsFadingOut)
            {
                fadeOutElapsed = _elapsed - _fadeOutStartElapsed;
                var duration = FadeOutCurve.keys.Last().time;
                if (fadeOutElapsed > duration)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            // calculate the lerp for each link
            var p1Lerp = FadeInCurve.Evaluate(fadeInElapsed);
            var p0Lerp = FadeOutCurve.Evaluate(fadeOutElapsed);
            p0Lerp = Mathf.Min(p0Lerp, p1Lerp);
            p1Lerp = Mathf.Max(p1Lerp, p0Lerp);

            // calculate endpoint positions
            var e0 = EndPoint0.DynamicPosition;
            var e1 = EndPoint1.DynamicPosition;
            var e1toe0 = e0 - e1;
            var p0 = e1 + e1toe0 * p0Lerp;
            var p1 = e1 + e1toe0 * p1Lerp;

            // update line renderer
            LineRenderer.positionCount = 2;
            LineRenderer.SetPosition(0, p0);
            LineRenderer.SetPosition(1, p1);
        }
    }
}
