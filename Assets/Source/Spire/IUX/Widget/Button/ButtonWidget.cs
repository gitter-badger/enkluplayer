using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Three distinct states of a button.
    /// </summary>
    public enum ButtonState
    {
        Ready,
        Activating,
        Activated
    }

    /// <summary>
    /// Basic widget that combines an activator and a label.
    /// </summary>
    public class ButtonWidget : Widget, IInteractable
    {
        /// <summary>
        /// For primitives.
        /// </summary>
        private readonly IPrimitiveFactory _primitives;
        
        /// <summary>
        /// Recognized voice commands.
        /// </summary>
        private readonly IVoiceCommandManager _voice;

        /// <summary>
        /// Loads images.
        /// </summary>
        private readonly IImageLoader _loader;

        /// <summary>
        /// Controls rendering changes based on button state.
        /// </summary>
        private readonly ButtonStateRenderer _stateRenderer;

        /// <summary>
        /// Texture for src.
        /// </summary>
        private Texture2D _srcTexture;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _voiceActivatorProp;
        private ElementSchemaProp<string> _labelProp;
        private ElementSchemaProp<float> _labelPaddingProp;
        private ElementSchemaProp<string> _iconProp;
        private ElementSchemaProp<string> _layoutProp;
        private ElementSchemaProp<string> _srcProp;

        /// <summary>
        /// Text primitive.
        /// </summary>
        private TextPrimitive _text;

        /// <summary>
        /// Activator!
        /// </summary>
        private ActivatorPrimitive _activator;

        /// <summary>
        /// Voice command.
        /// </summary>
        private string _registeredVoiceCommand;

        /// <summary>
        /// Activator.
        /// </summary>
        public ActivatorPrimitive Activator
        {
            get { return _activator; }
        }

        /// <summary>
        /// Text primitive.
        /// </summary>
        public TextPrimitive Text
        {
            get { return _text; }
        }
        
        /// <inheritdoc cref="IInteractable"/>
        public bool Interactable { get { return _activator.Interactable; } }

        /// <inheritdoc cref="IInteractable"/>
        public float Aim { get { return _activator.Aim; } }

        /// <inheritdoc cref="IInteractable"/>
        public bool Raycast(Vec3 origin, Vec3 direction)
        {
            return _activator.Raycast(origin, direction);
        }

        /// <inheritdoc cref="IInteractable"/>
        public bool Focused
        {
            get { return _activator.Focused; }
            set { _activator.Focused = value; }
        }

        /// <inheritdoc cref="IInteractable"/>
        public int HighlightPriority
        {
            get { return _activator.HighlightPriority; }
            set { _activator.HighlightPriority = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ButtonWidget(
            GameObject gameObject,
            WidgetConfig config,
            IPrimitiveFactory primitives,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IVoiceCommandManager voice,
            IImageLoader loader)
            : base(
                gameObject,
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _primitives = primitives;
            _voice = voice;
            _loader = loader;
            _stateRenderer = new ButtonStateRenderer(tweens, colors, config, this);
        }
        
        /// <inheritdoc cref="Element"/>
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            // Activator
            {
                _activator = _primitives.Activator(Schema, this);
                AddChild(_activator);

                _srcProp = Schema.Get<string>("src");
                _srcProp.OnChanged += Src_OnChanged;

                _iconProp = Schema.Get<string>("icon");
                _iconProp.OnChanged += Icon_OnChanged;

                UpdateIcon();
            }

            // create label
            {
                _labelProp = Schema.Get<string>("label");
                _labelProp.OnChanged += Label_OnChange;
                
                _labelPaddingProp = Schema.Get<float>("label.padding");
                _labelPaddingProp.OnChanged += LabelPadding_OnChanged;

                _layoutProp = Schema.Get<string>("layout");
                _layoutProp.OnChanged += Layout_OnChanged;

                _text = _primitives.Text(Schema);
                _text.Text = _labelProp.Value;
                AddChild(_text);

                UpdateLabelLayout();
            }

            // Voice Activator
            {
                _voiceActivatorProp = Schema.Get<string>("voiceActivator");
                _voiceActivatorProp.OnChanged += VoiceActivator_OnChange;
                
                RegisterVoiceCommand();
            }
        }

        /// <inheritdoc cref="Element"/>
        protected override void AfterUnloadChildrenInternal()
        {
            base.AfterUnloadChildrenInternal();
            
            // activator
            {
                _srcProp.OnChanged -= Src_OnChanged;
                _iconProp.OnChanged -= Icon_OnChanged;
            }

            // cleanup label
            {
                _labelProp.OnChanged -= Label_OnChange;
                _labelPaddingProp.OnChanged -= LabelPadding_OnChanged;
            }

            // cleanup voice
            {
                UnregisterVoiceCommand();
                _voiceActivatorProp.OnChanged -= VoiceActivator_OnChange;
            }
        }

        /// <inheritdoc />
        protected override void LateUpdateInternal()
        {
            base.LateUpdateInternal();

            _stateRenderer.Update(Time.smoothDeltaTime);
        }

        /// <summary>
        /// Registers voice command.
        /// </summary>
        private void RegisterVoiceCommand()
        {
            var voiceActivator = _voiceActivatorProp.Value;
            if (!string.IsNullOrEmpty(voiceActivator))
            {
                _registeredVoiceCommand = _labelProp.Value;
                _voice.Register(_registeredVoiceCommand, Voice_OnRecognized);
            }
        }
        
        /// <summary>
        /// Unregisters voice command.
        /// </summary>
        private void UnregisterVoiceCommand()
        {
            if (!string.IsNullOrEmpty(_registeredVoiceCommand))
            {
                _voice.Unregister(_registeredVoiceCommand);
                _registeredVoiceCommand = string.Empty;
            }
        }

        /// <summary>
        /// Updates the icon.
        /// </summary>
        private void UpdateIcon()
        {
            _activator.Icon = Config.Icons.Icon(_iconProp.Value);

            // if there is a src, load it
            var src = _srcProp.Value;
            if (!string.IsNullOrEmpty(src))
            {
                if (null == _srcTexture)
                {
                    _srcTexture = new Texture2D(2, 2);
                }

                _loader
                    .Load(
                        "{assetsUrl}" + src,
                        _srcTexture)
                    .OnSuccess(_ =>
                    {
                        Log.Info(this, "Successfully loaded image.");

                        _activator.Icon = Sprite.Create(
                            _srcTexture,
                            Rect.MinMaxRect(0, 0, _srcTexture.width, _srcTexture.height),
                            new Vector2(0.5f, 0.5f));
                    })
                    .OnFailure(exception => Log.Error(
                        this,
                        "Could not load {0} : {1}.",
                        src,
                        exception));
            }
        }

        /// <summary>
        /// Updates label positioning.
        /// </summary>
        private void UpdateLabelLayout()
        {
            var rect = _text.Rect;

            switch (_layoutProp.Value)
            {
                case "vertical":
                {
                    _text.Alignment = TextAlignmentType.MidCenter;
                    _text.LocalPosition = new Vector3(
                        0,
                        -rect.size.y - _labelPaddingProp.Value,
                        0);
                    
                    break;
                }

                default:
                {
                    _text.LocalPosition = new Vector3(
                        -rect.min.x + _labelPaddingProp.Value,
                        -(rect.max.y - rect.min.y) / 2,
                        0f);
                    break;
                }
            }
        }

        /// <summary>
        /// Called when the icon has changed.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Icon_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateIcon();
        }

        /// <summary>
        /// Called when the src has changed.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Src_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateIcon();
        }

        /// <summary>
        /// Invoked when a recognizable word or phrase is spoken.
        /// </summary>
        /// <param name="keyword">The keywords spoken.</param>
        private void Voice_OnRecognized(string keyword)
        {
            if (_activator.Interactable)
            {
                _activator.Activate();
            }
        }
        
        /// <summary>
        /// Called when the voice activator prop has changed.
        /// </summary>
        /// <param name="prop">Prop!</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void VoiceActivator_OnChange(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UnregisterVoiceCommand();
            RegisterVoiceCommand();
        }

        /// <summary>
        /// Called when label has been updated.
        /// </summary>
        /// <param name="prop">Label prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Label_OnChange(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            _text.Text = next;
        }
        
        /// <summary>
        /// Called when the label padding has changed.
        /// </summary>
        /// <param name="prop">Property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void LabelPadding_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateLabelLayout();
        }

        /// <summary>
        /// Called when the layout has changed.
        /// </summary>
        /// <param name="prop">Property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Layout_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateLabelLayout();
        }
    }
}