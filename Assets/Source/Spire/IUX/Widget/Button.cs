using System.Linq;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Button
    /// </summary>
    public class Button : Widget, IInteractive
    {
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _propVoiceActivator;

        /// <summary>
        /// Caption to the button.
        /// </summary>
        private Caption _caption;

        /// <summary>
        /// Keyword recognizer for voice activation
        /// </summary>
        private KeywordRecognizer _keywordRecognizer;

        /// <summary>
        /// Activator primitive
        /// </summary>
        private IActivator _activator;

        /// <summary>
        /// Activator Accessor
        /// </summary>
        public IActivator Activator
        {
            get { return _activator; }
        }

        /// <summary>
        /// Activator Accessor
        /// </summary>
        public Caption Caption
        {
            get { return _caption; }
        }

        /// <summary>
        /// IInteractive interfaces.
        /// </summary>
        public bool Interactable { get { return _activator.Interactable; } }
        public bool Cast(Ray ray) { return _activator.Cast(ray); }
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
        public void Initialize(
            IWidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IIntentionManager intention,
            IInteractionManager interaction)
        {
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
                _activator = FindOne("activator") as IActivator;
            }

            // Caption
            {
                _caption = FindOne("caption") as Caption;
            }

            // Voice Activator
            {
                _propVoiceActivator = Schema.Get<string>("voiceActivator");
                var voiceActivator = _propVoiceActivator.Value;
                if (string.IsNullOrEmpty(voiceActivator))
                {
                    if (_caption != null)
                    {
                        voiceActivator = _caption.Text.Text;
                    }
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

    }
}
