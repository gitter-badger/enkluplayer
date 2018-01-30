using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Displays options in a grid format.
    /// </summary>
    public class Grid : Widget
    {
        /// <summary>
        /// For creating primitives.
        /// </summary>
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Grid(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IPrimitiveFactory primitives)
            : base(
                gameObject,
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _primitives = primitives;
        }
    }
}