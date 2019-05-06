using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Marks a location and configuration for a Kinect.
    /// </summary>
    public class KinectWidget : Widget
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public KinectWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors)
            : base(gameObject, layers, tweens, colors)
        {
            //
        }
    }
}