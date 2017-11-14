using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Button
    /// </summary>
    public class Button : AimableWidget
    {
        /// <summary>
        /// Reflection of intention stability
        /// </summary>
        private float _stabilityDegrees;

        /// <summary>
        /// Reflection of intention duration
        /// </summary>
        private float _activation;

        /// <summary>
        /// Keyword recognizer for voice activation
        /// </summary>
        private KeywordRecognizer _keywordRecognizer;

        /// <summary>
        /// State management for the button
        /// </summary>
        private readonly FiniteStateMachine _states;

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
        /// Constructor.
        /// </summary>
        public Button()
        {
            var ready = new ButtonReadyState(this);
            ready.OnTransition += type => _states.Change(type);

            var activating = new ButtonActivatingState(this);
            activating.OnTransition += type => _states.Change(type);

            var activated = new ButtonActivatedState(this);
            activated.OnTransition += type => _states.Change(type);

            _states = new FiniteStateMachine(new IState[]
            {
                ready, activating, activated
            });

            _states.Change<ButtonReadyState>();
        }

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _activator = Primitives.LoadActivator(this);
            InteractivePrimitive = _activator;

            var voiceActivator = Schema.Get<string>("voiceActivator").Value;

            /*if (Caption != null)
            {
                if (Anchors != null)
                {
                    // TODO: this should be part of Element creation
                    Anchors.Anchor(Caption.GameObject.transform, schema.Caption.AnchorPosition);
                }

                Caption.SetSchema(schema.Caption);
            }

            if (string.IsNullOrEmpty(voiceActivator))
            {
                if (schema.Caption != null
                && !string.IsNullOrEmpty(schema.Caption.Text))
                {
                    // default to the caption text if there is no voice activator specified
                    voiceActivator = schema.Caption.Text;
                }
            }*/

            if (!string.IsNullOrEmpty(voiceActivator))
            {
                StartKeywordRecognizer(voiceActivator);
            }
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            var deltaTime = Time.smoothDeltaTime;
            _states.Update(deltaTime);

            UpdateAim();
            UpdateActivation();
            UpdateStability(deltaTime);
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
        private void UpdateAim()
        {
            _activator.SetAimScale(Config.GetAimScale(Aim));
            _activator.SetAimColor(Config.GetAimColor(Aim));
        }

        /// <summary>
        /// Updates the steadiness feedback
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdateStability(float deltaTime)
        {
            var targetStabilityDegrees 
                = IsFocused 
                ? Config.StabilityRotation * Intention.Stability 
                : 0.0f;

            const float STABILITY_LERP_RATE_MAGIC_NUMBER = 8.0f;
            var lerp = deltaTime * STABILITY_LERP_RATE_MAGIC_NUMBER;
            _stabilityDegrees
                = Mathf.Lerp(
                    _stabilityDegrees, 
                    targetStabilityDegrees,
                    lerp);

            _activator.SetStabilityRotation(_stabilityDegrees);
        }
        
        /// <summary>
        /// Updates visual activation feedback.
        /// </summary>
        private void UpdateActivation()
        {
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
