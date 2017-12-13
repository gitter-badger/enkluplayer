using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class Menu : Widget
    {
        private readonly IPrimitiveFactory _primitives;

        private ElementSchemaProp<string> _label;
        private ElementSchemaProp<int> _fontSize;

        private TextPrimitive _labelPrimitive;

        public Menu(
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IPrimitiveFactory primitives)
            : base(
                  new GameObject("Menu"),
                  config,
                  layers,
                  tweens,
                  colors,
                  messages)
        {
            _primitives = primitives;
        }

        protected override void LoadInternal()
        {
            base.LoadInternal();

            // retrieve properties
            _label = Schema.Get<string>("label");
            _label.OnChanged += Label_OnChanged;
            _fontSize = Schema.Get<int>("fontSize");
            _fontSize.OnChanged += FontSize_OnChanged;

            // create + place label
            _labelPrimitive = _primitives.Text();
            _labelPrimitive.Parent = this;
            _labelPrimitive.Text = _label.Value;
            _labelPrimitive.FontSize = _fontSize.Value;
            _labelPrimitive.Position = new Vec3(-0.15f, 0, 0);

            // retrieve and place buttons
            var buttons = new List<Button>();
            Find("(@type=Button)", buttons);
            
            Log.Info(this, "Placing {0} buttons.", buttons.Count);

            for (int i = 0, len = buttons.Count; i < len; i++)
            {
                
            }
        }

        protected override void UnloadInternal()
        {
            _label.OnChanged -= Label_OnChanged;
            _label = null;

            _fontSize.OnChanged -= FontSize_OnChanged;
            _fontSize = null;

            _labelPrimitive.Destroy();

            base.UnloadInternal();
        }

        private void Label_OnChanged(
            ElementSchemaProp<string> prop,
            string previous,
            string next)
        {
            _labelPrimitive.Text = next;
        }

        private void FontSize_OnChanged(
            ElementSchemaProp<int> prop,
            int previous,
            int next)
        {
            _labelPrimitive.FontSize = next;
        }
    }
}