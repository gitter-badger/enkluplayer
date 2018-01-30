using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Almost 1:1 with HTML Select tag. Child Option components will be
    /// displayed.
    /// </summary>
    public class Select : Widget
    {
        /// <summary>
        /// Primitives.
        /// </summary>
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// List of potential options.
        /// </summary>
        private readonly List<string> _options = new List<string>();

        /// <summary>
        /// Left activator.
        /// </summary>
        private ActivatorPrimitive _leftActivator;

        /// <summary>
        /// Right activator.
        /// </summary>
        private ActivatorPrimitive _rightActivator;

        /// <summary>
        /// Manages font size.
        /// </summary>
        private ElementSchemaProp<int> _fontSizeProp;

        /// <summary>
        /// Text primitive.
        /// </summary>
        private TextPrimitive _text;

        /// <summary>
        /// Index into options list.
        /// </summary>
        private int _selection = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Select(
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors, 
            IMessageRouter messages,
            IPrimitiveFactory primitives)
            : base(
                new GameObject("Select"),
                config, layers, tweens, colors, messages)
        {
            _primitives = primitives;
        }

        /// <inheritdoc />
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            // Left Activator
            {
                _leftActivator = _primitives.Activator(Schema, this);
                _leftActivator.Icon = Config.Icons.Icon("arrow-left");
                _leftActivator.OnActivated += Left_OnActivated;
                AddChild(_leftActivator);
            }

            // Right Activator
            {
                _rightActivator = _primitives.Activator(Schema, this);
                _rightActivator.Icon = Config.Icons.Icon("arrow-right");
                _rightActivator.OnActivated += Right_OnActivated;
                // TODO: Don't hack positions
                _rightActivator.GameObject.transform.localPosition = new Vector3(0.48f, 0, 0);
                AddChild(_rightActivator);
            }

            // Text
            {
                _fontSizeProp = Schema.Get<int>("fontSize");
                _fontSizeProp.OnChanged += FontSize_OnChanged;

                _text = _primitives.Text(Schema);
                _text.FontSize = _fontSizeProp.Value;
                // TODO: Don't hack positions
                _text.GameObject.transform.localPosition = new Vector3(0.09f, -0.03f, 0);
                AddChild(_text);
            }

            // Options
            {
                GatherOptions();
                UpdateLabel();
            }
        }

        /// <inheritdoc />
        protected override void BeforeUnloadChildrenInternal()
        {
            _leftActivator.OnActivated -= Left_OnActivated;
            _rightActivator.OnActivated -= Right_OnActivated;

            base.BeforeUnloadChildrenInternal();
        }
        
        /// <summary>
        /// Updates the label in the middle.
        /// </summary>
        private void UpdateLabel()
        {
            if (_selection < 0 || _selection >= _options.Count)
            {
                return;
            }

            _text.Text = _options[_selection];
        }

        /// <summary>
        /// Gathers all the potential options.
        /// </summary>
        private void GatherOptions()
        {
            _options.Clear();
            var children = Children;
            for (int i = 0, len = children.Length; i < len; i++)
            {
                var child = children[i];
                var option = child as SelectOption;
                if (null != option)
                {
                    _options.Add(option.Schema.Get<string>("label").Value);
                }
            }
        }

        private void Right_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            _selection = (_selection + 1) % _options.Count;

            UpdateLabel();
        }

        /// <summary>
        /// Called when the left button is activated.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Left_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (_options.Count == 1)
            {
                return;
            }

            if (0 == _selection)
            {
                _selection = _options.Count - 1;
            }
            else
            {
                _selection -= 1;
            }

            UpdateLabel();
        }

        /// <summary>
        /// Called when the font size prop is changed.
        /// </summary>
        /// <param name="prop">The prop that has changed.</param>
        /// <param name="prev">The previous value of the prop.</param>
        /// <param name="next">The next value of the prop.</param>
        private void FontSize_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            _text.FontSize = next;
        }
    }
}