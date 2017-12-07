using CreateAR.Commons.Unity.Messaging;

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
        [Inject] public ITweenConfig Tweens { get; set; }
        [Inject] public IMessageRouter Messages { get; set; }
        [Inject] public IIntentionManager Intention { get; set; }
        [Inject] public IInteractionManager Interactions { get; set; }
        [Inject] public IAssetPoolManager Pools { get; set; }
        [Inject] public WidgetConfig Config { get; set; }
        [Inject] public IInteractableManager Interactables { get; set; }

        /// <inheritdoc cref="IPrimitiveFactory"/>
        public TextPrimitive Text()
        {
            return new TextPrimitive(Config, Pools);
        }

        /// <inheritdoc cref="IPrimitiveFactory"/>
        public ActivatorPrimitive Activator()
        {
            var activator = new ActivatorPrimitive(
                Config,
                Interactables,
                Interactions,
                Intention,
                Messages,
                Layers,
                Tweens,
                Colors);

            Elements.Add(activator);

            return activator;
        }

        /// <inheritdoc cref="IPrimitiveFactory"/>
        public ReticlePrimitive Reticle()
        {
            return new ReticlePrimitive(Config);
        }
    }
}