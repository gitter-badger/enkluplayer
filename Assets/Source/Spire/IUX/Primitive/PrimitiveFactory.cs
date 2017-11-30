﻿using CreateAR.Commons.Unity.Messaging;

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

        /// <summary>
        /// Basic text rendering primitive.
        /// </summary>
        public ActivatorMonoBehaviour ActivatorMonoBehaviour;

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <returns></returns>
        public TextPrimitive Text()
        {
            return new TextPrimitive(Config, Pools);
        }

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <returns></returns>
        public IActivator Activator()
        {
            var activator = Instantiate(ActivatorMonoBehaviour);
            activator.Initialize(Config, Layers, Tweens, Colors, Messages, Intention, Interactions, Interactables);
            return activator;
        }

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <returns></returns>
        public ReticlePrimitive Reticle()
        {
            return new ReticlePrimitive(Config);
        }
    }
}