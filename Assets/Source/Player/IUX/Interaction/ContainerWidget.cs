using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Container widget.
    /// </summary>
    public class ContainerWidget : Widget
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContainerWidget(
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