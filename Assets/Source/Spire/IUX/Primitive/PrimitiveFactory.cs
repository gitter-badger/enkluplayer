using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Contains prefabs for UI rendering primitives used in all widgets.
    /// </summary>
    public class PrimitiveFactory : InjectableMonoBehaviour, IPrimitiveFactory
    {
        /// <summary>
        /// Dependencies
        /// </summary>
        [Inject] public IElementManager Elements { get; set; }
        [Inject] public ILayerManager Layers { get; set; }
        [Inject] public IColorConfig Colors { get; set; }
        [Inject] public IWidgetConfig Config { get; set; }
        [Inject] public ITweenConfig Tweens { get; set; }
        [Inject] public IMessageRouter Messages { get; set; }
        [Inject] public IIntentionManager Intention { get; set; }
        [Inject] public IInteractionManager Interactions { get; set; }

        /// <summary>
        /// Basic text rendering primitive.
        /// </summary>
        public TextMonoBehaviour TextMonoBehaviour;
        public ActivatorMonoBehaviour ActivatorMonoBehaviour;
        public ReticleMonoBehaviour ReticleMonoBehaviour;

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <returns></returns>
        public IText Text()
        {
            return Initialize<IText>(Instantiate(TextMonoBehaviour));
        }

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <returns></returns>
        public IActivator Activator()
        {
            var activator = Instantiate(ActivatorMonoBehaviour);
            activator.Initialize(Config, Layers, Tweens, Colors, Messages, Intention, Interactions);
            return activator;
        }

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <returns></returns>
        public IReticle Reticle()
        {
            return Initialize<IReticle>(Instantiate(ReticleMonoBehaviour));
        }

        /// <summary>
        /// Initializes the new monobehaviour.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="widgetMonoBehaviour"></param>
        /// <returns></returns>
        private T Initialize<T>(WidgetMonoBehaviour widgetMonoBehaviour) where T : class
        {
            widgetMonoBehaviour
                .Initialize(
                    Config, 
                    Layers, 
                    Tweens, 
                    Colors, 
                    Messages);

            return widgetMonoBehaviour as T;
        }
    }
}
