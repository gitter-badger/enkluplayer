using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Container widget.
    /// </summary>
    public class Container : Widget
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Container(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages)
            : base(gameObject, config, layers, tweens, colors, messages)
        {
            //
        }
    }
}