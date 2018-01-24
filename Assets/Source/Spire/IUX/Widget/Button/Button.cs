﻿using CreateAR.Commons.Unity.Messaging;
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
        /// Controls rendering changes based on button state.
        /// </summary>
        private readonly ButtonStateRenderer _stateRenderer;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _voiceActivatorProp;
        private ElementSchemaProp<string> _labelProp;
        private ElementSchemaProp<float> _labelPaddingProp;
        private ElementSchemaProp<string> _iconProp;

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
        public Button(
            WidgetConfig config,
            IPrimitiveFactory primitives,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
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
            _stateRenderer = new ButtonStateRenderer(tweens, colors, config, this);
        }
        
        /// <inheritdoc cref="Element"/>
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            // Activator
            {
                _activator = _primitives.Activator(Schema);
                AddChild(_activator);
                
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
        /// Updates label positioning.
        /// </summary>
        private void UpdateLabelLayout()
        {
            var textRect = _text.Rect;
            _text.LocalPosition = new Vector3(
                -textRect.min.x + _labelPaddingProp.Value,
                -(textRect.max.y - textRect.min.y) / 2,
                0f);
        }

        /// <summary>
        /// Updates the icon.
        /// </summary>
        private void UpdateIcon()
        {
            _activator.Icon = Config.Icons.Icon(_iconProp.Value);
        }
    }
}