using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Button
    /// </summary>
    public class Button : AimableWidget
    {
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _propVoiceActivator;

        /// <summary>
        /// Reflection of intention duration
        /// </summary>
        private float _activation;

        /// <summary>
        /// Caption to the button.
        /// </summary>
        private Caption _caption;

        /// <summary>
        /// Keyword recognizer for voice activation
        /// </summary>
        private KeywordRecognizer _keywordRecognizer;

        /// <summary>
        /// State management for the button
        /// </summary>
        private FiniteStateMachine _states;

        /// <summary>
        /// Activator primitive
        /// </summary>
        private IActivatorPrimitive _activator;
        
        /// <summary>
        /// Activation percentage
        /// </summary>
        public float Activation
        {
            get { return _activation; }
            set { _activation = value; }
        }

        /// <summary>
        /// Activator Accessor
        /// </summary>
        public IActivatorPrimitive Activator
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
        /// Initialization
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _activator = Primitives.LoadActivator(this);
            InteractivePrimitive = _activator;

            // Caption
            {
                _caption = Find("caption").FirstOrDefault() as Caption;
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

            // States
            {
                var buttonStates
                    = Find("states.*")
                        .Cast<IState>()
                        .ToArray();
                for (int i = 0, count = buttonStates.Length; i < count; ++i)
                {
                    var buttonState = buttonStates[i] as ButtonState;
                    if (buttonState != null)
                    {
                        buttonState.Initialize(this);
                    }
                }

                _states = new FiniteStateMachine(buttonStates);
                _states.Change<ButtonReadyState>();
            }
        }

        /// <summary>
        /// Changes the state
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal void ChangeState<T>() where T : ButtonState
        {
            _states.Change<T>();
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            var deltaTime = Time.smoothDeltaTime;
            _states.Update(deltaTime);

            UpdateActivator();
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
        /// Updates the aim visual
        /// </summary>
        private void UpdateActivator()
        {
            _activator.SetAimScale(Config.GetAimScale(Aim));
            _activator.SetAimColor(Config.GetAimColor(Aim));
            _activator.SetStabilityRotation(Stability * Config.StabilityRotation);
            _activator.SetActivationFill(_activation);
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
            if (IsInteractable 
            && _states.StateType != typeof(ButtonActivatingState))
            {
                _keywordRecognizer.Stop();
                _keywordRecognizer.Dispose();
                _states.Change<ButtonActivatedState>();
            }
        }
    }
}
