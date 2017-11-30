using CreateAR.Commons.Unity.Messaging;
using UnityEngine.Windows.Speech;

namespace CreateAR.SpirePlayer.UI
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

        /// <summary>
        /// Text primitive.
        /// </summary>
        private TextPrimitive _text;

        /// <summary>
        /// Keyword recognizer for voice activation
        /// </summary>
        private KeywordRecognizer _keywordRecognizer;

        /// <summary>
        /// Activator primitive
        /// </summary>
        private ActivatorMonoBehaviour _activator;

        /// <summary>
        /// Activator Accessor
        /// </summary>
        public ActivatorMonoBehaviour Activator
        {
            get { return _activator; }
        }
        
        /// <summary>
        /// IInteractive interfaces.
        /// </summary>
        public bool Interactable { get { return _activator.Interactable; } }
        public float Aim { get { return _activator.Aim; } }
        public bool Raycast(Vec3 origin, Vec3 direction)
        {
            return _activator.Raycast(origin, direction);
        }
        public bool Focused
        {
            get { return _activator.Focused; }
            set { _activator.Focused = value; }
        }
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
        {
            _primitives = primitives;

            Initialize(config, layers, tweens, colors, messages);
        }
        
        /// <summary>
        /// Initialization
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            // Activator
            {
                _activator = FindOne<ActivatorMonoBehaviour>("activator");
            }

            // create label
            {
                _labelProp = Schema.Get<string>("label");
                _labelProp.OnChanged += Label_OnChange;

                _fontSizeProp = Schema.Get<int>("fontSize");
                _fontSizeProp.OnChanged += FontSize_OnChanged;

                _text = _primitives.Text();
                _text.Parent = this;
                _text.Text = _labelProp.Value;
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

        /// <summary>
        /// Destroy necessary items here
        /// </summary>
        protected override void UnloadInternal()
        {
            base.UnloadInternal();

            // cleanup label
            {
                _fontSizeProp.OnChanged -= FontSize_OnChanged;
                _fontSizeProp = null;

                _labelProp.OnChanged -= Label_OnChange;
                _labelProp = null;
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
    }
}
