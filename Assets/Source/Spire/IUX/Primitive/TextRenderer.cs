using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.IUX
{
    public class TextRenderer : MonoBehaviour
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private WidgetConfig _config;
        private ITweenConfig _tweens;
        private IColorConfig _colors;

        /// <summary>
        /// Source primitive.
        /// </summary>
        private TextPrimitive _textPrimitive;

        /// <summary>
        /// Unity text rendering.
        /// </summary>
        public Text Text;

        /// <summary>
        /// Initialization.
        /// </summary>
        internal void Initialize(
            TextPrimitive textPrimitive,
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IIntentionManager intention,
            IInteractionManager interaction,
            IInteractableManager interactables)
        {
            _textPrimitive = textPrimitive;
            _tweens = tweens;
            _colors = colors;
            _config = config;
        }
    }
}
