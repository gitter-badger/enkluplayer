﻿using System;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// A simple widget that toggles on and off.
    /// </summary>
    public class Toggle : Button
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
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Toggle(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IPrimitiveFactory primitives,
            IVoiceCommandManager voice)
            : base(
                gameObject,
                config,
                primitives,
                layers,
                tweens,
                colors,
                messages,
                voice)
        {
            //
        }

        /// <inheritdoc />
        protected override void BeforeLoadChildrenInternal()
        {
            // override icon
            Schema.Set("icon", "");

            base.BeforeLoadChildrenInternal();
        }

        /// <inheritdoc />
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();
            
            _valueProp = Schema.GetOwn("value", false);
            _valueProp.OnChanged += Value_OnChanged;

            Activator.OnActivated += Activator_OnActivated;
        }

        /// <inheritdoc />
        protected override void BeforeUnloadChildrenInternal()
        {
            base.BeforeUnloadChildrenInternal();

            Activator.OnActivated -= Activator_OnActivated;
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
            Schema.Set("icon", next ? "toggled" : "");
        }
    }
}