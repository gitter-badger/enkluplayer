using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class Float : Widget
    {
        /// <summary>
        /// Primitives!
        /// </summary>
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// Text rendering primitive.
        /// </summary>
        private FloatPrimitive _primitive;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="layers"></param>
        /// <param name="tweens"></param>
        /// <param name="colors"></param>
        /// <param name="messages"></param>
        /// <param name="primitives"></param>
        public Float(
            WidgetConfig config, 
            ILayerManager layers, 
            ITweenConfig tweens, 
            IColorConfig colors, 
            IMessageRouter messages,
            IPrimitiveFactory primitives) 
            : base(
                  new GameObject("Float"), 
                  config, 
                  layers, 
                  tweens, 
                  colors, 
                  messages)
        {
            _primitives = primitives;
        }

        /// <inheritdoc cref="Element"/>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _primitive = _primitives.Float(Schema);
            _primitive.Parent = this;
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            
        }
    }
}
