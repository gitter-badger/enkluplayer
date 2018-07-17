using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
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
        /// Config.
        /// </summary>
        private readonly WidgetConfig _config;

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
        /// Token for image load.
        /// </summary>
        private IAsyncToken<ManagedTexture> _loadToken;

        /// <summary>
        /// Managed texture we release after we are done.
        /// </summary>
        private ManagedTexture _texture;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _voiceActivatorProp;
        private ElementSchemaProp<string> _labelProp;
        private ElementSchemaProp<float> _labelPaddingProp;
        private ElementSchemaProp<string> _iconProp;
        private ElementSchemaProp<float> _iconScaleProp;
        private ElementSchemaProp<string> _layoutProp;
        private ElementSchemaProp<string> _srcProp;

        /// <summary>
        /// Voice command.
        /// </summary>
        private string _registeredVoiceCommand;

        /// <summary>
        /// Activator.
        /// </summary>
        public ActivatorPrimitive Activator { get; private set; }

        /// <summary>
        /// Text primitive.
        /// </summary>
        public TextPrimitive Text { get; private set; }

        /// <inheritdoc />
        public bool Interactable { get { return Activator.Interactable; } }

        /// <inheritdoc />
        public bool IsHighlighted { get; set; }

        /// <inheritdoc />
        public float Aim { get { return Activator.Aim; } }

        /// <inheritdoc />
        public bool Raycast(Vec3 origin, Vec3 direction)
        {
            return Activator.Raycast(origin, direction);
        }

        /// <inheritdoc />
        public bool Focused
        {
            get { return Activator.Focused; }
            set { Activator.Focused = value; }
        }

        /// <inheritdoc />
        public Vec3 Focus
        {
            get { return GameObject.transform.position.ToVec(); }
        }

        /// <inheritdoc />
        public Vec3 FocusScale
        {
            get { return GameObject.transform.lossyScale.ToVec(); }
        }

        /// <inheritdoc />
        public int HighlightPriority
        {
            get { return Activator.HighlightPriority; }
            set { Activator.HighlightPriority = value; }
        }

        /// <inheritdoc />
        public event Action<IInteractable> OnVisibilityChanged;

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
            IVoiceCommandManager voice,
            IImageLoader loader)
            : base(
                gameObject,
                layers,
                tweens,
                colors)
        {
            _config = config;
            _primitives = primitives;
            _voice = voice;
            _loader = loader;
            _stateRenderer = new ButtonStateRenderer(tweens, colors, this);
        }
        
        /// <inheritdoc cref="Element"/>
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();
            
            // Activator
            {
                Activator = _primitives.Activator(Schema, this);
                AddChild(Activator);
                
                _srcProp = Schema.Get<string>("src");
                _srcProp.OnChanged += Src_OnChanged;

                _iconProp = Schema.Get<string>("icon");
                _iconProp.OnChanged += Icon_OnChanged;

                _iconScaleProp = Schema.Get<float>("icon.scale");
                _iconScaleProp.OnChanged += IconScale_OnChanged;

                UpdateIcon();
                UpdateIconScale();
            }

            // create label
            {
                _labelProp = Schema.Get<string>("label");
                _labelProp.OnChanged += Label_OnChange;
                
                _labelPaddingProp = Schema.Get<float>("label.padding");
                _labelPaddingProp.OnChanged += LabelPadding_OnChanged;

                _layoutProp = Schema.Get<string>("layout");
                _layoutProp.OnChanged += Layout_OnChanged;

                Text = _primitives.Text(Schema);
                Text.Text = _labelProp.Value;
                Text.OnTextRectUpdated += TextPrimitive_OnTextRectUpdated;
                AddChild(Text);

                UpdateLabelLayout();
            }

            // Voice Activator
            {
                _voiceActivatorProp = Schema.Get<string>("voiceActivator");
                _voiceActivatorProp.OnChanged += VoiceActivator_OnChange;

                RegisterVoiceCommand();
            }

            _stateRenderer.Initialize();
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();

            if (null != _loadToken)
            {
                _loadToken.Abort();
            }

            if (null != _texture)
            {
                _texture.Release();
                _texture = null;
            }

            _stateRenderer.Uninitialize();

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

        /// <inheritdoc />
        protected override void OnVisibilityUpdated()
        {
            base.OnVisibilityUpdated();

            if (null != OnVisibilityChanged)
            {
                OnVisibilityChanged(this);
            }
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
            // kill previous managed textures
            if (null != _texture)
            {
                _texture.Release();
                _texture = null;

                Activator.Icon = null;
            }

            // load icon
            Activator.Icon = _config.Icons.Icon(_iconProp.Value);

            // if there is a src, load it
            var src = _srcProp.Value;
            if (!string.IsNullOrEmpty(src))
            {
                // abort previous loads
                if (null != _loadToken)
                {
                    _loadToken.Abort();
                }

                _loadToken = _loader
                    .Load(src)
                    .OnSuccess(texture =>
                    {
                        Log.Info(this, "Successfully loaded image.");

                        _texture = texture;
                        
                        // create sprite
                        Activator.Icon = Sprite.Create(
                            texture.Source,
                            Rect.MinMaxRect(0, 0,
                                texture.Source.width,
                                texture.Source.height),
                            new Vector2(0.5f, 0.5f));
                    })
                    .OnFailure(exception => Log.Warning(
                        this,
                        "Could not load {0} : {1}.",
                        src,
                        exception));
            }
        }

        /// <summary>
        /// Updates the scale of the icon.
        /// </summary>
        private void UpdateIconScale()
        {
            Activator.IconScale = _iconScaleProp.Value;
        }

        /// <summary>
        /// Updates label positioning.
        /// </summary>
        private void UpdateLabelLayout()
        {
            switch (_layoutProp.Value)
            {
                case "vertical":
                {
                    var rect = Text.Rect;

                    Text.Alignment = TextAlignmentType.MidCenter;
                    Text.LocalPosition = new Vector3(
                        0,
                        -rect.size.y - _labelPaddingProp.Value,
                        0);
                    
                    break;
                }

                default:
                {
                    Text.Alignment = TextAlignmentType.MidLeft;
                    Text.LocalPosition = new Vector3(_labelPaddingProp.Value, 0, 0);
                    
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
        /// 
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void IconScale_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateIconScale();
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
            if (Activator.Interactable)
            {
                Activator.Activate();
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
            Text.Text = next;
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

        /// <summary>
        /// Called when the text rect changes.
        /// </summary>
        /// <param name="textPrimitive">The primitive.</param>
        private void TextPrimitive_OnTextRectUpdated(TextPrimitive textPrimitive)
        {
            UpdateLabelLayout();
        }
    }
}