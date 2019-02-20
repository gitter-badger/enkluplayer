using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// A simple widget that toggles on and off.
    /// </summary>
    public class ToggleWidget : ButtonWidget
    {
        /// <summary>
        /// Internal value.
        /// </summary>
        private ElementSchemaProp<bool> _valueProp;

        /// <summary>
        /// Returns true iff toggle is set to true.
        /// </summary>
        public bool Value
        {
            get { return _valueProp.Value; }
            set { _valueProp.Value = value; }
        }

        /// <summary>
        /// Called when the value has changed.
        /// </summary>
        public event Action<ToggleWidget> OnValueChanged;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ToggleWidget(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IPrimitiveFactory primitives,
            IVoiceCommandManager voice,
            IImageLoader imageLoader)
            : base(
                gameObject,
                config,
                primitives,
                layers,
                tweens,
                colors,
                voice,
                imageLoader)
        {
            //
        }

        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            // override icon
            Schema.Set("icon", "");

            base.LoadInternalBeforeChildren();
        }

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();
            
            _valueProp = Schema.GetOwn("value", false);
            _valueProp.OnChanged += Value_OnChanged;

            UpdateIcon();

            Activator.OnActivated += Activator_OnActivated;
        }

        /// <inheritdoc />
        protected override void UnloadInternalBeforeChildren()
        {
            base.UnloadInternalBeforeChildren();

            Activator.OnActivated -= Activator_OnActivated;
        }
        
        /// <summary>
        /// Updates icon based on value.
        /// </summary>
        private void UpdateIcon()
        {
            Schema.Set("icon", _valueProp.Value ? "toggled" : "");
        }

        /// <summary>
        /// Called when the activator activates.
        /// </summary>
        /// <param name="activatorPrimitive">Primitive.</param>
        private void Activator_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            _valueProp.Value = !_valueProp.Value;
        }

        /// <summary>
        /// Called when value has changed.
        /// </summary>
        /// <param name="prop">Prop.</param>
        /// <param name="prev">Previous prop.</param>
        /// <param name="next">Next prop.</param>
        private void Value_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            UpdateIcon();

            if (null != OnValueChanged)
            {
                OnValueChanged(this);
            }
        }
    }
}