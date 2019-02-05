using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Almost 1:1 with HTML Select tag. Child Option components will be
    /// displayed.
    /// </summary>
    public class SelectWidget : Widget
    {
        /// <summary>
        /// Config.
        /// </summary>
        private readonly WidgetConfig _config;

        /// <summary>
        /// Primitives.
        /// </summary>
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// List of potential options.
        /// </summary>
        private readonly List<Option> _options = new List<Option>();

        /// <summary>
        /// Public collection.
        /// </summary>
        private readonly ReadOnlyCollection<Option> _optionsWrapper;

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
        private int _selection = -1;

        /// <summary>
        /// Retrieves all options.
        /// </summary>
        public ReadOnlyCollection<Option> Options
        {
            get { return _optionsWrapper; }
        }

        /// <summary>
        /// Currently selected option.
        /// </summary>
        public Option Selection
        {
            get
            {
                return _selection >= 0 && _selection < _options.Count
                    ? _options[_selection]
                    : null;
            }
            set
            {
                _selection = _options.IndexOf(value);

                UpdateLabel(_selection);
            }
        }

        /// <summary>
        /// Called when the selection has changed.
        /// </summary>
        public event Action<SelectWidget> OnValueChanged;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectWidget(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors, 
            IPrimitiveFactory primitives)
            : base(
                gameObject,
                layers,
                tweens,
                colors)
        {
            _config = config;
            _primitives = primitives;

            _optionsWrapper = new ReadOnlyCollection<Option>(_options);
        }

        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();

            // Left Activator
            {
                _leftActivator = _primitives.Activator(Schema, this);
                _leftActivator.Icon = _config.Icons.Icon("arrow-left");
                _leftActivator.OnActivated += Left_OnActivated;
                AddChild(_leftActivator);
            }

            // Right Activator
            {
                _rightActivator = _primitives.Activator(Schema, this);
                _rightActivator.Icon = _config.Icons.Icon("arrow-right");
                _rightActivator.OnActivated += Right_OnActivated;
                // TODO: Don't hack positions
                _rightActivator.Schema.Set("position", new Vec3(0.48f, 0, 0));
                AddChild(_rightActivator);
            }

            // Text
            {
                _fontSizeProp = Schema.Get<int>("fontSize");
                _fontSizeProp.OnChanged += FontSize_OnChanged;

                _text = _primitives.Text(Schema);
                _text.FontSize = _fontSizeProp.Value;
                _text.Alignment = TextAlignmentType.MidCenter;

                // center
                var left = _leftActivator.GameObject.transform.localPosition;
                var right = _rightActivator.GameObject.transform.localPosition;
                var pos = (left + right) / 2f;
                
                _text.Schema.Set(
                    "position",
                    pos.ToVec());
                AddChild(_text);
            }
        }
        
        /// <inheritdoc />
        protected override void UnloadInternalBeforeChildren()
        {
            _leftActivator.OnActivated -= Left_OnActivated;
            _rightActivator.OnActivated -= Right_OnActivated;

            base.UnloadInternalBeforeChildren();
        }

        /// <inheritdoc />
        protected override void AddChildInternal(Element element)
        {
            base.AddChildInternal(element);

            var option = element as Option;
            if (null != option)
            {
                _options.Add(option);

                Verbose("Added {0}. {1} total options.",
                    option,
                    _options.Count);

                UpdateLabel(_selection);
            }
        }

        /// <inheritdoc />
        protected override void RemoveChildInternal(Element element)
        {
            var option = element as Option;
            if (null != option)
            {
                // remove from options
                if (!_options.Remove(option))
                {
                    Log.Warning(this, "Untracked Option removed from Select element!");
                    return;
                }

                Verbose("Removed {0}. {1} total options.",
                    option,
                    _options.Count);
                
                UpdateLabel(_selection);
            }

            base.RemoveChildInternal(element);
        }

        /// <summary>
        /// Updates the label in the middle.
        /// </summary>
        private void UpdateLabel(int target)
        {
            var current = _selection;

            if (-1 == _selection)
            {
                if (_options.Count > 0)
                {
                    target = 0;
                }
            }

            if (_selection > _options.Count)
            {
                if (0 == _options.Count)
                {
                    target = -1;
                }
                else
                {
                    target = _options.Count - 1;
                }
            }

            if (target < 0 || target >= _options.Count)
            {
                _selection = target;
                _text.Text = "";
            }
            else
            {
                _selection = target;
                _text.Text = _options[_selection].Schema.Get<string>("label").Value;
            }

            if (current != _selection && null != OnValueChanged)
            {
                OnValueChanged(this);
            }
        }

        /// <summary>
        /// Called when the right button is activated.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Right_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (_options.Count < 2)
            {
                return;
            }

            UpdateLabel((_selection + 1) % _options.Count);
        }

        /// <summary>
        /// Called when the left button is activated.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Left_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (_options.Count < 2)
            {
                return;
            }

            var target = _selection;
            if (0 == _selection)
            {
                target = _options.Count - 1;
            }
            else
            {
                target -= 1;
            }

            UpdateLabel(target);
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

        /// <summary>
        /// Verbose logging.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string message, params object[] parameters)
        {
            Log.Info(this, message, parameters);
        }
    }
}