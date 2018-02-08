using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
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
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages)
            : base(gameObject, config, layers, tweens, colors, messages)
        {
            //
        }
    }
}