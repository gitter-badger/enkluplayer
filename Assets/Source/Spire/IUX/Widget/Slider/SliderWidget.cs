using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Basic slider control.
    /// </summary>
    public class SliderWidget : Widget
    {
        /// <summary>
        /// For creating primitives.
        /// </summary>
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<float> _sizeProp;

        /// <summary>
        /// Activator primitive.
        /// </summary>
        private ActivatorPrimitive _activator;

        /// <summary>
        /// Normalized value.
        /// </summary>
        public float Value { get; set; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public SliderWidget(
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

        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            _sizeProp = Schema.Get<float>("size");
            _sizeProp.OnChanged += Size_OnChanged;

            _activator = _primitives.Activator(Schema, this);
            AddChild(_activator);
        }

        protected override void AfterUnloadChildrenInternal()
        {
            base.AfterUnloadChildrenInternal();

            _sizeProp.OnChanged -= Size_OnChanged;
        }

        private void Size_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            
        }
    }
}