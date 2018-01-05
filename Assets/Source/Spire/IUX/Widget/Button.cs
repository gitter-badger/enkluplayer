using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Button
    /// </summary>
    public class Button : Widget, IInteractable
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
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _propVoiceActivator;
        private ElementSchemaProp<string> _labelProp;
        private ElementSchemaProp<int> _fontSizeProp;
        private ElementSchemaProp<float> _labelPaddingProp;

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
        public Button(
            WidgetConfig config,
            IPrimitiveFactory primitives,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IVoiceCommandManager voice)
            : base(
                new GameObject("Button"),
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _primitives = primitives;
            _voice = voice;
        }
        
        /// <inheritdoc cref="Element"/>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            // Activator
            {
                _activator = _primitives.Activator(Schema);

                AddChild(_activator);
            }

            // create label
            {
                _labelProp = Schema.Get<string>("label");
                _labelProp.OnChanged += Label_OnChange;

                _fontSizeProp = Schema.Get<int>("fontSize");
                _fontSizeProp.OnChanged += FontSize_OnChanged;

                _labelPaddingProp = Schema.Get<float>("label.padding");
                _labelPaddingProp.OnChanged += LabelPadding_OnChanged;

                _text = _primitives.Text(Schema);
                _text.Parent = this;
                _text.Text = _labelProp.Value;

                UpdateLabelLayout();
            }

            // Voice Activator
            {
                _propVoiceActivator = Schema.Get<string>("voiceActivator");
                _propVoiceActivator.OnChanged += VoiceActivator_OnChange;
                
                RegisterVoiceCommand();
            }
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            base.UnloadInternal();

            // activator
            {
                _activator.Unload();
            }

            // cleanup label
            {
                _fontSizeProp.OnChanged -= FontSize_OnChanged;
                _labelProp.OnChanged -= Label_OnChange;
                _labelPaddingProp.OnChanged -= LabelPadding_OnChanged;
            }
            
            UnregisterVoiceCommand();
        }

        /// <summary>
        /// Registers voice command.
        /// </summary>
        private void RegisterVoiceCommand()
        {
            var voiceActivator = _propVoiceActivator.Value;
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
        /// Called when the label has been updated.
        /// </summary>
        /// <param name="prop">FontSize prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void FontSize_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            _text.FontSize = next;
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
        /// Updates label positioning.
        /// </summary>
        private void UpdateLabelLayout()
        {
            _text.Position = new Vec2(_labelPaddingProp.Value, 0f);
        }
    }
}
