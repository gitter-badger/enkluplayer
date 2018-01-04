using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.Windows.Speech;

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
        /// Keyword recognizer for voice activation
        /// </summary>
        private KeywordRecognizer _keywordRecognizer;

        /// <summary>
        /// Activator!
        /// </summary>
        private ActivatorPrimitive _activator;

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
            IMessageRouter messages)
            : base(
                new GameObject("Button"),
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

            // Activator
            {
                _activator = _primitives.Activator();

                var activatorSchema = new ElementSchema();
                activatorSchema.Wrap(Schema);
                _activator.Load(
                    new ElementData
                    {
                        Id = "Activator"
                    },
                    activatorSchema, 
                    new Element[0]);

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

                _text = _primitives.Text();
                _text.Parent = this;
                _text.Text = _labelProp.Value;

                UpdateLabelLayout();
            }

            // Voice Activator
            {
                _propVoiceActivator = Schema.Get<string>("voiceActivator");
                var voiceActivator = _propVoiceActivator.Value;
                if (string.IsNullOrEmpty(voiceActivator))
                {
                    voiceActivator = _labelProp.Value;
                }

                if (!string.IsNullOrEmpty(voiceActivator))
                {
                    StartKeywordRecognizer(voiceActivator);
                }
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

            if (_keywordRecognizer != null)
            {
                _keywordRecognizer.Stop();
                _keywordRecognizer.Dispose();
            }
        }
        
        /// <summary>
        /// Starts a KeywordRecognizer session to listen to key word or phrase
        /// </summary>
        /// <param name="str">Keyword or phrase to listen for</param>
        private void StartKeywordRecognizer(string str)
        {
            if (_keywordRecognizer != null)
            {
                _keywordRecognizer.Stop();
                _keywordRecognizer.Dispose();
            }

            _keywordRecognizer = new KeywordRecognizer(new[] { str });
            _keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            _keywordRecognizer.Start();
        }

        /// <summary>
        /// Invoked when a recognizable word or phrase is spoken
        /// </summary>
        /// <param name="args"></param>
        private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            if (_activator.Interactable)
            {
                _keywordRecognizer.Stop();
                _keywordRecognizer.Dispose();

                _activator.Activate();
            }
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
